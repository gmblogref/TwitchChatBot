namespace TwitchChatBot.Models
{
    public class AlertItem
    {
        public string Type { get; set; } = "alert"; // "alert" or "fullscreen"
        public string Message { get; set; } = string.Empty;
        public string? Media { get; set; }
    }
}
