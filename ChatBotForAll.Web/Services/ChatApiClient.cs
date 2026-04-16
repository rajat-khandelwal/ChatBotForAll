using ChatBotForAll.Web.Models.Chat;
using System.Net.Http.Headers;

namespace ChatBotForAll.Web.Services
{
    public class ChatApiClient(HttpClient httpClient, TokenStore tokenStore)
    {
        private void AttachToken()
        {
            if (tokenStore.CurrentUser?.Token is { } token)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ConversationResponse>> GetConversationsAsync()
        {
            AttachToken();
            var response = await httpClient.GetAsync("api/chat/conversations");
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<ConversationResponse>>() ?? [];
        }

        public async Task<ConversationResponse?> StartConversationAsync(string? title = null)
        {
            AttachToken();
            var response = await httpClient.PostAsJsonAsync("api/chat/conversations", new { Title = title });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ConversationResponse>();
        }

        public async Task<ConversationResponse?> GetConversationAsync(Guid conversationId)
        {
            AttachToken();
            var response = await httpClient.GetAsync($"api/chat/conversations/{conversationId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ConversationResponse>();
        }

        public async Task<AskResponse?> AskAsync(Guid conversationId, string question)
        {
            AttachToken();
            var response = await httpClient.PostAsJsonAsync($"api/chat/conversations/{conversationId}/ask", new { Question = question });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AskResponse>();
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId)
        {
            AttachToken();
            var response = await httpClient.DeleteAsync($"api/chat/conversations/{conversationId}");
            return response.IsSuccessStatusCode;
        }
    }
}
