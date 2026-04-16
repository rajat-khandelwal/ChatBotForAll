using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document> AddAsync(Document document);
        Task<Document?> GetByIdAsync(Guid tenantId, Guid documentId);
        Task<List<Document>> GetAllByTenantAsync(Guid tenantId);
        Task UpdateAsync(Document document);
        Task DeleteAsync(Document document);
    }
}
