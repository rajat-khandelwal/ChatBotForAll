using ChatBotForAll.ApiService.Interfaces;
using ChatBotForAll.ApiService.Models.Chat;

namespace ChatBotForAll.ApiService.Services
{
    /// <summary>
    /// Stub RAG service — returns a placeholder until the real pipeline (embeddings + LLM) is wired in Day 5-6.
    /// </summary>
    public class StubRagService : IRagService
    {
        public Task<RagResult> GetAnswerAsync(Guid tenantId, string question, IList<MessageHistory> history)
        {
            var result = new RagResult
            {
                Answer = $"[RAG pipeline not yet implemented] You asked: \"{question}\"",
                CitationsJson = null,
                PromptTokens = 0,
                CompletionTokens = 0,
                LatencyMs = 0
            };

            return Task.FromResult(result);
        }
    }
}
