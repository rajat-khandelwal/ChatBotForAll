namespace ChatBotForAll.ApiService.Models.Auth
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "ChatBotForAll";
        public string Audience { get; set; } = "ChatBotForAllClients";
        public string Key { get; set; } = "ChangeThisToASecretKeyWithAtLeast32Chars";
        public int ExpirationMinutes { get; set; } = 60;
    }
}
