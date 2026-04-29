namespace TwitchChatBot.Models
{
	public class DonationEvent
	{
		public string DonorName { get; set; } = "";
		public decimal Amount { get; set; }
		public string? Message { get; set; }
		public string Provider { get; set; } = "";
	}
}