using Microsoft.Extensions.Configuration;

namespace TwitchChatBot.Models
{
    public static class AppSettings
    {
        public static IConfiguration? Configuration { get; set; }

        public static class Twitch
        {
            public static string TWITCH_BOT_USERNAME => GetStringSetting("Twitch:BotUsername");
            public static string TWITCH_CHANNEL => GetStringSetting("Twitch:Channel");
            public static string TWITCH_BOT_ID => GetStringSetting("Twitch:BotUserId");
            public static string TWITCH_USER_ID => GetStringSetting("Twitch:BroadcasterUserId");
            public static string TwitchUrl => GetStringSetting("Twitch:TwitchUrl");
        }

        public static class Auth
        {
            public static string BOT_CLIENT_ID => GetStringSetting("Auth:BotClientId");
            public static string BOT_CLIENT_SECRET => GetStringSetting("Auth:BotClientSecret");
            public static string TWITCH_CLIENT_ID => GetStringSetting("Auth:TwitchClientId");
            public static string TWITCH_OAUTH_TOKEN => GetStringSetting("Auth:OAuthToken");
            public static string TWITCH_OAUTH_BEARER_BOT => GetStringSetting("Auth:OAuthBearerBot");
            public static string TWITCH_ACCESS_TOKEN => GetStringSetting("Auth:AccessToken");
            public static string TWITCH_APP_ACCESS_TOKEN => GetStringSetting("Auth:AppAccessToken");
            public static string REFRESH_TOKEN => GetStringSetting("Auth:RefreshToken");
        }

        public static class Ads
        {
            public static int AdInitialSeconds => GetIntSetting("Ads:AdInitialSeconds");
            public static int AdIntervalMinutes => GetIntSetting("Ads:AdIntervalMinutes");
            public static string DefaultUserName => GetStringSetting("Ads:DefaultUserName");
        }
        
        public static class WebHost
        {
            public static string BaseUrl => GetStringSetting("Webhost:BaseUrl");
            public static string WebRoot => GetStringSetting("Webhost:WebRoot");
        }

       public static class EventSub
        {
            public static string Uri => GetStringSetting("EventSub:Uri");
            public static string EVENTSUB_SECRET => GetStringSetting("EventSub:Secret");
            public static string EVENTSUB_CALLBACK_URL => GetStringSetting("EventSub:CallbackUrl");
            public static string PostSubscriptionsUrl => GetStringSetting("EventSub:PostSubscriptionsurl");
            public static string Validate => GetStringSetting("EventSub:Validate");
        }

        public static class MediaMapFiles
        {
            public static string TwitchAlertMedia => GetStringSetting("MediaMapFiles:TwitchAlertMedia");
            public static string ExcludedUsersMedia => GetStringSetting("MediaMapFiles:ExcludedUsersMedia");
            public static string FirstChattersMedia => GetStringSetting("MediaMapFiles:FirstChattersMedia");
            public static string CommandAlertMedia => GetStringSetting("MediaMapFiles:CommandAlertMedia");
            public static string UserWatchStreakMedia => GetStringSetting("MediaMapFiles:UserWatchStreakMedia");
        }

        public static class MediaBase
        {
            public static string TwitchAlertsFolder => GetStringSetting("MediaBase:TwitchAlertsFolder");
        }

        public static class Chatters
        {
            public static string ModsUrl => GetStringSetting("Chatters:ModsUrl");
            public static string VipUrl => GetStringSetting("Chatters:VipUrl");
            public static string InitialDelay => GetStringSetting("Chatters:InitialDelay");
            public static string ContinuousDelay => GetStringSetting("Chatters:ContinuousDelay");
        }

        public static class TTS
        {
            public static string Engine => GetStringSetting("TTS:Engine");
            public static string PollyRegion => GetStringSetting("TTS:PollyRegion");
            public static string DefaultSpeaker => GetStringSetting("TTS:DefaultSpeaker");
            public static int MaxChars => GetIntSetting("TTS:MaxChars");
            public static IReadOnlyCollection<string> AllowedVoices => GetListStringSetting("TTS:AllowedVoices");
        }

