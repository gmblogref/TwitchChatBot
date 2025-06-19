using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class Tier
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("media")]
        public string Media { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("min")] 
        public int? Min { get; set; }

        [JsonPropertyName("match")]
        public string Match { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public int? Value { get; set; }
    }
}
