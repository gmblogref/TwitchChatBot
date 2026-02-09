using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Core.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;
        private readonly IWebSocketServer _webSocketServer;
        private readonly Queue<AlertItem> _alertQueue = new();
        private readonly object _sync = new();

        private TaskCompletionSource<bool>? _currentAlertTcs;
        private TaskCompletionSource<bool>? _currentAckTcs;
        private bool _isProcessing = false;
        private string? _currentAlertId;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter
        private static readonly TimeSpan AlertTimeout = TimeSpan.FromSeconds(AppSettings.AlertTimeOut);
        private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(3);

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;

            // Subscribe to done events from WebSocketServer
            _webSocketServer.OnClientDone += HandleClientDone;
            _webSocketServer.OnClientAck += HandleClientAck;
        }

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        // Overload for default alert type
        public void EnqueueAlert(string message, string? mediaPath = null)
        {
            EnqueueAlert("alert", message, mediaPath);
        }

        // New overload that lets you specify type
        public void EnqueueAlert(string type, string message, string? mediaPath = null)
        {
            // If media is not found do not try to play it
            if (!string.IsNullOrWhiteSpace(mediaPath))
            {
                var absPath = CoreHelperMethods.ToAbsoluteMediaPath(mediaPath);

                if (!File.Exists(absPath))
                {
                    _logger.LogWarning(
                        "⚠️ Alert skipped — media file not found. Type={Type} MediaPath={MediaPath}",
                        type,
                        absPath);

                    return;
                }
            }

            lock (_sync)
            {
                _alertQueue.Enqueue(new AlertItem
                {
                    Type = type,
                    Message = message,
                    Media = mediaPath
                });
            }

            _ = ProcessQueueAsync();
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                AlertItem? alert = null;
                TaskCompletionSource<bool>? alertTcs = null;
                TaskCompletionSource<bool>? ackTcs = null;
                string? alertId = null;

                lock (_sync)
                {
                    if (_isProcessing)
                    {
                        return;
                    }

                    if (_alertQueue.Count == 0)
                    {
                        return;
                    }

                    _isProcessing = true;

                    alert = _alertQueue.Dequeue();

                    alertId = Guid.NewGuid().ToString("N");
                    _currentAlertId = alertId;

                    _currentAckTcs = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    _currentAlertTcs = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    ackTcs = _currentAckTcs;
                    alertTcs = _currentAlertTcs;
                }

                try
                {
                    var payload = new
                    {
                        type = alert!.Type,
                        alertId,
                        message = alert.Message,
                        media = alert.Media
                    };
                    
                    if (!_webSocketServer.HasClientsConnected)
                    {
                        _logger.LogWarning("⚠️ No overlay websocket clients connected. Skipping alert.");
                        continue;
                    }
                    await _webSocketServer.BroadcastAsync(payload).ConfigureAwait(false);

                    _logger.LogInformation(
                        "📤 Alert sent: {Type}, {Message}, Media: {Media}",
                        alert.Type,
                        alert.Message,
                        alert.Media);

                    var ackCompleted = await Task.WhenAny(
                        ackTcs!.Task,
                        Task.Delay(AckTimeout)
                        ).ConfigureAwait(false);

                    if (ackCompleted != ackTcs.Task)
                    {
                        _logger.LogWarning("⚠️ Alert not ACKed by overlay within {Seconds}s. Skipping. AlertId={AlertId}", AckTimeout.TotalSeconds, alertId);
                        ackTcs.TrySetResult(false);
                        continue;
                    }

                    var completed = await Task.WhenAny(
                        alertTcs!.Task,
                        Task.Delay(AlertTimeout)
                    ).ConfigureAwait(false);

                    if (completed != alertTcs.Task)
                    {
                        _logger.LogWarning("⏱️ Alert timed out waiting for DONE. Forcing completion.");
                        // IMPORTANT: complete it so this alert is “done” even if overlay is late
                        alertTcs.TrySetResult(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to process alert.");
                }
                finally
                {
                    lock (_sync)
                    {
                        if (ReferenceEquals(_currentAlertTcs, alertTcs))
                        {
                            _currentAlertTcs = null;
                        }

                        if (ReferenceEquals(_currentAckTcs, ackTcs))
                        {
                            _currentAckTcs = null;
                        }

                        if (string.Equals(_currentAlertId, alertId, StringComparison.Ordinal))
                        {
                            _currentAlertId = null;
                        }
                    }
                }

                // Loop again to process next item (if any)
            }
        }

        private void HandleClientDone(string? alertId)
        {
            TaskCompletionSource<bool>? tcs;
            string? currentId;

            lock (_sync)
            {
                tcs = _currentAlertTcs;
                currentId = _currentAlertId;
            }

            if (!string.IsNullOrWhiteSpace(alertId) && !string.Equals(alertId, currentId, StringComparison.Ordinal))
            {
                _logger.LogWarning("⚠️ DONE ignored for non-current alert. DoneAlertId={DoneAlertId} CurrentAlertId={CurrentAlertId}", alertId, currentId);
                return;
            }

            _logger.LogInformation("✅ Client reported alert finished. AlertId={AlertId}", alertId ?? currentId);
            tcs?.TrySetResult(true);
        }

        private void HandleClientAck(string? alertId)
        {
            TaskCompletionSource<bool>? tcs;
            string? currentId;

            lock (_sync)
            {
                tcs = _currentAckTcs;
                currentId = _currentAlertId;
            }

            if (!string.IsNullOrWhiteSpace(alertId) && !string.Equals(alertId, currentId, StringComparison.Ordinal))
            {
                _logger.LogWarning("⚠️ ACK ignored for non-current alert. AckAlertId={AckAlertId} CurrentAlertId={CurrentAlertId}", alertId, currentId);
                return;
            }

            _logger.LogDebug("📥 Client ACK received. AlertId={AlertId}", alertId ?? currentId);
            tcs?.TrySetResult(true);
        }
    }
}