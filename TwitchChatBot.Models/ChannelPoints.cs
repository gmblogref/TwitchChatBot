using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitchChatBot.Models
{
    public class ChannelPoints
    {
        [JsonPropertyName("tiers")]
        public List<Tier>? Tiers { get; set; }


        [JsonPropertyName("default")]
        public string? Default { get; set; }
    }
}
