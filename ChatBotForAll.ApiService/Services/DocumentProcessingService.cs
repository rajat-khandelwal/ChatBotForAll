using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Enums;
using ChatBotForAll.ApiService.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Pgvector;
using System.Text;

namespace ChatBotForAll.ApiService.Services
{
    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentChunkRepository _chunkRepository;
        private readonly ChatBotDbContext _context;
        private readonly ILogger<DocumentProcessingService> _logger;

        public DocumentProcessingService(
            IFileStorageService fileStorageService,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IDocumentChunkRepository chunkRepository,
            IDocumentRepository documentRepository,
            ChatBotDbContext context,
            ILogger<DocumentProcessingService> logger)
        {
            _fileStorageService = fileStorageService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _chunkRepository = chunkRepository;
            _context = context;
            _logger = logger;
            _documentRepository = documentRepository;
        }

        public async Task<List<DocumentChunk>> ProcessDocumentAsync(Guid tenantId, Guid documentId)
        {
            try
            {
                _logger.LogInformation($"Starting document processing: TenantId={tenantId}, DocumentId={documentId}");

                // Get document from database
                var document = await _documentRepository.GetByIdAsync(tenantId, documentId);

                if (document == null)
                {
                    _logger.LogError($"Document not found: TenantId={tenantId}, DocumentId={documentId}");
                    throw new InvalidOperationException("Document not found");
                }

                // Update document status to Processing
                document.DocumentStatus = DocumentStatus.Processing;
                document.UpdatedDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Read file content
                var fileContent = await _fileStorageService.ReadAsync(document.StoragePath);

                // Extract text based on content type
                var extractedText = ExtractTextFromContent(fileContent, document.ContentType);

                // Chunk the text
                var chunks = await _chunkingService.ChunkTextAsync(extractedText, documentId, tenantId);

                _logger.LogInformation($"Document chunked into {chunks.Count} chunks");

                // Create DocumentChunk entities and generate embeddings
                var documentChunks = new List<DocumentChunk>();

                foreach (var chunkDto in chunks)
                {
                    var documentChunk = new DocumentChunk
                    {
                        DocumentChunkId = Guid.NewGuid(),
                        TenantId = tenantId,
                        DocumentId = documentId,
                        ChunkIndex = chunkDto.ChunkIndex,
                        Text = chunkDto.Text,
                        TokenCount = chunkDto.TokenCount,
                        MetadataJson = chunkDto.MetadataJson,
                        CreatedDateTime = DateTime.UtcNow,
                        CreatedBy = document.UploadedByUserId.ToString(),
                        UpdatedBy = document.UploadedByUserId.ToString(),
                        UpdatedDateTime = DateTime.UtcNow
                    };

                    documentChunks.Add(documentChunk);
                }

                // Save chunks to database
                await _chunkRepository.AddRangeAsync(documentChunks);

                _logger.LogInformation($"Saved {documentChunks.Count} chunks to database");

                // Generate embeddings for each chunk
                var chunkEmbeddings = new List<ChunkEmbedding>();

                for (int i = 0; i < documentChunks.Count; i++)
                {
                    try
                    {
                        var embedding = await _embeddingService.GetEmbeddingAsync(documentChunks[i].Text);

                        chunkEmbeddings.Add(new ChunkEmbedding
                        {
                            ChunkEmbeddingId = Guid.NewGuid(),
                            TenantId = tenantId,
                            DocumentChunkId = documentChunks[i].DocumentChunkId,
                            Model = "text-embedding-3-small",
                            Vector = new Vector(embedding),
                            CreatedDateTime = DateTime.UtcNow,
                            CreatedBy = document.UploadedByUserId.ToString(),
                            UpdatedBy = document.UploadedByUserId.ToString(),
                            UpdatedDateTime = DateTime.UtcNow
                        });

                        _logger.LogInformation($"Generated embedding for chunk {i + 1}/{documentChunks.Count}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error generating embedding for chunk {i}: {ex.Message}");
                        throw;
                    }
                }

                // Save embeddings to database
                _context.ChunkEmbeddings.AddRange(chunkEmbeddings);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Saved {chunkEmbeddings.Count} embeddings to database");

                // Update document status to Indexed
                document.DocumentStatus = DocumentStatus.Indexed;
                document.UpdatedDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Document processing completed successfully");

                return documentChunks;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document: {ex.Message}");

                // Update document status to Failed
                var document = await _documentRepository.GetByIdAsync(tenantId, documentId);

                if (document != null)
                {
                    document.DocumentStatus = DocumentStatus.Failed;
                    document.ErrorMessage = ex.Message;
                    document.UpdatedDateTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                throw;
            }
        }

        private string ExtractTextFromContent(string content, string contentType)
        {
            return contentType switch
            {
                "text/plain" => content,
                "text/markdown" or "text/x-markdown" => content,
                "application/pdf" => ExtractTextFromPdf(content),
                _ => content
            };
        }

        private string ExtractTextFromPdf(string base64PdfContent)
        {
            try
            {
                // Decode base64 string to bytes
                var pdfBytes = Convert.FromBase64String(base64PdfContent);

                using (var pdfStream = new MemoryStream(pdfBytes))
                {
                    using (var pdfReader = new PdfReader(pdfStream))
                    {
                        using (var pdfDocument = new PdfDocument(pdfReader))
                        {
                            var extractedText = new StringBuilder();
                            var pageCount = pdfDocument.GetNumberOfPages();

                            for (int i = 1; i <= pageCount; i++)
                            {
                                try
                                {
                                    var page = pdfDocument.GetPage(i);
                                    var textExtractor = new SimpleTextExtractionStrategy();
                                    var pageText = PdfTextExtractor.GetTextFromPage(page, textExtractor);

                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        extractedText.AppendLine($"--- Page {i} ---");
                                        extractedText.AppendLine(pageText);
                                        extractedText.AppendLine();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning($"Failed to extract text from page {i}: {ex.Message}");
                                    // Continue with next page instead of failing completely
                                }
                            }

                            var result = extractedText.ToString().Trim();

                            if (string.IsNullOrWhiteSpace(result))
                            {
                                _logger.LogWarning("No text content extracted from PDF");
                                return "[PDF document contains no extractable text]";
                            }

                            _logger.LogInformation($"Successfully extracted text from PDF ({pageCount} pages)");
                            return result;
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                _logger.LogError($"Invalid base64 PDF content: {ex.Message}");
                throw new InvalidOperationException("Failed to decode PDF content - invalid base64 format", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting text from PDF: {ex.Message}");
                throw new InvalidOperationException("Failed to extract text from PDF", ex);
            }
        }
    }
}
