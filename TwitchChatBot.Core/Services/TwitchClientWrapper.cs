using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchClientWrapper : ITwitchClientWrapper, IDisposable
    {
        private readonly TwitchClient _twitchClient;
        private readonly ILogger<TwitchClientWrapper> _logger;
        private ICommandAlertService _commandAlertService;
        private IExcludedUsersService _excludedUsersService;
        private IFirstChatterAlertService _firstChatterAlertService;
        private bool _disposed = false;

        public event EventHandler<TwitchMessageEventArgs>? OnMessageReceived;

        public TwitchClientWrapper(
                ILogger<TwitchClientWrapper> logger,
                ICommandAlertService commandAlertService,
                IExcludedUsersService excludedUsersService,
                IFirstChatterAlertService firstChatterAlertService)
        {
            _logger = logger;
            _commandAlertService = commandAlertService;
            _excludedUsersService = excludedUsersService;
            _firstChatterAlertService = firstChatterAlertService;
            
            try
            {
                var credentials = new ConnectionCredentials(AppSettings.TWITCH_BOT_USERNAME!, AppSettings.TWITCH_OAUTH_TOKEN!);
                _twitchClient = new TwitchClient();
                _twitchClient.Initialize(credentials, AppSettings.TWITCH_CHANNEL!);

                _twitchClient.OnMessageReceived += async (s, e) => await HandleMessageReceivedAsync(e);
                _twitchClient.OnConnected += (s, e) => _logger.LogInformation("✅ Twitch connected.");
                _twitchClient.OnDisconnected += (s, e) => _logger.LogWarning("⚠️ Twitch disconnected.");
                _twitchClient.OnConnectionError += (s, e) => _logger.LogError("❌ Twitch connection error: {Error}", e.Error.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize TwitchClient.");
                throw;
            }
        }

        public void Connect() => _twitchClient.Connect();

        public void Disconnect()
        {
            if (_twitchClient.IsConnected)
            {
                _twitchClient.Disconnect();
                _logger.LogInformation("🛑 Twitch client disconnected.");
            }
        }

        public void SendMessage(string channel, string message)
        {
            if (_twitchClient.IsConnected)
            {
                _twitchClient.SendMessage(channel, message);
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
            var channel = e.ChatMessage.Channel;

            // If excluded user we skip handeling the message
            if (await _excludedUsersService.IsUserExcludedAsync(username))
            {
                _logger.LogInformation("🙈 Ignoring message from excluded user: {Username}", username);
                return;
            }

            var trimmedMessage = e.ChatMessage.Message.Trim().ToLower();

            // Check if it's the clear command and from the broadcaster
            if (trimmedMessage == "!clearfirst" && username == AppSettings.TWITCH_CHANNEL!.ToLower())
            {
                _firstChatterAlertService.ClearFirstChatters();
                _logger.LogInformation("✅ First chatters list cleared by {User}", username);
                SendMessage(e.ChatMessage.Channel, "✅ First chatters list has been cleared.");
                return;
            }

            // Do First Chat Message
            await _firstChatterAlertService.HandleFirstChatAsync(username, e.ChatMessage.Username);


            if (trimmedMessage.StartsWith("!"))
            {
                await _commandAlertService.HandleCommandAsync(trimmedMessage, username, channel, SendMessage);
            }

            OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
            {
                Channel = channel,
                Username = username,
                Message = e.ChatMessage.Message
            });
        }
    }
}
