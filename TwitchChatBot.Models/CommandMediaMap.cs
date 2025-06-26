using System.Text.Json.Serialization;

namespace TwitchChatBot.Models
{
    public class CommandMediaMap
    {
        [JsonPropertyName("commandMediaItems")]
        public List<CommandMediaItem> CommandMediaItems { get; set; } = new();
    }

    public class CommandMediaItem
    {
        public string Command { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? Media { get; set; }
    }
}
