using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IWebSocketServer
    {
        void Start();
        void Stop();
        Task BroadcastAsync(object payload);
        Task HandleConnectionAsync(HttpContext context, WebSocket webSocket);
    }
}