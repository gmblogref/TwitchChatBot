using TwitchChatBot.Core.Utilities.Contracts;

namespace TwitchChatBot.Core.Utilities
{
    public class CoreHelperMethods : ICoreHelperMethods
    {
        public int GetRandomNumberForMediaSelection(int listLength)
        {
            Random random = new Random();

            return random.Next(listLength);
        }
    }
}
