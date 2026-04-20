namespace TwitchChatBot.Models
{
	public class WheelItem
	{
		public string Id { get; set; } = string.Empty;

		public int Position { get; set; }

		public string DisplayName { get; set; } = string.Empty;

		public string? AlertType { get; set; }

		public string? AlertKey { get; set; }

		public int Weight { get; set; } = 1;

		public bool IsHidden { get; set; } = false;
	}
}