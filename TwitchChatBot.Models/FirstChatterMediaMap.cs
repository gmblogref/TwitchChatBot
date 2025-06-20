namespace TwitchChatBot.Models
{
    public class FirstChatterMediaMap
    {
        public List<FirstChatterMediaItem> FirstChatterMediaItems { get; set; } = new();
    }

    public class FirstChatterMediaItem
    {
        public string Username { get; set; } = string.Empty;
        public string Media { get; set; } = string.Empty;
    }
}