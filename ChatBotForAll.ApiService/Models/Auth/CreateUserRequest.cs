using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Auth
{
    public class CreateUserRequest
    {
        public Guid TenantId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; } = UserRole.TenantUser;
    }
}
