namespace TwitchChatBot.Models
{
	public class SpinResult
	{
		public string EntryId { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string? AlertType { get; set; }
		public string? AlertKey { get; set; }
	}
}