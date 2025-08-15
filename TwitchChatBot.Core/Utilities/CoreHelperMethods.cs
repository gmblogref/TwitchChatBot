using System.Text.RegularExpressions;

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

        public static string ToPublicMediaPath(string mediaPath) =>
            "/media/" + mediaPath.Replace("\\", "/");
    }
}