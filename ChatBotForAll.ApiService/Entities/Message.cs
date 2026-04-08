using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Entities
{
    public class Message : DefaultColumns
    {
        public Guid MessageId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ConversationId { get; set; }
        public MessageRole Role { get; set; } = MessageRole.User;
        public string Content { get; set; }
        public string? CitationsJson { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? LatencyMs { get; set; }
    }
}
