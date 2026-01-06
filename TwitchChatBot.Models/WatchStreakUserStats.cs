namespace TwitchChatBot.Models
{
    public class WatchStreakUserStats
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public int FailedUserIdLookups { get; set; }
        public int TotalStreams { get; set; }
        public int Consecutive { get; set; }
        public int LastAttendedIndex { get; set; }
        public DateTimeOffset? LastSeenUtc { get; set; }
        public DateTimeOffset? FirstSeenUtc { get; set; }
    }
}