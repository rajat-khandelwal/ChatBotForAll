using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Chat
{
    public class MessageResponse
    {
        public Guid MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public MessageRole Role { get; set; }
        public string Content { get; set; }
        public string? CitationsJson { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? LatencyMs { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }
}
