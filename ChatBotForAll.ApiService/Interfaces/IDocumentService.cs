using ChatBotForAll.ApiService.Models.Documents;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadAsync(IFormFile file, Guid tenantId, Guid uploadedByUserId);
        Task<List<DocumentResponse>> GetAllAsync(Guid tenantId);
        Task<DocumentResponse?> GetByIdAsync(Guid tenantId, Guid documentId);
        Task<bool> DeleteAsync(Guid tenantId, Guid documentId);
    }
}
