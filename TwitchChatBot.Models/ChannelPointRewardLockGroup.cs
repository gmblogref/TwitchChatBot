namespace TwitchChatBot.Models
{
	public class ChannelPointRewardLockGroup
	{
		public string GroupName { get; set; } = string.Empty;
		public List<string> RewardTitles { get; set; } = new();
	}
}