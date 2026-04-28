using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IDocumentProcessingService
    {
        Task<List<DocumentChunk>> ProcessDocumentAsync(Guid tenantId, Guid documentId);
    }
}
