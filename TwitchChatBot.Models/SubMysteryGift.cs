using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class SubMysteryGift
    {
        [JsonPropertyName("tiers")]
        public List<Tier>? tiers { get; set; }

        [JsonPropertyName("default")]
        public string? Default { get; set; }
    }
}
