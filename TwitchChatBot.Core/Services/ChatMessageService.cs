using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
	public class ChatMessageService : IChatMessageService
	{
		private readonly ILogger<ChatMessageService> _logger;
		private readonly ITwitchClientWrapper _twitchClientWrapper;

		public ChatMessageService(
			ILogger<ChatMessageService> logger,
			ITwitchClientWrapper twitchClientWrapper)
		{
			_logger = logger;
			_twitchClientWrapper = twitchClientWrapper;
		}

		public void SendMessage(string channel, string message)
		{
			if (string.IsNullOrWhiteSpace(channel))
			{
				_logger.LogWarning("Cannot send chat message because channel was empty.");
				return;
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				_logger.LogWarning("Cannot send chat message because message was empty.");
				return;
			}

			_twitchClientWrapper.SendMessage(channel, message);
		}
	}
}