        public static class Voices
        {
            public static string Cheer => GetStringSetting("TTS:Voices:Cheer");
            public static string Subscribe => GetStringSetting("TTS:Voices:Subscribe");
            public static string SubscriptionMessage => GetStringSetting("TTS:Voices:SubscriptionMessage");
            public static string GiftSubs => GetStringSetting("TTS:Voices:GiftSubs");
            public static string Raid => GetStringSetting("TTS:Voices:Raid");
            public static string Follow => GetStringSetting("TTS:Voices:Follow");
            public static string SingleGiftSub => GetStringSetting("TTS:Voices:SingleGiftSub");
            public static string WatchStreak => GetStringSetting("TTS:Voices:WatchStreak");
            public static string ChannelPoints => GetStringSetting("TTS:Voices:ChannelPoints");
        }

        public static class Templates
        {
            public static string Follow => GetStringSetting("TTS:Templates:Follow");
            public static string Raid => GetStringSetting("TTS:Templates:Raid");
            public static string CheerNoMessage => GetStringSetting("TTS:Templates:CheerNoMessage");
            public static string SubNoMessage => GetStringSetting("TTS:Templates:SubNoMessage");
            public static string ReSub => GetStringSetting("TTS:Templates:ReSub");
            public static string MysteryGift => GetStringSetting("TTS:Templates:MysteryGift");
            public static string SingleGiftSub => GetStringSetting("TTS:Templates:SingleGiftSub");
            public static string WatchStreak => GetStringSetting("TTS:Templates:WatchStreak");
        }

        public static class Commands
        {
            public static string UsersByLogin => GetStringSetting("Commands:UsersByLogin");
            public static string StreamsByUserId => GetStringSetting("Commands:StreamsByUserId");
            public static string ChannelInfoByBroadcasterId => GetStringSetting("Commands:ChannelInfoByBroadcasterId");
            public static string VideosByUserId => GetStringSetting("Commands:VideosByUserId");
            public static string GamesByGameId => GetStringSetting("Commands:GamesByGameId");
        }

        public static class Moderation
        {
            public static HashSet<string> ClearSpecialAlertUsers => GetListStringSetting("Moderation:ClearSpecialAlertUsers");
        }

        public static class OpenAI
        {
            public static string BaseUrl => GetStringSetting("OpenAI:BaseUrl");
            public static string ApiKey => GetStringSetting("OpenAI:ApiKey");
            public static string Model => GetStringSetting("OpenAI:Model") ?? "gpt-4o-mini";
            public static string DefaultAlertTone => GetStringSetting("OpenAI:DefaultAlertTone");
            public static int DefaultAlertMaxWords => GetIntSetting("OpenAI:DefaultAlertMaxWords");
            public static int WatchStreakThreshold => GetIntSetting("OpenAI:WatchStreakThreshold");
            public static int GiftSubThreshold => GetIntSetting("OpenAI:GiftSubThreshold");
        }

        public static class AlertSettings
        {
            public static int AlertTimeOut => GetIntSetting("AlertSettings:AlertTimeOut");
            public static int AcknowledgeTimeOut => GetIntSetting("AlertSettings:AcknowledgeTimeOut");
            public static int MaxQueueSize => GetIntSetting("AlertSettings:MaxQueueSize");
            public static int MaxOfflineQueueTime => GetIntSetting("AlertSettings:MaxOfflineQueueTime");
            public static int RetryTaskDelay => GetIntSetting("AlertSettings:RetryTaskDelay");
        }

        public static string GetStringSetting(string key)
        {
            var value = Configuration![key];

            if (value == null)
            {
                throw new ArgumentOutOfRangeException($"Couldn't locate an application setting with key of: '{key}'");
            }

            return value ?? string.Empty;
        }

        public static HashSet<string> GetListStringSetting(string key)
        {
            // This supports both:
            // 1) JSON array: "ClearNukeUsers": ["a","b"]
            // 2) config style: "ClearNukeUsers": "a,b" (optional fallback)

            var section = Configuration!.GetSection(key);

            // If it’s a JSON array, it will have children
            if (section.Exists() && section.GetChildren().Any())
            {
                var list = section.Get<List<string>>() ?? new List<string>();
                return new HashSet<string>(
                    list
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            }

            // Optional fallback: comma-separated string
            var value = Configuration![key];

            if (string.IsNullOrWhiteSpace(value))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return new HashSet<string>(
                value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }
                
        public static int GetIntSetting(string key)
        {
            var value = Configuration![key];

            if (value == null)
            {
                throw new ArgumentOutOfRangeException($"Couldn't locate an application setting with key of: '{key}'");
            }

            return int.TryParse(value, out var result) ? result : 0;
        }
    }
}