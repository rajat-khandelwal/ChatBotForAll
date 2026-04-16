using ChatBotForAll.ApiService.Entities;
using ChatBotForAll.ApiService.Enums;
using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Documents;

namespace ChatBotForAll.ApiService.Services
{
    public class DocumentService : IDocumentService
    {
        private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB
        private static readonly HashSet<string> AllowedExtensions = [".pdf", ".txt", ".md"];
        private static readonly HashSet<string> AllowedContentTypes =
        [
            "application/pdf",
            "text/plain",
            "text/markdown",
            "text/x-markdown"
        ];

        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorageService _fileStorageService;

        public DocumentService(IDocumentRepository documentRepository, IFileStorageService fileStorageService)
        {
            _documentRepository = documentRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<DocumentResponse> UploadAsync(IFormFile file, Guid tenantId, Guid uploadedByUserId)
        {
            ValidateFile(file);

            var documentId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var storagePath = await _fileStorageService.SaveAsync(tenantId, documentId, file.FileName, file.OpenReadStream());

            var document = new Document
            {
                DocumentId = documentId,
                TenantId = tenantId,
                UploadedByUserId = uploadedByUserId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                StoragePath = storagePath,
                SizeBytes = file.Length,
                DocumentStatus = DocumentStatus.Uploaded,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                CreatedBy = uploadedByUserId.ToString(),
                UpdatedBy = uploadedByUserId.ToString()
            };

            var created = await _documentRepository.AddAsync(document);
            return MapToResponse(created);
        }

        public async Task<List<DocumentResponse>> GetAllAsync(Guid tenantId)
        {
            var documents = await _documentRepository.GetAllByTenantAsync(tenantId);
            return documents.Select(MapToResponse).ToList();
        }

        public async Task<DocumentResponse?> GetByIdAsync(Guid tenantId, Guid documentId)
        {
            var document = await _documentRepository.GetByIdAsync(tenantId, documentId);
            return document is null ? null : MapToResponse(document);
        }

        public async Task<bool> DeleteAsync(Guid tenantId, Guid documentId)
        {
            var document = await _documentRepository.GetByIdAsync(tenantId, documentId);
            if (document is null)
            {
                return false;
            }

            await _fileStorageService.DeleteAsync(document.StoragePath);
            await _documentRepository.DeleteAsync(document);
            return true;
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new InvalidOperationException("Uploaded file is empty.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}.");
            }
        }

        private static DocumentResponse MapToResponse(Document doc)
        {
            return new DocumentResponse
            {
                DocumentId = doc.DocumentId,
                TenantId = doc.TenantId,
                UploadedByUserId = doc.UploadedByUserId,
                FileName = doc.FileName,
                ContentType = doc.ContentType,
                SizeBytes = doc.SizeBytes,
                Status = doc.DocumentStatus,
                ErrorMessage = doc.ErrorMessage,
                CreatedDateTime = doc.CreatedDateTime,
                UpdatedDateTime = doc.UpdatedDateTime
            };
        }
    }
}
