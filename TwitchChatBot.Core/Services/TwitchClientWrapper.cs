﻿using Microsoft.Extensions.Logging;
using System.Drawing;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
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
        private readonly IIRCNoticeService _ircNoticeService;
        private readonly ITwitchAlertTypesService _twitchAlertTypesService;
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
                ITwitchAlertTypesService twitchAlertTypesService)
        {
            _logger = logger;
            _commandAlertService = commandAlertService;
            _excludedUsersService = excludedUsersService;
            _firstChatterAlertService = firstChatterAlertService;
            _twitchRoleService = twitchRoleService;
            _watchStreakService = watchStreakService;
            _ircNoticeService = ircNoticeService;
            _twitchAlertTypesService = twitchAlertTypesService;

            try
            {
                var credentials = new ConnectionCredentials(AppSettings.TWITCH_BOT_USERNAME!, AppSettings.TWITCH_OAUTH_TOKEN!);
                _twitchClient = new TwitchClient();
                _twitchClient.Initialize(credentials, AppSettings.TWITCH_CHANNEL!);

                _twitchClient.OnMessageReceived += async (s, e) => await HandleMessageReceivedAsync(e);
                _twitchClient.OnConnected += async (s, e) => await HandleOnConnectedAsync();
                _twitchClient.OnJoinedChannel += (s, e) => _logger.LogInformation("✅ Successfully joined Twitch channel: {Channel}", e.Channel);
                _twitchClient.OnDisconnected += (s, e) => _logger.LogWarning("⚠️ Twitch disconnected.");
                _twitchClient.OnConnectionError += (s, e) => _logger.LogError("❌ Twitch connection error: {Error}", e.Error.Message);
                _twitchClient.OnUserJoined += async (s, e) => await HandleOnUserJoined(e.Username);
                _twitchClient.OnUserLeft += (s, e) => HandleOnUserLeft(e.Username);
                _twitchClient.OnLog += async (s, e) => await HandleOnLogAsync(e);

                _twitchClient.OnNewSubscriber += async (s, e) => { await HandleOnNewSubscriberAsync(e); };
                _twitchClient.OnReSubscriber += async (s, e) => { await HandleOnReSubscriberAsync(e); };
                _twitchClient.OnGiftedSubscription += async (s, e) => { await HandleOnGiftedSubscriptionAsync(e); };
                _twitchClient.OnCommunitySubscription += async (s, e) => { await HandleOnCommunitySubscriptionAsync(e); };
                _twitchClient.OnRaidNotification += async (s, e) => { await HandleOnRaidNotificationAsync(e); };

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

        private async Task HandleOnConnectedAsync()
        {
            _logger.LogInformation("✅ Twitch connected.");

            try
            {
                _twitchClient.SendRaw("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");

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
            var username = e.ChatMessage!.Username.ToLower();
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

        private async Task HandleOnLogAsync(OnLogArgs e)
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

                if(IsThisAlertAlreadyProcessing(tags, raw))
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
                    tags.GetValueOrDefault("display-name", tags.GetValueOrDefault("login", "someone")),
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
            var user = e.Subscriber?.DisplayName ?? e.Subscriber?.Login ?? "someone";
            var tier = ConvertPlanToTier(e.Subscriber?.SubscriptionPlan);
            await _twitchAlertTypesService.HandleSubscriptionAsync(user, tier);
        }

        private async Task HandleOnReSubscriberAsync(TwitchLib.Client.Events.OnReSubscriberArgs e)
        {
            // DisplayName/Login
            var username = e.ReSubscriber?.DisplayName ?? e.ReSubscriber?.Login ?? "someone";

            // Twitch sends months data as msg-param-cumulative-months / msg-param-streak-months / msg-param-months
            // TwitchLib exposes them through MsgParam... properties
            int months = 1;

            if (int.TryParse(e.ReSubscriber?.MsgParamCumulativeMonths, out var cum))
            {
                months = cum;
            }
            else if (int.TryParse(e.ReSubscriber?.MsgParamStreakMonths, out var streak))
            {
                months = streak;
            }
            else if (e.ReSubscriber?.Months != null)
            {
                months = e.ReSubscriber.Months;
            }

            // Optional text message
            var message = e.ReSubscriber?.ResubMessage ?? string.Empty;

            // Tier mapping (still same SubscriptionPlan enum)
            var tier = ConvertPlanToTier(e.ReSubscriber?.SubscriptionPlan);

            await _twitchAlertTypesService.HandleResubAsync(username, months, message, tier);
        }

        private async Task HandleOnGiftedSubscriptionAsync(TwitchLib.Client.Events.OnGiftedSubscriptionArgs e)
        {
            var gifter = e.GiftedSubscription?.DisplayName ?? e.GiftedSubscription?.Login ?? "someone";
            var recipient = e.GiftedSubscription?.MsgParamRecipientUserName ?? e.GiftedSubscription?.MsgParamRecipientUserName ?? "someone";
            var tier = ConvertPlanToTier(e.GiftedSubscription?.MsgParamSubPlan);
            await _twitchAlertTypesService.HandleSubGiftAsync(gifter, recipient, tier);
        }

        private async Task HandleOnCommunitySubscriptionAsync(TwitchLib.Client.Events.OnCommunitySubscriptionArgs e)
        {
            var gifter = e.GiftedSubscription?.DisplayName ?? e.GiftedSubscription?.Login ?? "someone";
            var count =
                (e.GiftedSubscription?.MsgParamMassGiftCount ?? 0) > 0 ? e.GiftedSubscription!.MsgParamMassGiftCount :
                (e.GiftedSubscription?.MsgParamMassGiftCount ?? 0) > 0 ? e.GiftedSubscription!.MsgParamMassGiftCount : 1;
            var tier = ConvertPlanToTier(e.GiftedSubscription?.MsgParamSubPlan);
            await _twitchAlertTypesService.HandleSubMysteryGiftAsync(gifter, count, tier);
        }

        private async Task HandleOnRaidNotificationAsync(TwitchLib.Client.Events.OnRaidNotificationArgs e)
        {
            var raiderDisplay = e.RaidNotification?.MsgParamDisplayName ?? "someone";
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
                AppSettings.TWITCH_BOT_USERNAME!,
                AppSettings.TWITCH_CHANNEL!,
                SendMessage,
                isAutoCommand: true);
        }

        private static string ConvertPlanToTier(TwitchLib.Client.Enums.SubscriptionPlan? plan)
        {
            if (!plan.HasValue) { return "1000"; }
            if (plan.Value == TwitchLib.Client.Enums.SubscriptionPlan.Prime ||
                plan.Value == TwitchLib.Client.Enums.SubscriptionPlan.Tier1) { return "1000"; }
            if (plan.Value == TwitchLib.Client.Enums.SubscriptionPlan.Tier2) { return "2000"; }
            if (plan.Value == TwitchLib.Client.Enums.SubscriptionPlan.Tier3) { return "3000"; }
            return "1000";
        }


        private bool IsThisAlertAlreadyProcessing(Dictionary<string, string> tags, string raw)
        {
            var uniqueId =
                tags.GetValueOrDefault("id") ??
                $"{tags.GetValueOrDefault("login", "")}|{tags.GetValueOrDefault("tmi-sent-ts", "")}";

            if (string.IsNullOrEmpty(uniqueId))
            {
                // Extremely rare: still create a stable key to prevent back-to-back duplicates
                uniqueId = $"{tags.GetValueOrDefault("display-name", "someone")}|" + raw.GetHashCode();
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
    }
}