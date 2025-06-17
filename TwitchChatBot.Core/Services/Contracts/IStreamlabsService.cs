namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IStreamlabsService
    {
        /// <summary>
        /// Starts the connection to the Streamlabs WebSocket.
        /// </summary>
        /// <param name="onFollowAlert">A delegate that handles follow alerts by queuing alerts.</param>
        void Start(Action<string, string?> onFollowAlert);

        /// <summary>
        /// Stops the Streamlabs WebSocket connection.
        /// </summary>
        void Stop();
    }
}