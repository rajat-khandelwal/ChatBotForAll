using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation> AddAsync(Conversation conversation);
        Task<Conversation?> GetByIdAsync(Guid tenantId, Guid conversationId);
        Task<Conversation?> GetByIdWithMessagesAsync(Guid tenantId, Guid conversationId);
        Task<List<Conversation>> GetAllAsync(Guid tenantId, Guid? userId);
        Task UpdateAsync(Conversation conversation);
        Task DeleteAsync(Conversation conversation);
    }
}
