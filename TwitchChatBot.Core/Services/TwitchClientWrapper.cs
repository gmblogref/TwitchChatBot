using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json;
using System.Threading.Channels;
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
        private readonly ITwitchRoleService _twitchRoleService;
        private readonly IWatchStreakService _watchStreakService;
        private System.Threading.Timer? _adTimer;
        private bool _disposed = false;
        
        private readonly HashSet<string> _connectedUsers = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _mods = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _vips = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _modList = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _vipList = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _viewers = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<List<ViewerEntry>>? OnViewerListChanged;
        public event EventHandler<TwitchMessageEventArgs>? OnMessageReceived;

        public TwitchClientWrapper(
                ILogger<TwitchClientWrapper> logger,
                ICommandAlertService commandAlertService,
                IExcludedUsersService excludedUsersService,
                IFirstChatterAlertService firstChatterAlertService,
                ITwitchRoleService twitchRoleService,
                IWatchStreakService watchStreakService)
        {
            _logger = logger;
            _commandAlertService = commandAlertService;
            _excludedUsersService = excludedUsersService;
            _firstChatterAlertService = firstChatterAlertService;
            _twitchRoleService = twitchRoleService;
            _watchStreakService = watchStreakService;

            try
            {
                var credentials = new ConnectionCredentials(AppSettings.TWITCH_BOT_USERNAME!, AppSettings.TWITCH_OAUTH_TOKEN!);
                _twitchClient = new TwitchClient();
                _twitchClient.Initialize(credentials, AppSettings.TWITCH_CHANNEL!);

                _twitchClient.OnMessageReceived += async (s, e) => await HandleMessageReceivedAsync(e);
                _twitchClient.OnConnected += async (s, e) => await HandelOnConnectedAsync();
                _twitchClient.OnJoinedChannel += (s, e) => _logger.LogInformation("✅ Successfully joined Twitch channel: {Channel}", e.Channel);
                _twitchClient.OnDisconnected += (s, e) => _logger.LogWarning("⚠️ Twitch disconnected.");
                _twitchClient.OnConnectionError += (s, e) => _logger.LogError("❌ Twitch connection error: {Error}", e.Error.Message);
                _twitchClient.OnUserJoined += async (s, e) => await HandleOnUserJoined(e.Username);
                _twitchClient.OnUserLeft += (s, e) => HandleOnUserLeft(e.Username);
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

        public List<ViewerEntry> GetGroupedViewers()
        {
            _logger.LogInformation("🛑 GetGroupedViewers called.");
            var result = new List<ViewerEntry>();

            result.Add(new ViewerEntry { Username = AppSettings.TWITCH_CHANNEL!, Role = "Broadcaster" }); 

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

        public void StartAdTimer()
        {
            _adTimer = new Timer(_ =>
            {
                _logger.LogInformation("⏰ Ad reminder timer fired. Sending !ads command internally.");
                _ = _commandAlertService.HandleCommandAsync("!ads", AppSettings.TWITCH_CHANNEL!, AppSettings.TWITCH_CHANNEL!, SendMessage);
            }, null, TimeSpan.FromSeconds(AppSettings.AdInitialMinutes), TimeSpan.FromMinutes(AppSettings.AdIntervalMinutes));
        }
        public void StopAdTimer()
        {
            _adTimer?.Dispose();
            _adTimer = null;
        }

        private async Task HandelOnConnectedAsync()
        {
            _logger.LogInformation("✅ Twitch connected.");

            try
            {
                var mods = await _twitchRoleService.GetModeratorsAsync(AppSettings.TWITCH_USER_ID!);
                var vips = await _twitchRoleService.GetVipsAsync(AppSettings.TWITCH_USER_ID!);

                _modList.UnionWith(mods);
                _vipList.UnionWith(vips);

                _logger.LogInformation("🔓 Populated modList with {ModCount} mods, vipList with {VipCount} VIPs", mods.Count, vips.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to load mod/vip lists after connection.");
            }
        }

        private async Task HandleOnUserJoined(string username)
        {
            await _watchStreakService.MarkAttendanceAsync(username);
            
            if (!_connectedUsers.Add(username))
                return;

            if (_modList.Contains(username))
                _mods.Add(username);
            else if (_vipList.Contains(username))
                _vips.Add(username);
            else if(username != AppSettings.TWITCH_CHANNEL)
                _viewers.Add(username);

            _logger.LogInformation("👤 Joined: {User}", username);
            OnViewerListChanged?.Invoke(this, GetGroupedViewers());
        }

        private void HandleOnUserLeft(string username)
        {
            if (!_connectedUsers.Remove(username))
                return;

            _mods.Remove(username);
            _vips.Remove(username);
            _viewers.Remove(username);

            _logger.LogInformation("👋 Left: {User}", username);
            OnViewerListChanged?.Invoke(this, GetGroupedViewers());
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

            if (e.ChatMessage.Message.Trim().ToLower() == "!clearfirst" && username == AppSettings.TWITCH_CHANNEL!.ToLower())
            {
                _firstChatterAlertService.ClearFirstChatters();
                _logger.LogInformation("✅ First chatters list cleared by {User}", username);
                SendMessage(e.ChatMessage.Channel, "✅ First chatters list has been cleared.");
                return;
            }

            await _firstChatterAlertService.HandleFirstChatAsync(username, e.ChatMessage.Username);

            if (e.ChatMessage.Message.Trim().StartsWith("!"))
            {
                await _commandAlertService.HandleCommandAsync(e.ChatMessage.Message.Trim(), username, channel, SendMessage);
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