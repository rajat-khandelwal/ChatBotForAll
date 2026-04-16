namespace ChatBotForAll.ApiService.Models.Chat
{
    public class ConversationResponse
    {
        public Guid ConversationId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string? Title { get; set; }
        public string? ExternalSessionId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public List<MessageResponse> Messages { get; set; } = [];
    }
}
