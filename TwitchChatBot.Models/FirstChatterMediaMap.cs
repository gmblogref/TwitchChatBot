namespace TwitchChatBot.Models
{
    public class FirstChatterMediaMap
    {
        public List<FirstChatterMediaItem> FirstChatterMediaItems { get; set; } = new();
    }

    public class FirstChatterMediaItem
    {
        public string UserId { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public string Media { get; set; } = string.Empty;

        // Empty or null = allowed any day
        public List<DayOfWeek> AllowedDaysOfWeek { get; set; } = new();
    }
}