using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class Tier
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("media")]
        public string? Media { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("min")] 
        public int? Min { get; set; }

        [JsonPropertyName("match")]
        public string? Match { get; set; }

        [JsonPropertyName("value")]
        public int? Value { get; set; }
    }
}
