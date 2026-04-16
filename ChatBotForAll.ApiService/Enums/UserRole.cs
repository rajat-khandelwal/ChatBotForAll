using System.Text.Json.Serialization;

namespace ChatBotForAll.ApiService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        PlatformAdmin = 0,
        TenantAdmin = 1,
        TenantUser = 2
    }
}
