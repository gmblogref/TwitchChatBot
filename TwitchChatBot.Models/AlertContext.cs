namespace TwitchChatBot.Models
{
    public sealed class AlertContext
    {
        // Core identifiers
        public AlertAiType AiType { get; set; }
        public string Username { get; set; } = AppSettings.DefaultUserName;

        // Optional event-specific fields
        public int? ViewerCount { get; set; }
        public int? StreakCount { get; set; }
        public int? Months { get; set; }
        public int? GiftCount { get; set; }
        public string? Tier { get; set; }
        public string? UserMessage { get; set; }

        // AI controls
        public string Tone { get; set; } = AppSettings.OpenAI.DefaultAlertTone;
        public int MaxWords { get; set; } = 14;
    }
}