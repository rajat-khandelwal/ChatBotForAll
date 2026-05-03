using Azure;
using Azure.AI.OpenAI;
using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Chat;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Pgvector;
using System.Text.Json;

namespace ChatBotForAll.ApiService.Services
{
    /// <summary>
    /// RAG service that retrieves relevant document chunks and generates answers using Azure OpenAI.
    /// </summary>
    public class RagService : IRagService
    {
        private readonly ChatBotDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly AzureOpenAIClient _openAiClient;
        private readonly string _chatDeploymentName;
        private readonly ILogger<RagService> _logger;

        private const int TopK = 5; // Number of relevant chunks to retrieve
        private const float SimilarityThreshold = 0.3f; // Minimum similarity score

        public RagService(
            ChatBotDbContext context,
            IEmbeddingService embeddingService,
            IConfiguration configuration,
            ILogger<RagService> logger)
        {
            _context = context;
            _embeddingService = embeddingService;
            _logger = logger;

            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _chatDeploymentName = configuration["AzureOpenAI:ChatDeploymentName"] ?? "o4-mini";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing.");
            }

            _openAiClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<RagResult> GetAnswerAsync(Guid tenantId, string question, IList<MessageHistory> history)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"RAG: Processing question for tenant {tenantId}: {question}");

