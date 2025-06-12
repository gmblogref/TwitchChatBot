using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchChatBot.Models
{
    public class AlertMediaMap
    {
        [JsonPropertyName("channel_points")] 
        public ChannelPoints? Channel_Points { get; set; }

        [JsonPropertyName("channel_points_text")] 
        public ChannelPointsText? Channel_Points_Text { get; set; }

        [JsonPropertyName("cheer")] 
        public Cheer? Cheer { get; set; }

        [JsonPropertyName("follow")] 
        public List<string>? Follow { get; set; }

        [JsonPropertyName("hype_train")] 
        public List<string>? Hype_Train { get; set; }

        [JsonPropertyName("raid")] 
        public List<string>? Raid { get; set; }

        [JsonPropertyName("resub")] 
        public List<string>? Resub { get; set; }

        [JsonPropertyName("subgift")] 
        public List<string>? Subgift { get; set; }

        [JsonPropertyName("submysterygift")] 
        public SubMysteryGift? Submysterygift { get; set; }

        [JsonPropertyName("subscription")] 
        public List<string>? Subscription { get; set; }
    }
}
