using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Threading.Channels;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchClientWrapper : ITwitchClientWrapper, IDisposable, IAsyncDisposable
    {
        private readonly TwitchClient _twitchClient;
        private readonly ILogger<TwitchClientWrapper> _logger;
        private ICommandAlertService _commandAlertService;
        private IExcludedUsersService _excludedUsersService;
        private IFirstChatterAlertService _firstChatterAlertService;
        private readonly ITwitchRoleService _twitchRoleService;
        private readonly IWatchStreakService _watchStreakService;
        private readonly IIRCNoticeService _ircNoticeService;
        private readonly ITwitchAlertTypesService _twitchAlertTypesService;
        private readonly IHelixLookupService _helixLookupService;
        private System.Threading.Timer? _adTimer;
        private bool _disposed = false;

        private readonly HashSet<string> _connectedUsers = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _mods = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _vips = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _modList = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _vipList = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _viewers = new(StringComparer.OrdinalIgnoreCase);

        // Dedup processed USERNOTICEs (keyed by "id" or login|tmi-sent-ts)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _seenUserNoticeIds
            = new System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>();


        public event EventHandler<List<ViewerEntry>>? OnViewerListChanged;
        public event EventHandler<TwitchMessageEventArgs>? OnMessageReceived;

        public TwitchClientWrapper(
                ILogger<TwitchClientWrapper> logger,
                ICommandAlertService commandAlertService,
                IExcludedUsersService excludedUsersService,
                IFirstChatterAlertService firstChatterAlertService,
                ITwitchRoleService twitchRoleService,
                IWatchStreakService watchStreakService,
                IIRCNoticeService ircNoticeService,
                ITwitchAlertTypesService twitchAlertTypesService,
                IHelixLookupService helixLookupService)
        {
            _logger = logger;
            _commandAlertService = commandAlertService;
            _excludedUsersService = excludedUsersService;
            _firstChatterAlertService = firstChatterAlertService;
            _twitchRoleService = twitchRoleService;
            _watchStreakService = watchStreakService;
            _ircNoticeService = ircNoticeService;
            _twitchAlertTypesService = twitchAlertTypesService;
            _helixLookupService = helixLookupService;

            try
            {
                var credentials = new ConnectionCredentials(AppSettings.TWITCH_BOT_USERNAME, AppSettings.TWITCH_OAUTH_TOKEN);
                _twitchClient = new TwitchClient();
                _twitchClient.Initialize(credentials, AppSettings.TWITCH_CHANNEL);

                // Non async wire ups
                _twitchClient.OnJoinedChannel += (s, e) =>
                {
                    _logger.LogInformation("✅ Successfully joined Twitch channel: {Channel}", e.Channel);
                    return Task.CompletedTask;
                };
                _twitchClient.OnDisconnected += (s, e) =>
                {
                    _logger.LogWarning("⚠️ Twitch disconnected.");
                    return Task.CompletedTask;
                };
                _twitchClient.OnConnectionError += (s, e) => 
                { 
                    _logger.LogError("❌ Twitch connection error: {Error}", e.Error.Message); 
                    return Task.CompletedTask; 
                };
                _twitchClient.OnUserLeft += (s, e) => 
                { 
                    HandleOnUserLeft(e.Username); 
                    return Task.CompletedTask; 
                };

                // Safe async wire ups
                _twitchClient.OnMessageReceived += (s, e) =>
                {
                    return RunSafeAsync(() => HandleMessageReceivedAsync(e), "OnMessageReceived");
                };
                _twitchClient.OnConnected += (s, e) =>
                {
                    return RunSafeAsync(HandleOnConnectedAsync, "OnConnected");
                };
                _twitchClient.OnUserJoined += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnUserJoined(e.Username), "OnUserJoined");
                };
                _twitchClient.OnSendReceiveData += (s, e) =>
                {
                    return RunSafeAsync(() => HandleSendReceiveDataAsync(e), "OnSendReceiveData");
                };
                _twitchClient.OnNewSubscriber += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnNewSubscriberAsync(e), "OnNewSubscriber");
                };
                _twitchClient.OnReSubscriber += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnReSubscriberAsync(e), "OnReSubscriber");
                };
                _twitchClient.OnGiftedSubscription += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnGiftedSubscriptionAsync(e), "OnGiftedSubscription");
                };
                _twitchClient.OnCommunitySubscription += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnCommunitySubscriptionAsync(e), "OnCommunitySubscription");
                };
                _twitchClient.OnRaidNotification += (s, e) =>
                {
                    return RunSafeAsync(() => HandleOnRaidNotificationAsync(e), "OnRaidNotification");
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize TwitchClient.");
                throw;
            }
        }

        public async Task ConnectAsync() => await _twitchClient.ConnectAsync();

        public async Task DisconnectAsync()
        {
            if (_twitchClient.IsConnected)
            {
                await _twitchClient.DisconnectAsync();
                _logger.LogInformation("🛑 Twitch client disconnected.");
            }
        }

        public List<ViewerEntry> GetGroupedViewers()
        {
            _logger.LogInformation("🛑 GetGroupedViewers called.");
            var result = new List<ViewerEntry>();

            result.Add(new ViewerEntry { Username = AppSettings.TWITCH_CHANNEL, Role = "Broadcaster" });

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
            _ = RunSafeAsync(() => SendMessageAsync(channel, message), "SendMessage");
        }

        public async Task SendMessageAsync(string channel, string message)
        {
            if (_twitchClient.IsConnected &&
                _twitchClient.JoinedChannels.Any(c => c.Channel.Equals(channel, StringComparison.OrdinalIgnoreCase)))
            {
                await _twitchClient.SendMessageAsync(channel, message);
            }
            else
            {
                _logger.LogWarning("⚠️ Tried to send message while Twitch client is disconnected.");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Best-effort sync shutdown
            try
            {
                _ = DisconnectAsync();
            }
            catch
            {
                // swallow — Dispose must not throw
            }

            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            try
            {
                await DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during async dispose.");
            }

            _disposed = true;
        }

        public void StartAdTimer()
        {
            _adTimer = new Timer(_ =>
            {
                _logger.LogInformation("⏰ Ad reminder timer fired. Sending !ads command internally.");

                _ = _commandAlertService.HandleCommandAsync(
                    "!ads",
                    AppSettings.TWITCH_BOT_ID,
                    AppSettings.TWITCH_CHANNEL,
                    AppSettings.TWITCH_CHANNEL,
                    SendMessage
                );

            },
            null,
            TimeSpan.FromSeconds(AppSettings.AdInitialMinutes),
            TimeSpan.FromMinutes(AppSettings.AdIntervalMinutes));
        }

        public void StopAdTimer()
        {
            _adTimer?.Dispose();
            _adTimer = null;
        }

        private async Task HandleOnConnectedAsync()
        {
            _logger.LogInformation("✅ Twitch connected.");

            try
            {
                await _twitchClient.SendRawAsync("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");

                var mods = await _twitchRoleService.GetModeratorsAsync(AppSettings.TWITCH_USER_ID);
                var vips = await _twitchRoleService.GetVipsAsync(AppSettings.TWITCH_USER_ID);

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
            var userId = (await _helixLookupService.GetUserIdByLoginAsync(username)) ?? string.Empty;
            await _watchStreakService.MarkAttendanceAsync(userId, username);

            if (!_connectedUsers.Add(username))
                return;

            if (_modList.Contains(username))
                _mods.Add(username);
            else if (_vipList.Contains(username))
                _vips.Add(username);
            else if (username != AppSettings.TWITCH_CHANNEL)
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
            var username = e.ChatMessage!.Username.ToLower();
            var displayName = e.ChatMessage.Username;
            var userId = e.ChatMessage.UserId;
            var channel = e.ChatMessage.Channel;

            if (await _excludedUsersService.IsUserExcludedAsync(userId, username))
            {
                _logger.LogInformation("🙈 Ignoring message from excluded user: {Username}", username);
                return;
            }

            if (await _firstChatterAlertService.HandleFirstChatAsync(userId, username, e.ChatMessage.Username))
            {
                await _commandAlertService.TryAutoShoutOutIfStreamerAsync(username, channel, SendMessage);
            }

            if (e.ChatMessage.Message.Trim().StartsWith("!"))
            {
                await _commandAlertService.HandleCommandAsync(e.ChatMessage.Message.Trim(), userId, username, channel, SendMessage);
            }

            OnMessageReceived?.Invoke(this, new TwitchMessageEventArgs
            {
                Channel = channel,
                Username = username,
                Message = e.ChatMessage.Message,
                Color = ColorTranslator.FromHtml(e.ChatMessage.HexColor ?? "#FFFFFF")
            });
        }

        private async Task HandleSendReceiveDataAsync(OnSendReceiveDataArgs e)
        {
            var raw = e.Data;
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            // We only care about USERNOTICE for watch streaks
            if (!raw.Contains("USERNOTICE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var tags = ParseTags(raw);

                if (IsThisAlertAlreadyProcessing(tags, raw))
                {
                    return;
                }

                if (!tags.TryGetValue("msg-id", out var msgId) || !string.Equals(msgId, "viewermilestone", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!string.Equals(msgId, "viewermilestone", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!string.Equals(tags.GetValueOrDefault("msg-param-category"), "watch-streak", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Unescape a few tag values we typically use
                if (tags.TryGetValue("system-msg", out var sys))
                {
                    tags["system-msg"] = CoreHelperMethods.UnescapeTagValue(sys);
                }
                if (tags.TryGetValue("user-message", out var um))
                {
                    tags["user-message"] = CoreHelperMethods.UnescapeTagValue(um);
                }
                if (tags.TryGetValue("display-name", out var dn))
                {
                    tags["display-name"] = CoreHelperMethods.UnescapeTagValue(dn);
                }

                // Log for visibility while you debug
                _logger.LogInformation("🌟 Watch streak USERNOTICE for {User} (streak {Streak})",
                    tags.GetValueOrDefault("display-name", tags.GetValueOrDefault("login", AppSettings.DefaultUserName)),
                    tags.GetValueOrDefault("watch-streak-value", "?"));

                await _ircNoticeService.HandleUserNoticeAsync(tags, tags.GetValueOrDefault("system-msg"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process USERNOTICE watch-streak.");
            }
        }

        private async Task HandleOnNewSubscriberAsync(TwitchLib.Client.Events.OnNewSubscriberArgs e)
        {
            var user = e.Subscriber?.DisplayName ?? e.Subscriber?.Login ?? AppSettings.DefaultUserName;
            var tier = ConvertPlanToTier(e.Subscriber?.MsgParamSubPlan);
            await _twitchAlertTypesService.HandleSubscriptionAsync(user, tier);
        }

        private async Task HandleOnReSubscriberAsync(TwitchLib.Client.Events.OnReSubscriberArgs e)
        {
            // DisplayName/Login
            var username = e.ReSubscriber?.DisplayName ?? e.ReSubscriber?.Login ?? AppSettings.DefaultUserName;

            // Twitch sends months data as msg-param-cumulative-months / msg-param-streak-months / msg-param-months
            // TwitchLib exposes them through MsgParam... properties
            int months = e.ReSubscriber?.MsgParamCumulativeMonths
                ?? e.ReSubscriber?.MsgParamStreakMonths
                ?? 1;

            // Optional text message
            var message = e.ReSubscriber?.ResubMessage ?? string.Empty;

            // Tier mapping (still same SubscriptionPlan enum)
            var tier = ConvertPlanToTier(e.ReSubscriber?.MsgParamSubPlan);

            await _twitchAlertTypesService.HandleResubAsync(username, months, message, tier);
        }

        private async Task HandleOnGiftedSubscriptionAsync(TwitchLib.Client.Events.OnGiftedSubscriptionArgs e)
        {
            var gifter = e.GiftedSubscription?.DisplayName ?? e.GiftedSubscription?.Login ?? AppSettings.DefaultUserName;
            var recipient = e.GiftedSubscription?.MsgParamRecipientUserName ?? e.GiftedSubscription?.MsgParamRecipientUserName ?? AppSettings.DefaultUserName;
            var tier = ConvertPlanToTier(e.GiftedSubscription?.MsgParamSubPlan);
            await _twitchAlertTypesService.HandleSubGiftAsync(gifter, recipient, tier);
        }

        private async Task HandleOnCommunitySubscriptionAsync(TwitchLib.Client.Events.OnCommunitySubscriptionArgs e)
        {
            var gifter = e.GiftedSubscription?.DisplayName ?? e.GiftedSubscription?.Login ?? AppSettings.DefaultUserName;
            var count =
                (e.GiftedSubscription?.MsgParamMassGiftCount ?? 0) > 0 ? e.GiftedSubscription!.MsgParamMassGiftCount :
                (e.GiftedSubscription?.MsgParamMassGiftCount ?? 0) > 0 ? e.GiftedSubscription!.MsgParamMassGiftCount : 1;
            var tier = ConvertPlanToTier(e.GiftedSubscription?.MsgParamSubPlan);
            await _twitchAlertTypesService.HandleSubMysteryGiftAsync(gifter, count, tier);
        }

        private async Task HandleOnRaidNotificationAsync(TwitchLib.Client.Events.OnRaidNotificationArgs e)
        {
            var raiderDisplay = e.RaidNotification?.MsgParamDisplayName ?? AppSettings.DefaultUserName;
            var raiderUserId = e.RaidNotification?.UserId ?? string.Empty;
            int viewers = 1;
            if (int.TryParse(e.RaidNotification?.MsgParamViewerCount, out var parsed))
            {
                viewers = parsed;
            }

            await _twitchAlertTypesService.HandleRaidAsync(raiderDisplay, viewers);

            // Auto-!so as the bot (bypass excluded users via isSystem flag on your command service)
            var raiderLogin = e.RaidNotification?.MsgParamLogin;
            var handle = string.IsNullOrWhiteSpace(raiderLogin) ? raiderDisplay : raiderLogin;
            if (!handle.StartsWith("@", StringComparison.Ordinal))
            {
                handle = "@" + handle;
            }

            await _commandAlertService.HandleCommandAsync(
                $"!so {handle}",
                raiderUserId,
                AppSettings.TWITCH_BOT_USERNAME,
                AppSettings.TWITCH_CHANNEL,
                SendMessage,
                isAutoCommand: true);
        }

        private static string ConvertPlanToTier(TwitchLib.Client.Enums.SubscriptionPlan? plan)
        {
            if(plan == null)
            {
                return "1000"; // Default to Tier 1 if plan is missing
            }

            switch(plan.Value)
            {
                case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                    return "1000";
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                    return "2000";
                case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                    return "3000";
                default:
                    return "1000";
            }
        }

        private bool IsThisAlertAlreadyProcessing(Dictionary<string, string> tags, string raw)
        {
            var uniqueId =
                tags.GetValueOrDefault("id") ??
                $"{tags.GetValueOrDefault("login", "")}|{tags.GetValueOrDefault("tmi-sent-ts", "")}";

            if (string.IsNullOrEmpty(uniqueId))
            {
                // Extremely rare: still create a stable key to prevent back-to-back duplicates
                uniqueId = $"{tags.GetValueOrDefault("display-name", AppSettings.DefaultUserName)}|" + raw.GetHashCode();
            }

            // Try to add; if already there, we already handled this notice
            if (!_seenUserNoticeIds.TryAdd(uniqueId, DateTime.UtcNow))
            {
                _logger.LogDebug("🔁 Duplicate USERNOTICE ignored: {UniqueId}", uniqueId);
                return true;
            }

            return false;
        }

        private static Dictionary<string, string> ParseTags(string raw)
        {
            var tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(raw))
            {
                return tags;
            }

            // Find the first '@' even if the logger added a prefix like "Received: "
            var atIndex = raw.IndexOf('@');
            if (atIndex < 0)
            {
                return tags;
            }

            // Tags end at the first space following the '@...;' section
            var spaceAfterTags = raw.IndexOf(' ', atIndex);
            if (spaceAfterTags < 0)
            {
                return tags;
            }

            // Extract "badge-info=...;...;vip=0" (without the leading '@')
            var tagSection = raw.Substring(atIndex + 1, spaceAfterTags - (atIndex + 1));
            var pairs = tagSection.Split(';');

            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair))
                {
                    continue;
                }

                var eqIndex = pair.IndexOf('=');
                if (eqIndex >= 0)
                {
                    var key = pair.Substring(0, eqIndex);
                    var value = (eqIndex + 1 < pair.Length) ? pair.Substring(eqIndex + 1) : string.Empty;
                    tags[key] = value;
                }
                else
                {
                    // Key present with empty value
                    tags[pair] = string.Empty;
                }
            }

            // Now extract trailing message after " USERNOTICE ... :"
            // The pattern is: ":tmi.twitch.tv USERNOTICE #channel :<trailing>"
            var usernoticeIndex = raw.IndexOf(" USERNOTICE ", spaceAfterTags, StringComparison.OrdinalIgnoreCase);
            if (usernoticeIndex >= 0)
            {
                var colonIndex = raw.IndexOf(" :", usernoticeIndex, StringComparison.Ordinal);
                if (colonIndex >= 0 && colonIndex + 2 <= raw.Length)
                {
                    var key = "user-message";
                    var value = raw.Substring(colonIndex + 2);
                    tags[key] = value;
                }
            }

            return tags;
        }

        private async Task RunSafeAsync(Func<Task> action, string context)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unhandled exception in {Context}", context);
            }
        }
    }
}