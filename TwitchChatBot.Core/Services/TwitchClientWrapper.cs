using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchClientWrapper : ITwitchClientWrapper, IDisposable
    {
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchClientWrapper> _logger;
        private bool _disposed = false;

        public event EventHandler<TwitchMessageEventArgs>? OnMessageReceived;

        public TwitchClientWrapper(string username, string accessToken, string channel, ILogger<TwitchClientWrapper> logger)
        {
            _logger = logger;

            try
            {
                var credentials = new ConnectionCredentials(username, accessToken);
                _client = new TwitchClient();
                _client.Initialize(credentials, channel);

                _client.OnMessageReceived += HandleMessageReceived;
                _client.OnConnected += (s, e) => _logger.LogInformation("✅ Twitch connected.");
                _client.OnDisconnected += (s, e) => _logger.LogWarning("⚠️ Twitch disconnected.");
                _client.OnConnectionError += (s, e) => _logger.LogError("❌ Twitch connection error: {Error}", e.Error.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize TwitchClient.");
                throw;
            }
        }

        public void Connect() => _client.Connect();

        public void Disconnect()
        {
            if (_client.IsConnected)
            {
                _client.Disconnect();
                _logger.LogInformation("🛑 Twitch client disconnected.");
            }
        }

        public void SendMessage(string channel, string message)
        {
            if (_client.IsConnected)
            {
                _client.SendMessage(channel, message);
            }
            else
            {
                _logger.LogWarning("⚠️ Tried to send message while Twitch client is disconnected.");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Disconnect();
            _disposed = true;
        }

        private void HandleMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
            {
                Channel = e.ChatMessage.Channel,
                Username = e.ChatMessage.Username,
                Message = e.ChatMessage.Message
            });
        }
    }
}
