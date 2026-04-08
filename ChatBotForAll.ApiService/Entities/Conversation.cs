namespace ChatBotForAll.ApiService.Entities
{
    public class Conversation : DefaultColumns
    {
        public Guid ConversationId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string? ExternalSessionId { get; set; }
        public string? Title { get; set; }
    }
}
