using ChatBotForAll.ApiService.Models.Chat;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IChatService
    {
        Task<ConversationResponse> StartConversationAsync(Guid tenantId, Guid? userId, StartConversationRequest request);
        Task<List<ConversationResponse>> GetAllConversationsAsync(Guid tenantId, Guid? userId);
        Task<ConversationResponse?> GetConversationAsync(Guid tenantId, Guid conversationId);
        Task<AskResponse> AskAsync(Guid tenantId, Guid conversationId, AskRequest request);
        Task<bool> DeleteConversationAsync(Guid tenantId, Guid conversationId);
    }
}
