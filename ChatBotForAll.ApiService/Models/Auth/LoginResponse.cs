using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
    }
}
