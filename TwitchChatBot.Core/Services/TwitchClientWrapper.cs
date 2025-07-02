using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
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
        private readonly HttpClient _httpClient = new(); // 🔁 For TMI fallback
        private Timer? _fallbackTimer; // 🔁 TMI fallback timer
        private readonly HashSet<string> _connectedUsers = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _mods = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _vips = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _viewers = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<List<ViewerEntry>>? OnViewerListChanged;
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
                _twitchClient.OnJoinedChannel += (s, e) => _logger.LogInformation("✅ Successfully joined Twitch channel: {Channel}", e.Channel);
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

            _fallbackTimer?.Dispose(); // 🔁 stop TMI polling on disconnect
        }

        public List<ViewerEntry> GetGroupedViewers()
        {
            var result = new List<ViewerEntry>();

            foreach (var name in _mods.OrderBy(x => x))
                result.Add(new ViewerEntry { Username = name, Role = "mod" });

            foreach (var name in _vips.OrderBy(x => x))
                result.Add(new ViewerEntry { Username = name, Role = "vip" });

            foreach (var name in _viewers.OrderBy(x => x))
                result.Add(new ViewerEntry { Username = name, Role = "viewer" });

            return result;
        }

        public void SendMessage(string channel, string message)
        {
            if (_twitchClient.IsConnected &&
                _twitchClient.JoinedChannels.Any(c => c.Channel.Equals(channel, StringComparison.OrdinalIgnoreCase)))
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

        // 🔁 Polls TMI every 5 mins and merges users into _connectedUsers
        public void StartTmiFallbackTimer()
        {
            if (!double.TryParse(AppSettings.Chatters.InitialDelay, out double initDelay))
            {
                initDelay = 15; // safe default
                _logger.LogWarning("Using default initial delay (15s) for TMI fallback.");
            }

            if (!double.TryParse(AppSettings.Chatters.ContinuousDelay, out double continuousDelay))
            {
                continuousDelay = 300; // default 5 minutes
                _logger.LogWarning("Using default continuous delay (5min) for TMI fallback.");
            }

            _fallbackTimer = new Timer(async _ => await PollTmiChattersAsync(), null, TimeSpan.FromSeconds(initDelay), TimeSpan.FromSeconds(continuousDelay));
        }

        private async Task PollTmiChattersAsync()
        {
            try
            {
                _mods.Clear();
                _vips.Clear();
                _viewers.Clear();

                var url = $"{AppSettings.Chatters.BaseUrl}{AppSettings.TWITCH_CHANNEL!.ToLower()}/chatters";
                var json = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var chatters = new List<string>();

                foreach (var group in doc.RootElement.GetProperty("chatters").EnumerateObject())
                {
                    foreach (var user in group.Value.EnumerateArray())
                    {
                        var name = user.GetString()!;

                        switch (group.Name.ToLowerInvariant())
                        {
                            case "moderators":
                                _mods.Add(name);
                                break;
                            case "vips":
                                _vips.Add(name);
                                break;
                            case "viewers":
                                _viewers.Add(name);
                                break;
                        }
                    }
                }

                _connectedUsers.Clear(); // Clear connected users and just add what came back

                _logger.LogInformation("🔁 TMI fallback updated viewer list. MODS: {Mods}, VIPS: {Vips}, VIEWERS: {Viewers}",
                    _mods.Count, _vips.Count, _viewers.Count);

                OnViewerListChanged?.Invoke(this, GetGroupedViewers());

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to fetch fallback viewer list.");
            }
        }

        private async Task HandleMessageReceivedAsync(OnMessageReceivedArgs e)
        {
            var username = e.ChatMessage.Username.ToLower();
            var channel = e.ChatMessage.Channel;

            if (await _excludedUsersService.IsUserExcludedAsync(username))
            {
                _logger.LogInformation("🙈 Ignoring message from excluded user: {Username}", username);
                return;
            }

            var trimmedMessage = e.ChatMessage.Message.Trim().ToLower();

            if (trimmedMessage == "!clearfirst" && username == AppSettings.TWITCH_CHANNEL!.ToLower())
            {
                _firstChatterAlertService.ClearFirstChatters();
                _logger.LogInformation("✅ First chatters list cleared by {User}", username);
                SendMessage(e.ChatMessage.Channel, "✅ First chatters list has been cleared.");
                return;
            }

            await _firstChatterAlertService.HandleFirstChatAsync(username, e.ChatMessage.Username);

            if (trimmedMessage.StartsWith("!"))
            {
                await _commandAlertService.HandleCommandAsync(trimmedMessage, username, channel, SendMessage);
            }

            OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
            {
                Channel = channel,
                Username = username,
                Message = e.ChatMessage.Message,
                Color = ColorTranslator.FromHtml(e.ChatMessage.ColorHex ?? "#FFFFFF")
            });
        }
    }
}