namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IAppFlags
    {
        bool IsTesting { get; set; } // when true: don’t update watch streaks, etc.
        bool IsReplay { get; }       // true only inside a replay scope

        IDisposable BeginReplayScope();
    }
}