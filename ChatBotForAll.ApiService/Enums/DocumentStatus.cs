using System.Text.Json.Serialization;

namespace ChatBotForAll.ApiService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentStatus
    {
        Uploaded = 0,
        Processing = 1,
        Indexed = 2,
        Failed = 3
    }
}