                // Step 1: Generate embedding for the question
                var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);
                if (questionEmbedding == null || questionEmbedding.Length == 0)
                {
                    _logger.LogWarning("Failed to generate embedding for question");
                    return CreateErrorResult("Failed to process question", 0, 0);
                }

                // Step 2: Retrieve relevant document chunks using vector similarity
                var relevantChunks = await RetrieveSimilarChunksAsync(tenantId, questionEmbedding);
                _logger.LogInformation($"RAG: Retrieved {relevantChunks.Count} relevant chunks");

                // Step 3: Build context from chunks
                var contextText = BuildContext(relevantChunks);

                // Step 4: Generate answer using LLM with context
                var (answer, promptTokens, completionTokens) = await GenerateAnswerAsync(question, contextText, history);

                // Step 5: Extract citations
                var citationsJson = ExtractCitations(relevantChunks);

                stopwatch.Stop();

                return new RagResult
                {
                    Answer = answer,
                    CitationsJson = citationsJson,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    LatencyMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RAG pipeline: {ex.Message}");
                stopwatch.Stop();
                return CreateErrorResult(ex.Message, 0, 0);
            }
        }

        private async Task<List<ChunkWithScore>> RetrieveSimilarChunksAsync(Guid tenantId, float[] questionEmbedding)
        {
            try
            {
                var questionVector = new Vector(questionEmbedding);

                // Query chunks with vector similarity using pgvector
                // Filter embeddings by tenant AND ensure vector is not null
                var allEmbeddings = await _context.ChunkEmbeddings
                    .Where(x => x.TenantId == tenantId && x.Vector != null)
                    .ToListAsync();

                _logger.LogInformation($"RAG: Found {allEmbeddings.Count} embeddings for tenant {tenantId}");

                if (allEmbeddings.Count == 0)
                {
                    _logger.LogWarning($"RAG: No embeddings found for tenant {tenantId}. Ensure documents have been processed.");
                    return new List<ChunkWithScore>();
                }

                // Get the chunk details
                var chunkIds = allEmbeddings.Select(x => x.DocumentChunkId).ToList();
                var chunks = await _context.DocumentChunks
                    .Where(x => x.TenantId == tenantId && chunkIds.Contains(x.DocumentChunkId))
                    .ToListAsync();

                _logger.LogInformation($"RAG: Retrieved {chunks.Count} document chunks for {chunkIds.Count} embeddings");

                // Calculate similarity in memory and sort
                var allResults = allEmbeddings
                    .Select(embedding => new
                    {
                        Embedding = embedding,
                        Similarity = CalculateCosineSimilarity(embedding.Vector, questionVector)
                    })
                    .OrderByDescending(x => x.Similarity)
                    .ToList();

                _logger.LogInformation($"RAG: Top similarity scores: {string.Join(", ", allResults.Take(5).Select(x => $"{x.Similarity:F4}"))}");

                var results = allResults
                    .Take(TopK)
                    .Select(x =>
                    {
                        var documentChunk = chunks.FirstOrDefault(c => c.DocumentChunkId == x.Embedding.DocumentChunkId);
                        if (documentChunk == null)
                        {
                            _logger.LogWarning($"RAG: Document chunk {x.Embedding.DocumentChunkId} not found");
                            return null;
                        }

                        if (x.Similarity >= SimilarityThreshold)
                        {
                            return new ChunkWithScore
                            {
                                ChunkId = documentChunk.DocumentChunkId,
                                DocumentId = documentChunk.DocumentId,
                                ChunkIndex = documentChunk.ChunkIndex,
                                Text = documentChunk.Text,
                                SimilarityScore = x.Similarity
                            };
                        }

                        _logger.LogInformation($"RAG: Chunk {documentChunk.DocumentChunkId} filtered out (similarity: {x.Similarity:F4} < threshold: {SimilarityThreshold:F4})");
                        return null;
                    })
                    .Where(x => x != null)
                    .ToList();

                _logger.LogInformation($"RAG: Returning {results.Count} chunks that meet similarity threshold");
                return results!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving similar chunks: {ex.Message}");
                throw;
            }
        }

        private float CalculateCosineSimilarity(Vector vec1, Vector vec2)
        {
            try
            {
                // Convert vectors to float arrays for similarity calculation
                var arr1 = vec1.ToArray();
                var arr2 = vec2.ToArray();

                if (arr1.Length == 0 || arr2.Length == 0)
                    return 0;

                // Cosine similarity: (A · B) / (||A|| * ||B||)
                double dotProduct = 0;
                double normA = 0;
                double normB = 0;

                for (int i = 0; i < arr1.Length; i++)
                {
                    dotProduct += arr1[i] * arr2[i];
                    normA += arr1[i] * arr1[i];
                    normB += arr2[i] * arr2[i];
                }

                if (normA == 0 || normB == 0)
                    return 0;

                return (float)(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating cosine similarity: {ex.Message}");
                return 0;
            }
        }

        private string BuildContext(List<ChunkWithScore> chunks)
        {
            if (chunks.Count == 0)
            {
                return "No relevant documents found.";
            }

            var contextParts = new List<string> { "## Relevant Context from Documents:\n" };

            foreach (var chunk in chunks)
            {
                contextParts.Add($"### Chunk {chunk.ChunkIndex} (relevance: {chunk.SimilarityScore:P})");
                contextParts.Add(chunk.Text);
                contextParts.Add("");
            }

            return string.Join("\n", contextParts);
        }

        private async Task<(string answer, int promptTokens, int completionTokens)> GenerateAnswerAsync(
            string question,
            string context,
            IList<MessageHistory> history)
        {
            try
            {
                var systemMessage = @"You are a helpful AI assistant that answers questions based on provided documents.

When answering:
1. Use the provided context to answer accurately
2. If the context doesn't contain relevant information, say so clearly
3. Be concise and direct
4. Cite the relevant document sections when applicable";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemMessage),
                };

                // Add conversation history
                foreach (var msg in history)
                {
                    if (msg.Role == Enums.MessageRole.User)
                    {
                        messages.Add(new UserChatMessage(msg.Content));
                    }
                    else
                    {
                        messages.Add(new AssistantChatMessage(msg.Content));
                    }
                }

                // Add context and current question
                var userMessage = $"Context:\n{context}\n\nQuestion: {question}";
                messages.Add(new UserChatMessage(userMessage));

                var chatClient = _openAiClient.GetChatClient(_chatDeploymentName);
                var response = await chatClient.CompleteChatAsync(messages);

                var answer = response.Value.Content.FirstOrDefault()?.Text ?? "Unable to generate answer";
                var promptTokens = (int)(response.Value.Usage?.InputTokenCount ?? 0);
                var completionTokens = (int)(response.Value.Usage?.OutputTokenCount ?? 0);

                _logger.LogInformation($"Generated answer - prompt tokens: {promptTokens}, completion tokens: {completionTokens}");

                return (answer, promptTokens, completionTokens);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating answer: {ex.Message}");
                throw;
            }
        }

        private string? ExtractCitations(List<ChunkWithScore> chunks)
        {
            if (chunks.Count == 0)
            {
                return null;
            }

            try
            {
                var citations = chunks
                    .OrderByDescending(x => x.SimilarityScore)
                    .Select(x => new
                    {
                        ChunkId = x.ChunkId,
                        DocumentId = x.DocumentId,
                        ChunkIndex = x.ChunkIndex,
                        RelevanceScore = x.SimilarityScore
                    })
                    .ToList();

                return JsonSerializer.Serialize(citations);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error serializing citations: {ex.Message}");
                return null;
            }
        }

        private RagResult CreateErrorResult(string message, int promptTokens, int completionTokens)
        {
            return new RagResult
            {
                Answer = $"I apologize, but I encountered an error processing your question. Please try again later. (Error: {message})",
                CitationsJson = null,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                LatencyMs = 0
            };
        }

        private class ChunkWithScore
        {
            public Guid ChunkId { get; set; }
            public Guid DocumentId { get; set; }
            public int ChunkIndex { get; set; }
            public string Text { get; set; }
            public float SimilarityScore { get; set; }
        }
    }
}
