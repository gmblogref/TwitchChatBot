using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TwitchChatBot.Core.Utilities
{
    public static class CoreHelperMethods
    {
        public static int GetRandomNumberForMediaSelection(int listLength)
        {
            Random random = new Random();

            return random.Next(listLength);
        }
    }
}
