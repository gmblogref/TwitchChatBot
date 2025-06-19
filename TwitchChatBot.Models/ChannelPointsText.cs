using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class ChannelPointsText
    {
        [JsonPropertyName("tiers")]
        public List<Tier> Tiers { get; set; } = new();
    }
}
