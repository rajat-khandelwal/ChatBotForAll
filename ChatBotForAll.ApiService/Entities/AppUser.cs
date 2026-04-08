using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Entities
{
    public class AppUser : DefaultColumns
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        //unique per tenant or global by policy
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.TenantUser;
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
