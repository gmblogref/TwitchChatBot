namespace TwitchChatBot.Models
{
    public class AlertItem
    {
        public string Type { get; set; } = "alert"; // "alert" or "fullscreen"
        public string Message { get; set; } = string.Empty;
        public string? MediaPath { get; set; }
		public bool IsTts { get; set; }
		public long? TtsSequence { get; set; }
		public bool IsCanceled { get; set; }

		public Dictionary<string, string?>? ExtraData { get; set; }
	}
}
