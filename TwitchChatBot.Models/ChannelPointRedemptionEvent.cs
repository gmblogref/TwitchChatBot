namespace TwitchChatBot.Models
{
	public class ChannelPointRedemptionEvent
	{
		public string RedemptionId { get; set; } = string.Empty;
		public string RewardId { get; set; } = string.Empty;
		public string RewardTitle { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string UserLogin { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public string UserInput { get; set; } = string.Empty;
	}
}