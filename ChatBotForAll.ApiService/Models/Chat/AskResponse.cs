namespace ChatBotForAll.ApiService.Models.Chat
{
    public class AskResponse
    {
        public Guid MessageId { get; set; }
        public string Answer { get; set; }
        public string? CitationsJson { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? LatencyMs { get; set; }
    }
}
