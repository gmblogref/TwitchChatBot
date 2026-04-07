namespace TwitchChatBot.Models
{
	public class Wheel
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();

		public string Name { get; set; } = string.Empty;

		public List<WheelItem> Items { get; set; } = new List<WheelItem>();
	}
}