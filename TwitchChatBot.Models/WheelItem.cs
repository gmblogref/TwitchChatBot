namespace TwitchChatBot.Models
{
	public class WheelItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();

		public string DisplayName { get; set; } = string.Empty;

		public string ActionType { get; set; } = string.Empty;

		public string ActionValue { get; set; } = string.Empty;

		public int Weight { get; set; } = 1;

		public bool IsHidden { get; set; } = false;
	}
}