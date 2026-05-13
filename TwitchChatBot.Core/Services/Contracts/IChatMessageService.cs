namespace TwitchChatBot.Core.Services.Contracts
{
	public interface IChatMessageService
	{
		void SendMessage(string channel, string message);
	}
}