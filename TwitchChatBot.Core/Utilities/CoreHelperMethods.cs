using System.Text.RegularExpressions;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Utilities
{
    public static class CoreHelperMethods
    {
        private static readonly Regex AtMention = new Regex(@"@(\w+)", RegexOptions.Compiled);

        public static string ForTts(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var noAt = AtMention.Replace(s, "$1");
            return noAt.Trim();
        }

        public static string RenderTemplate(string template, IDictionary<string, string?> data)
        {
            // Replace {key} with value, ignore missing keys gracefully
            return Regex.Replace(template, @"\{(\w+)\}", m =>
            {
                var k = m.Groups[1].Value;
                return data.TryGetValue(k, out var v) && !string.IsNullOrEmpty(v) ? v! : string.Empty;
            });
        }
        
        public static int GetRandomNumberForMediaSelection(int listLength)
        {
            Random random = new Random();

            return random.Next(listLength);
        }

        public static string ReplacePlaceholders(string text, string username)
        {
            return text.Replace("[userName]", username);
        }

        public static string ToAbsoluteMediaPath(string mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath))
            {
                return string.Empty;
            }

            var cleaned = mediaPath.Replace("media", "").TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());

            return Path.Combine(
                AppSettings.MediaBase.TwitchAlertsFolder,
                cleaned
            );
        }

        public static string ToPublicMediaPath(string mediaPath) =>
            "/media/" + mediaPath.Replace("\\", "/");

        public static string GetTtsOutputFolder()
            => Path.Combine(AppSettings.MediaBase.TwitchAlertsFolder, "text_to_speech");

        public static string UnescapeTagValue(string? v)
        {
            if (string.IsNullOrEmpty(v))
            {
                return v ?? string.Empty;
            }

            return v
                .Replace(@"\s", " ")
                .Replace(@"\n", "\n")
                .Replace(@"\r", "\r")
                .Replace(@"\:", ";")
                .Replace(@"\\", @"\");
        }
    }
}