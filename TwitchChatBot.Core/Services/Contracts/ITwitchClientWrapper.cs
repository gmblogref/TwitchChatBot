using System;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchClientWrapper : IDisposable
    {
        void Connect();
        void Disconnect();
        void SendMessage(string channel, string message);
        List<string> GetCurrentSortedViewers();

        event EventHandler<TwitchMessageEventArgs> OnMessageReceived;
        event Action<List<string>>? OnViewerListChanged;
    }
}