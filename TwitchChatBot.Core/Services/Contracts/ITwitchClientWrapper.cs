using System;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchClientWrapper : IDisposable
    {
        void Connect();
        void Disconnect();
        void SendMessage(string channel, string message);
        event EventHandler<TwitchMessageEventArgs> OnMessageReceived;
    }
}