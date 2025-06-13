namespace TwitchChatBot.Models
{
    public class AppSettings
    {
        public string? TWITCH_BOT_USERNAME { get; set; }
        public string? TWITCH_OAUTH_TOKEN { get; set; }
        public string? TWITCH_CHANNEL { get; set; }
        public string? STREAMLABS_SOCKET_TOKEN { get; set; }
        public string? BOT_CLIENT_ID { get; set; }
        public string? BOT_CLIENT_SECRET { get; set; }
        public string? TWITCH_ACCESS_TOKEN { get; set; }
        public string? TWITCH_CLIENT_ID { get; set; }
        public string? REFRESH_TOKEN { get; set; }
        public string? TWITCH_USER_ID { get; set; }
        public string? EVENTSUB_SECRET { get; set; }
        public string? EVENTSUB_CALLBACK_URL { get; set; }
        public string? TWITCH_APP_ACCESS_TOKEN { get; set; }

        // Optional: Timing
        public int AdIntervalMinutes { get; set; } = 60;
    }
}
