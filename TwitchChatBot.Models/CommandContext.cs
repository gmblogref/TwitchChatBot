namespace TwitchChatBot.Models
{
    public class CommandContext
    {
        public required string Channel { get; init; }
        public required string Username { get; init; }       // user who ran the command
        public required string Command { get; init; }
        public string? UserId { get; set; }
        public string? RawTarget { get; init; }             // target name without '@'
        public string? TtsText { get; init; }
        public string? CommandText { get; set; }
        public string Target => string.IsNullOrEmpty(RawTarget) ? string.Empty : $"@{RawTarget}";
        public string Url => string.IsNullOrEmpty(RawTarget) ? string.Empty : AppSettings.TwitchUrl + $"{RawTarget}";
        public string Game { get; set; } = string.Empty;
    }
}