using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class WebSocketServer : IWebSocketServer
    {
        private readonly ILogger<WebSocketServer> _logger;
        private readonly ConcurrentBag<WebSocket> _sockets = new();

        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            // Hook into Startup middleware manually if needed
        }

        public void Stop()
        {
            foreach (var socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to close WebSocket connection.");
                    }
                }
            }
            _logger.LogInformation("🛑 All WebSocket connections closed.");
        }

        public async Task BroadcastAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            foreach (var socket in _sockets.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to send WebSocket message.");
                    }
                }
                else
                {
                    _sockets.TryTake(out _); // Clean up closed sockets
                }
            }
        }

        public async Task HandleConnectionAsync(HttpContext context, WebSocket webSocket)
        {
            _sockets.Add(webSocket);
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }

            _sockets.TryTake(out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }
    }
}