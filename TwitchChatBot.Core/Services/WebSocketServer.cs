using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class WebSocketServer : IWebSocketServer
    {
        private sealed class SocketState
        {
            public DateTime LastPongUtc { get; set; } = DateTime.UtcNow;
        }

        private readonly ConcurrentDictionary<WebSocket, SocketState> _sockets = new();
        private readonly ILogger<WebSocketServer> _logger;
        private CancellationTokenSource? _keepAliveCts;
        private Task? _keepAliveTask;

        public event Action<string?>? OnClientDone;
        public event Action<string?>? OnClientAck;
        public bool HasClientsConnected => _sockets.Any(kvp => kvp.Key.State == WebSocketState.Open);
        
        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            if (_keepAliveCts != null)
            {
                return;
            }

            _keepAliveCts = new CancellationTokenSource();
            _keepAliveTask = Task.Run(() => KeepAliveLoopAsync(_keepAliveCts.Token));
        }

        public void Stop()
        {
            try
            {
                if (_keepAliveCts != null)
                {
                    _keepAliveCts.Cancel();
                    _keepAliveCts.Dispose();
                    _keepAliveCts = null;
                    _keepAliveTask = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed stopping KeepAlive loop.");
            }

            foreach (var socket in _sockets.Keys.ToList())
            {
                try
                {
                    SafeRemoveSocket(socket);

                    if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                    {
                        socket.Abort(); // immediate teardown, no deadlocks
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to close WebSocket connection.");
                }
            }
            _logger.LogInformation("🛑 All WebSocket connections closed.");
        }

        public async Task BroadcastAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);

            var socketsSnapshot = _sockets.Keys.ToList();
            _logger.LogDebug("📣 Broadcasting to {Count} websocket client(s). Payload bytes={Bytes}", socketsSnapshot.Count, bytes.Length);

            foreach (var socket in socketsSnapshot)
            {
                if (socket.State != WebSocketState.Open)
                {
                    SafeRemoveSocket(socket);
                    continue;
                }

                try
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ WebSocket send failed. Removing client.");
                    SafeRemoveSocket(socket);

                    try
                    {
                        socket.Abort();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        public async Task HandleConnectionAsync(HttpContext context, WebSocket webSocket)
        {
            _sockets.TryAdd(webSocket, new SocketState());
            _logger.LogInformation("🔌 WebSocket connected. Total: {Count}", _sockets.Count);

            try
            {
                await ReceiveLoopAsync(webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ WebSocket receive loop crashed.");
            }
            finally
            {
                SafeRemoveSocket(webSocket);

                try
                {
                    if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                    {
                        webSocket.Abort();
                    }
                }
                catch
                {
                    // ignore
                }

                _logger.LogInformation("🔌 WebSocket disconnected. Total: {Count}", _sockets.Count);
            }
        }

        private async Task ReceiveLoopAsync(WebSocket webSocket)
        {
            var buffer = new byte[8 * 1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    try
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch
                    {
                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        try
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }
                        catch
                        {
                            // ignore
                        }

                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Ignore binary messages
                        continue;
                    }

                    if (result.Count > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                }
                while (!result.EndOfMessage);

                var message = sb.ToString();
                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                if (TryHandleControlMessage(webSocket, message))
                {
                    continue;
                }
            }
        }

        private bool TryHandleControlMessage(WebSocket webSocket, string message)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);


                if (_sockets.TryGetValue(webSocket, out var state))
                {
                    state.LastPongUtc = DateTime.UtcNow;
                }

                if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                {
                    return false;
                }

                var type = typeProp.GetString();
                if (string.IsNullOrWhiteSpace(type))
                {
                    return false;
                }

                string? alertId = null;
                if (doc.RootElement.TryGetProperty("alertId", out var alertIdProp))
                {
                    alertId = alertIdProp.GetString();
                }

                if (string.Equals(type, "pong", StringComparison.OrdinalIgnoreCase))
                {
                    if (_sockets.TryGetValue(webSocket, out var socketState2))
                    {
                        socketState2.LastPongUtc = DateTime.UtcNow;
                    }
                    return true;
                }

                if (string.Equals(type, "ack", StringComparison.OrdinalIgnoreCase))
                {
                    OnClientAck?.Invoke(alertId);
                    return true;
                }

                if (string.Equals(type, "done", StringComparison.OrdinalIgnoreCase))
                {
                    OnClientDone?.Invoke(alertId);
                    return true;
                }
            }
            catch
            {
                // ignore malformed control messages
            }

            return false;
        }

        private async Task KeepAliveLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);

                    // lightweight ping that also helps you see "is overlay alive"
                    await BroadcastAsync(new { type = "ping", utc = DateTimeOffset.UtcNow });

                    var now = DateTime.UtcNow;
                    var staleSockets = _sockets
                        .Where(kvp => kvp.Key.State == WebSocketState.Open && (now - kvp.Value.LastPongUtc) > TimeSpan.FromSeconds(45))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var socket in staleSockets)
                    {
                        _logger.LogWarning("🧯 Overlay client stale (no pong). Removing socket.");
                        SafeRemoveSocket(socket);

                        try
                        {
                            socket.Abort();
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ KeepAlive loop error.");
                }
            }
        }

        private void SafeRemoveSocket(WebSocket socket)
        {
            _sockets.TryRemove(socket, out _);
        }
    }
}