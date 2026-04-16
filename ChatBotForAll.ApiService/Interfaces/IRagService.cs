using ChatBotForAll.ApiService.Models.Chat;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IRagService
    {
        Task<RagResult> GetAnswerAsync(Guid tenantId, string question, IList<MessageHistory> history);
    }
}
