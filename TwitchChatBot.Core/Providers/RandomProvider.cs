namespace TwitchChatBot.Core.Providers
{
	public class RandomProvider : IRandomProvider
	{
		private readonly Random _random = new Random();

		public int Next(int min, int max)
		{
			return _random.Next(min, max);
		}
	}
}