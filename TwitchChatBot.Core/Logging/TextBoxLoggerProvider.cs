using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Logging
{
    public class TextBoxLoggerProvider : ILoggerProvider
    {
        private readonly IUiBridge _uiBridge;

        public TextBoxLoggerProvider(IUiBridge uiBridge)
        {
            _uiBridge = uiBridge;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _uiBridge != null
        ? new TextBoxLogger(_uiBridge, categoryName)
        : throw new ArgumentNullException(nameof(categoryName));
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}