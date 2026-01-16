using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class ChannelPoints
    {
        [JsonPropertyName("tiers")]
        public List<Tier> Tiers { get; set; } = new();


        [JsonPropertyName("default")]
        public string Default { get; set; } = string.Empty;
    }
}
