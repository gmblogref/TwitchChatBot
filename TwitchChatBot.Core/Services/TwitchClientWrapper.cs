using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchClientWrapper : ITwitchClientWrapper, IDisposable
    {
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchClientWrapper> _logger;
        private IExcludedUsersRepository _excludedUsersRepository;
        private IFirstChatterMediaRepository _firstChatterMediaRepository;
        private bool _disposed = false;
        private List<string> _firstChatters = [];

        public event EventHandler<TwitchMessageEventArgs>? OnMessageReceived;

        public TwitchClientWrapper(
                string username, 
                string accessToken, 
                string channel, 
                ILogger<TwitchClientWrapper> logger,
                IExcludedUsersRepository excludedUsersRepository,
                IFirstChatterMediaRepository firstChatterMediaRepository)
        {
            _logger = logger;
            _excludedUsersRepository = excludedUsersRepository;
            _firstChatterMediaRepository = firstChatterMediaRepository;

            try
            {
                var credentials = new ConnectionCredentials(username, accessToken);
                _client = new TwitchClient();
                _client.Initialize(credentials, channel);

                _client.OnMessageReceived += async (s, e) => await HandleMessageReceivedAsync(e);
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

        private async Task HandleMessageReceivedAsync(OnMessageReceivedArgs e)
        {
            var username = e.ChatMessage.Username.ToLower();

            // If excluded user we skip handeling the message
            if (await _excludedUsersRepository.IsUserExcludedAsync(username))
            {
                _logger.LogInformation("🙈 Ignoring message from excluded user: {Username}", username);
                return;
            }

            var message = e.ChatMessage.Message.Trim().ToLower();

            // Check if it's the clear command and from the broadcaster
            if (message == "!clearfirst" && username == AppSettings.TWITCH_CHANNEL!.ToLower())
            {
                _firstChatterMediaRepository.ClearFirstChatters();
                _logger.LogInformation("✅ First chatters list cleared by {User}", username);
                SendMessage(e.ChatMessage.Channel, "✅ First chatters list has been cleared.");
                return;
            }

            OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
            {
                Channel = e.ChatMessage.Channel,
                Username = username,
                Message = e.ChatMessage.Message
            });
        }
    }
}
