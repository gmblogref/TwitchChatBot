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
        private bool _isProcessing = false;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter
        private static readonly TimeSpan AlertTimeout = TimeSpan.FromSeconds(AppSettings.AlertTimeOut);

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;

            // Subscribe to done events from WebSocketServer
            _webSocketServer.OnClientDone += HandleClientDone;
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
                TaskCompletionSource<bool>? tcs = null;

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

                    _currentAlertTcs = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    tcs = _currentAlertTcs;
                }

                try
                {
                    var payload = new
                    {
                        type = alert!.Type,
                        message = alert.Message,
                        media = alert.Media
                    };

                    await _webSocketServer.BroadcastAsync(payload).ConfigureAwait(false);

                    _logger.LogInformation(
                        "📤 Alert sent: {Type}, {Message}, Media: {Media}",
                        alert.Type,
                        alert.Message,
                        alert.Media);

                    var completed = await Task.WhenAny(
                        tcs!.Task,
                        Task.Delay(AlertTimeout)
                    ).ConfigureAwait(false);

                    if (completed != tcs.Task)
                    {
                        _logger.LogWarning("⏱️ Alert timed out waiting for DONE. Forcing completion.");
                        // IMPORTANT: complete it so this alert is “done” even if overlay is late
                        tcs.TrySetResult(false);
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
                        _isProcessing = false;

                        // Only clear the TCS if it’s still the same one we created.
                        // This prevents a late DONE from completing the wrong alert.
                        if (ReferenceEquals(_currentAlertTcs, tcs))
                        {
                            _currentAlertTcs = null;
                        }
                    }
                }

                // Loop again to process next item (if any)
            }
        }

        private void HandleClientDone()
        {
            _logger.LogInformation("✅ Client reported alert finished");

            TaskCompletionSource<bool>? tcs;

            lock (_sync)
            {
                tcs = _currentAlertTcs;
            }

            tcs?.TrySetResult(true);
        }
    }
}