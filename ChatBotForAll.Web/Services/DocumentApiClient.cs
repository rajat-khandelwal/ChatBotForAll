using ChatBotForAll.Web.Models.Documents;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace ChatBotForAll.Web.Services
{
    public class DocumentApiClient(HttpClient httpClient, TokenStore tokenStore)
    {
        private void AttachToken()
        {
            if (tokenStore.CurrentUser?.Token is { } token)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<DocumentResponse>> GetDocumentsAsync()
        {
            AttachToken();
            var response = await httpClient.GetAsync("api/document");
            if (!response.IsSuccessStatusCode) return [];
            return await response.Content.ReadFromJsonAsync<List<DocumentResponse>>() ?? [];
        }

        public async Task<(bool Success, string? Error)> UploadAsync(IBrowserFile file)
        {
            AttachToken();
            await using var stream = file.OpenReadStream(20 * 1024 * 1024);
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            var response = await httpClient.PostAsync("api/document/upload", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }

            return (true, null);
        }

        public async Task<bool> DeleteAsync(Guid documentId)
        {
            AttachToken();
            var response = await httpClient.DeleteAsync($"api/document/{documentId}");
            return response.IsSuccessStatusCode;
        }
    }
}
