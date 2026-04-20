namespace TwitchChatBot.Core.Providers
{
	public interface IRandomProvider
	{
		int Next(int min, int max);
	}
}