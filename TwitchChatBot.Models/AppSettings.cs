using Microsoft.Extensions.Configuration;

namespace TwitchChatBot.Models
{
    public static class AppSettings
    {
        public static IConfiguration? Configuration { get; set; }

        public static string? TWITCH_BOT_USERNAME => GetSetting("AppSettings:TWITCH_BOT_USERNAME");
        public static string? TWITCH_OAUTH_TOKEN => GetSetting("AppSettings:TWITCH_OAUTH_TOKEN");
        public static string? TWITCH_CHANNEL => GetSetting("AppSettings:TWITCH_CHANNEL");
        public static string? BOT_CLIENT_ID => GetSetting("AppSettings:BOT_CLIENT_ID");
        public static string? BOT_CLIENT_SECRET => GetSetting("AppSettings:BOT_CLIENT_SECRET");
        public static string? TWITCH_ACCESS_TOKEN => GetSetting("AppSettings:TWITCH_ACCESS_TOKEN");
        public static string? TWITCH_CLIENT_ID => GetSetting("AppSettings:TWITCH_CLIENT_ID");
        public static string? REFRESH_TOKEN => GetSetting("AppSettings:REFRESH_TOKEN");
        public static string? TWITCH_USER_ID => GetSetting("AppSettings:TWITCH_USER_ID");
        public static string? TWITCH_APP_ACCESS_TOKEN => GetSetting("AppSettings:TWITCH_APP_ACCESS_TOKEN");

        public static int AdInitialMinutes => GetIntSetting("AppSettings:AdInitialMinutes"); 
        public static int AdIntervalMinutes => GetIntSetting("AppSettings:AdIntervalMinutes");

        public static class WebHost
        {
            public static string? BaseUrl => GetSetting("Webhost:BaseUrl");
            public static string? WebRoot => GetSetting("Webhost:WebRoot");
        }

        public static class Streamlabs
        {
            public static string? STREAMLABS_SOCKET_TOKEN => GetSetting("Streamlabs:STREAMLABS_SOCKET_TOKEN");
            public static string? Url => GetSetting("Streamlabs:Url");
        }

        public static class EventSub
        {
            public static string? Uri => GetSetting("EventSub:Uri");
            public static string? EVENTSUB_SECRET => GetSetting("AppSettings:EVENTSUB_SECRET");
            public static string? EVENTSUB_CALLBACK_URL => GetSetting("AppSettings:EVENTSUB_CALLBACK_URL");
        }

        public static class MediaMapFiles
        {
            public static string TwitchAlertMedia => GetSetting("MediaMapFiles:TwitchAlertMedia");
            public static string ExcludedUsersMedia => GetSetting("MediaMapFiles:ExcludedUsersMedia");
            public static string FirstChattersMedia => GetSetting("MediaMapFiles:FirstChattersMedia");
            public static string CommandAlertMedia => GetSetting("MediaMapFiles:CommandAlertMedia");
        }

        public static class Media
        {
            public static string TwitchAlertsFolder => GetSetting("Media:TwitchAlertsFolder");
        }

        public static class Chatters
        {
            public static string VipUrl => GetSetting("Chatters:VipUrl");
            public static string InitialDelay => GetSetting("Chatters:InitialDelay");
            public static string ContinuousDelay => GetSetting("Chatters:ContinuousDelay");
        }

        public static class TTS
        {
            public static string TtsExecutable => GetSetting("TTS:TtsExecutable");
            public static string DefaultModel => GetSetting("TTS:DefaultModel");
            public static string DefaultSpeaker => GetSetting("TTS:DefaultSpeaker");
        }

        public static string GetSetting(string key)
        {
            var value = Configuration![key];

            if(value == null)
            {
                throw new ArgumentOutOfRangeException($"Couldn't locate an application setting with key of: '{key}'");
            }

            return value;
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