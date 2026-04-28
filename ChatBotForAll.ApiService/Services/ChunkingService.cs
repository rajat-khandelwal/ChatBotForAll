using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Documents;

namespace ChatBotForAll.ApiService.Services
{
    public class ChunkingService : IChunkingService
    {
        private const int DefaultOverlapTokens = 100;

        public Task<List<DocumentChunkDto>> ChunkTextAsync(string text, Guid documentId, Guid tenantId, int maxChunkSize = 1000)
        {
            var chunks = new List<DocumentChunkDto>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult(chunks);
            }

            // Split by sentences/paragraphs and then by token count
            var sentences = text.Split(new[] { ". ", ".\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            var currentChunk = new List<string>();
            var currentTokenCount = 0;
            int chunkIndex = 0;

            foreach (var sentence in sentences)
            {
                var sentenceTokenCount = EstimateTokenCount(sentence);

                // If adding this sentence exceeds max size, save current chunk and start new one
                if (currentTokenCount + sentenceTokenCount > maxChunkSize && currentChunk.Count > 0)
                {
                    var chunkText = string.Join(" ", currentChunk).Trim();
                    chunks.Add(new DocumentChunkDto
                    {
                        DocumentId = documentId,
                        TenantId = tenantId,
                        ChunkIndex = chunkIndex++,
                        Text = chunkText,
                        TokenCount = currentTokenCount,
                        MetadataJson = null
                    });

                    // Start new chunk with overlap (keep last few sentences)
                    currentChunk.Clear();
                    if (chunks.Count > 0)
                    {
                        var lastSentences = chunkText.Split(". ").TakeLast(2).ToList();
                        currentChunk.AddRange(lastSentences);
                        currentTokenCount = lastSentences.Sum(s => EstimateTokenCount(s)) + DefaultOverlapTokens;
                    }
                    else
                    {
                        currentTokenCount = 0;
                    }
                }

                currentChunk.Add(sentence);
                currentTokenCount += sentenceTokenCount;
            }

            // Add remaining chunk
            if (currentChunk.Count > 0)
            {
                var chunkText = string.Join(" ", currentChunk).Trim();
                chunks.Add(new DocumentChunkDto
                {
                    DocumentId = documentId,
                    TenantId = tenantId,
                    ChunkIndex = chunkIndex,
                    Text = chunkText,
                    TokenCount = currentTokenCount,
                    MetadataJson = null
                });
            }

            return Task.FromResult(chunks);
        }

        private static int EstimateTokenCount(string text)
        {
            // Rough estimate: ~4 characters per token (OpenAI approximation)
            return (int)Math.Ceiling(text.Length / 4.0);
        }
    }
}
