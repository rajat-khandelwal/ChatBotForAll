using ChatBotForAll.Web.Models.Auth;

namespace ChatBotForAll.Web.Services
{
    public class AuthApiClient(HttpClient httpClient)
    {
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task LogoutAsync()
        {
            await httpClient.PostAsJsonAsync("api/auth/logout", new { });
        }
    }
}
