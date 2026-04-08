namespace ChatBotForAll.ApiService.Models.Auth
{
    public class LoginRequest
    {
        public Guid TenantId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
