using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Chat
{
    public record MessageHistory(MessageRole Role, string Content);

    public class RagResult
    {
        public string Answer { get; set; }
        public string? CitationsJson { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int LatencyMs { get; set; }
    }
}
