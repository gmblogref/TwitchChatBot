namespace TwitchChatBot.Models
{
	public class ChannelPointLockResult
	{
		public bool IsAllowed { get; set; }
		public string GroupName { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
	}
}