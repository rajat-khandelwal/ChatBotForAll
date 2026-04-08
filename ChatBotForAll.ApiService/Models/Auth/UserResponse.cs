using ChatBotForAll.ApiService.Enums;

namespace ChatBotForAll.ApiService.Models.Auth
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
