using ChatBotForAll.ApiService.Models.Auth;

namespace ChatBotForAll.ApiService.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<UserResponse?> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse?> GetUserAsync(Guid userId, Guid tenantId);
    }
}
