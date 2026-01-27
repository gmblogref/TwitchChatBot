namespace TwitchChatBot.Core.Services.Contracts
{
    public interface ITwitchClientWrapper : IDisposable
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendMessageAsync(string channel, string message);
        List<ViewerEntry> GetGroupedViewers();
        void SendMessage(string channel, string message); 
        void StartAdTimer();
        void StopAdTimer();

        event EventHandler<TwitchMessageEventArgs> OnMessageReceived;
        event EventHandler<List<ViewerEntry>>? OnViewerListChanged;
    }
}