using System;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchClientWrapper : IDisposable
    {
        void Connect();
        void Disconnect();
        void SendMessage(string channel, string message);
        List<ViewerEntry> GetGroupedViewers();
        void StartAdTimer();
        void StopAdTimer();

        event EventHandler<TwitchMessageEventArgs> OnMessageReceived;
        event EventHandler<List<ViewerEntry>>? OnViewerListChanged;
    }
}