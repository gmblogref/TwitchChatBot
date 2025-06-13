using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchClient
    {
        void Connect();
        void SendMessage(string channel, string message);
        event EventHandler<TwitchMessageEventArgs> OnMessageReceived;
    }
}
