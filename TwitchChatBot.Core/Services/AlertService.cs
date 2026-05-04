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

        private readonly SemaphoreSlim _queueSignal = new(0, int.MaxValue);
        private readonly CancellationTokenSource _processorCts = new();
        private readonly Task _processorTask;

        private TaskCompletionSource<bool>? _currentAlertTcs;
        private TaskCompletionSource<bool>? _currentAckTcs;
        private bool _isProcessing = false;
        private string? _currentAlertId;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter
        private static readonly TimeSpan AlertTimeout = TimeSpan.FromSeconds(AppSettings.AlertSettings.AlertTimeOut);
        private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(AppSettings.AlertSettings.AcknowledgeTimeOut);

        private DateTime? _overlayOfflineSinceUtc;
        private static int MaxQueueSize = AppSettings.AlertSettings.MaxQueueSize;
        private static readonly TimeSpan MaxOfflineQueueTime = TimeSpan.FromSeconds(AppSettings.AlertSettings.MaxOfflineQueueTime);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(AppSettings.AlertSettings.RetryTaskDelay);

        public AlertService(ILogger<AlertService> logger, IWebSocketServer webSocketServer)
        {
            _logger = logger;
            _webSocketServer = webSocketServer;

            // Subscribe to done events from WebSocketServer
            _webSocketServer.OnClientDone += HandleClientDone;
            _webSocketServer.OnClientAck += HandleClientAcknowledge;
            _webSocketServer.OnClientConnected += HandleClientConnected;

            // Start one dedicated processor loop for the lifetime of this service
            _processorTask = Task.Run(() => ProcessorLoopAsync(_processorCts.Token), _processorCts.Token);
        }

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        // Overload for default alert type
        public void EnqueueAlert(string message, string? mediaPath = null)
        {
            EnqueueAlert(new AlertItem
			{
				Type = "alert",
				Message = message,
				MediaPath = mediaPath
			});
        }

		public void EnqueueAlert(AlertItem alert)
		{
			// If media is not found do not try to play it
			if (!string.IsNullOrWhiteSpace(alert.MediaPath))
			{
				var absPath = CoreHelperMethods.ToAbsoluteMediaPath(alert.MediaPath);

				if (!File.Exists(absPath))
				{
					_logger.LogWarning(
						"⚠️ Alert skipped — media file not found. Type={Type} MediaPath={MediaPath}",
						alert.Type,
						absPath);

					return;
				}
			}

			lock (_sync)
			{
				if (_alertQueue.Count >= MaxQueueSize)
				{
					_logger.LogWarning("⚠️ Alert queue full ({Max}). Dropping oldest alert.", MaxQueueSize);
					_alertQueue.Dequeue();
				}

				_alertQueue.Enqueue(alert);
			}

			SignalQueue();
		}

		private void SignalQueue()
        {
            try
            {
                _queueSignal.Release();
            }
            catch
            {
                // ignore
            }
        }

        private async Task ProcessorLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _queueSignal.WaitAsync(token).ConfigureAwait(false);
                    await ProcessQueueAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Alert processor loop crashed; continuing.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
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
                        media = alert.MediaPath
                    };

                    if (!_webSocketServer.HasClientsConnected)
                    {
                        var now = DateTime.UtcNow;

                        if (_overlayOfflineSinceUtc == null)
                        {
                            _overlayOfflineSinceUtc = now;
                        }

                        var offlineFor = now - _overlayOfflineSinceUtc.Value;

                        if (offlineFor >= MaxOfflineQueueTime)
                        {
                            _logger.LogWarning("🧹 Overlay offline for {Seconds}s. Dropping alert to prevent backlog.", offlineFor.TotalSeconds);
                            // drop this alert and keep processing anything else (or just return; either is fine)
                            continue;
                        }

                        // OPTIONAL: throttle this log if it's still noisy
                        _logger.LogWarning("⏸️ No overlay websocket clients connected. Holding alerts (offline {Seconds}s).", offlineFor.TotalSeconds);

                        lock (_sync)
                        {
                            RequeueAtFront(alert);
                        }

                        // IMPORTANT: Stop processing now.
                        // ProcessorLoopAsync will wait until the next SignalQueue() (client connects or new alert).
                        return;
                    }

                    await _webSocketServer.BroadcastAsync(payload).ConfigureAwait(false);

                    _logger.LogInformation(
                        "📤 Alert sent: {Type}, {Message}, Media: {Media}",
                        alert.Type,
                        alert.Message,
                        alert.MediaPath);

                    var ackCompleted = await Task.WhenAny(
                        ackTcs!.Task,
                        Task.Delay(AckTimeout, token)
                    ).ConfigureAwait(false);

                    if (ackCompleted != ackTcs.Task)
                    {
                        var now = DateTime.UtcNow;

                        if (_overlayOfflineSinceUtc == null)
                        {
                            _overlayOfflineSinceUtc = now;
                        }

                        var offlineFor = now - _overlayOfflineSinceUtc.Value;

                        if (offlineFor >= MaxOfflineQueueTime)
                        {
                            _logger.LogWarning("🧹 Overlay not ACKing for {Seconds}s. Dropping alert to prevent backlog. AlertId={AlertId}", offlineFor.TotalSeconds, alertId);
                            ackTcs.TrySetResult(false);
                            continue;
                        }

                        _logger.LogWarning("⏸️ Alert not ACKed within {Seconds}s. Holding alert (offline {Seconds}s). AlertId={AlertId}", AckTimeout.TotalSeconds, offlineFor.TotalSeconds, alertId);

                        ackTcs.TrySetResult(false);

                        RequeueAtFront(alert);

                        await Task.Delay(RetryDelay, token).ConfigureAwait(false);
                        continue;
                    }

                    // ACK succeeded -> overlay is alive
                    _overlayOfflineSinceUtc = null;

                    var completed = await Task.WhenAny(
                        alertTcs!.Task,
                        Task.Delay(AlertTimeout, token)
                    ).ConfigureAwait(false);

                    if (completed != alertTcs.Task)
                    {
                        _logger.LogWarning("⏱️ Alert timed out waiting for DONE. Forcing completion. AlertId={AlertId}", alertId);
                        alertTcs.TrySetResult(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to process alert. Re-queueing last alert.");

                    if (alert != null)
                    {
                        RequeueAtFront(alert);
                    }

                    try
                    {
                        await Task.Delay(RetryDelay, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore
                    }

                    continue;
                }
                finally
                {
                    lock (_sync)
                    {
                        _isProcessing = false;

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

            if (string.IsNullOrWhiteSpace(alertId))
            {
                _logger.LogWarning("⚠️ DONE ignored because alertId is missing. CurrentAlertId={CurrentAlertId}", currentId);
                return;
            }

            if (!string.Equals(alertId, currentId, StringComparison.Ordinal))
            {
                _logger.LogWarning("⚠️ DONE ignored for non-current alert. DoneAlertId={DoneAlertId} CurrentAlertId={CurrentAlertId}", alertId, currentId);
                return;
            }

            _logger.LogInformation("✅ Client reported alert finished. AlertId={AlertId}", alertId);
            tcs?.TrySetResult(true);
        }

        private void HandleClientAcknowledge(string? alertId)
        {
            TaskCompletionSource<bool>? tcs;
            string? currentId;

            lock (_sync)
            {
                tcs = _currentAckTcs;
                currentId = _currentAlertId;
            }

            if (string.IsNullOrWhiteSpace(alertId))
            {
                _logger.LogWarning("⚠️ ACK ignored because alertId is missing. CurrentAlertId={CurrentAlertId}", currentId);
                return;
            }

            if (!string.Equals(alertId, currentId, StringComparison.Ordinal))
            {
                _logger.LogWarning("⚠️ ACK ignored for non-current alert. AckAlertId={AckAlertId} CurrentAlertId={CurrentAlertId}", alertId, currentId);
                return;
            }

            _logger.LogDebug("📥 Client ACK received. AlertId={AlertId}", alertId);
            tcs?.TrySetResult(true);
        }

        private void RequeueAtFront(AlertItem alert)
        {
            lock (_sync)
            {
                if (_alertQueue.Count == 0)
                {
                    _alertQueue.Enqueue(alert);
                    return;
                }

                var reordered = new Queue<AlertItem>(_alertQueue.Count + 1);
                reordered.Enqueue(alert);

                while (_alertQueue.Count > 0)
                {
                    reordered.Enqueue(_alertQueue.Dequeue());
                }

                while (reordered.Count > 0)
                {
                    _alertQueue.Enqueue(reordered.Dequeue());
                }
            }
        }

        private void HandleClientConnected()
        {
            _logger.LogInformation("🔔 Overlay client connected. Signaling alert queue.");
            SignalQueue();
        }
    }
}