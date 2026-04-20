namespace TwitchChatBot.Models
{
	public class WheelItemRow
	{
		public string Id { get; set; } = string.Empty;
		public string Label { get; set; } = string.Empty;
		public int Weight { get; set; }
		public bool Hidden { get; set; }
	}
}