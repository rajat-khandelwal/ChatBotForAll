using ChatBotForAll.ApiService.Data;
using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IDocumentChunkRepository
    {
       

        public Task<DocumentChunk> AddAsync(DocumentChunk chunks);

        public Task AddRangeAsync(IEnumerable<DocumentChunk> chunks);  

        public Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid tenantId,Guid documentId);

        public Task DeleteByDocumentIdAsync(Guid tenantId, Guid documentId);

    }
}
