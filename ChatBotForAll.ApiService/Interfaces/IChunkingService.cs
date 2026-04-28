using ChatBotForAll.ApiService.Models.Documents;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IChunkingService
    {
        Task<List<DocumentChunkDto>> ChunkTextAsync(string text, Guid documentId, Guid tenantId, int maxChunkSize = 1000);
    }
}
