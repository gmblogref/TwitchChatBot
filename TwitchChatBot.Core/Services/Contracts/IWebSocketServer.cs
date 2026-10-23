using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWebSocketServer
    {
        event Action? OnClientConnected;
        event Action<string?>? OnClientDone;
        event Action<string?>? OnClientAck;

        bool HasClientsConnected { get; }
		IReadOnlyList<string> ActiveTtsAlertIds { get; }
		void ClearActiveTtsAlertIds(IReadOnlyCollection<string> alertIds);
        void Start();
        void Stop();
        Task BroadcastAsync(object payload);
        Task HandleConnectionAsync(HttpContext context, WebSocket webSocket);
    }
}
