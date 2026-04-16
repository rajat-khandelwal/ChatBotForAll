using ChatBotForAll.ApiService.Entities;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> AddAsync(Message message);
        Task<List<Message>> GetByConversationAsync(Guid tenantId, Guid conversationId);
    }
}
