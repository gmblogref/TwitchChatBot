namespace TwitchChatBot.Models
{
    public class WatchStreakState
    {
        public int Version { get; set; } = 1;
        public int CurrentStreamIndex { get; set; }
        public Dictionary<string, WatchStreakUserStats> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
