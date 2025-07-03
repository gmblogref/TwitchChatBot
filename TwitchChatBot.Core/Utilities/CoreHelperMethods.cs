using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Utilities
{
    public static class CoreHelperMethods
    {
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