namespace TwitchChatBot.Models
{
    public static class AlertHistoryType
    {
        public const string First = "first";                     // first chatter
        public const string Cmd = "cmd";                         // command text (e.g., !tts Joanna hello)
        public const string TwitchFollow = "twitch:follow";
        public const string TwitchRaid = "twitch:raid";
        public const string TwitchCheer = "twitch:cheer";
        public const string TwitchSub = "twitch:subscribe";      // first-time sub
        public const string TwitchSubMessage = "twitch:submsg";  // resub with message
        public const string TwitchSubGift = "twitch:subgift";    // single gift
        public const string TwitchMysteryGift = "twitch:mysterygift"; // anonymous bundle
        public const string TwitchBulkGift = "twitch:bulkgift";  // named bundle
        public const string ChannelPoint = "twitch:channelpoint";
        public const string Tts = "tts";                         // direct TTS replay (rare; usually use Cmd)
    }
}