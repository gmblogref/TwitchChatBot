namespace TwitchChatBot.Models
{
    public class AlertHistoryEntry
    {
        // Core
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public string Type { get; init; } = string.Empty;   // see AlertHistoryType
        public string Display { get; init; } = string.Empty; // nice one-liner for UI list
        public string? Username { get; init; }              // actor / primary user
        public string? DisplayName { get; init; }              // Twitch display name

        // Command / TTS
        public string? CommandText { get; init; }           // full raw command text (e.g., "!tts Ivy hello")
        public string? Voice { get; init; }                 // optional voice for direct TTS
        public string? Message { get; init; }               // message body (cheer msg, resub msg, etc.)

        // Twitch event specifics
        public int? Bits { get; init; }
        public int? Months { get; init; }
        public int? Count { get; init; }                    // gift count / raid viewers / watched streams
        public int? Viewers { get; init; }                  // raid viewers (alias of Count for clarity)
        public string? Tier { get; init; }                  // "1000" | "2000" | "3000"
        public string? Gifter { get; init; }
        public string? Recipient { get; init; }
        public string? RewardTitle { get; init; }           // channel points

        // Media (when replay falls back to media queue)
        public string? MediaPath { get; init; }             // public path or file path to media

        // Extra bag for future-proofing
        public Dictionary<string, string?> Extras { get; init; } = new();
    }
}