using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Logging
{
    public class TextBoxLogger : ILogger
    {
        private readonly IUiBridge _uiBridge;
        private readonly string _categoryName;

        public TextBoxLogger(IUiBridge uiBridge, string categoryName)
        {
            _uiBridge = uiBridge;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var logLine = $"[{logLevel}] {_categoryName}: {message}";
            _uiBridge.AppendLog(logLine);
        }
    }

    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        private NullScope() { }
        public void Dispose() { }
    }

}