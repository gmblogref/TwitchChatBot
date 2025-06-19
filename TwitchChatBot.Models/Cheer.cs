using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class Cheer
    {
        [JsonPropertyName("tiers")]
        public List<Tier> tiers { get; set; } = new();

        [JsonPropertyName("default")]
        public string Default { get; set; } = string.Empty;
    }
}
