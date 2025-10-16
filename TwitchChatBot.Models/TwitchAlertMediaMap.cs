using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchChatBot.Models
{
    public class TwitchAlertMediaMap
    {
        [JsonPropertyName("channel_points")]
        public ChannelPoints Channel_Points { get; set; } = new();

        [JsonPropertyName("channel_points_text")]
        public ChannelPointsText Channel_Points_Text { get; set; } = new();

        [JsonPropertyName("cheer")] 
        public Cheer Cheer { get; set; } = new();

        [JsonPropertyName("follow")]
        public List<string> Follow { get; set; } = new();

        [JsonPropertyName("hype_train")] 
        public List<string> Hype_Train { get; set; } = new();

        [JsonPropertyName("raid")] 
        public List<string> Raid { get; set; } = new();

        [JsonPropertyName("resub")] 
        public List<string> Resub { get; set; } = new();

        [JsonPropertyName("subgift")] 
        public List<string> Subgift { get; set; } = new();

        [JsonPropertyName("submysterygift")] 
        public SubMysteryGift Submysterygift { get; set; } = new();

        [JsonPropertyName("subscription")] 
        public List<string> Subscription { get; set; } = new();

        [JsonPropertyName("watch_streak")]
        public List<string> WatchStreak { get; set; } = new();

    }
}
