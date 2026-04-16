using System.Text.Json.Serialization;

namespace ChatBotForAll.ApiService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageRole
    {
        User = 0,
        Assistant = 1,
        System = 2
    }
}
