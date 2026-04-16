namespace ChatBotForAll.Web.Models.Chat
{
    public class MessageResponse
    {
        public Guid MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? CitationsJson { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }
}
