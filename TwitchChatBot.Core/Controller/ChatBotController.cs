using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Controller
{
    public class ChatBotController
    {
        private readonly ICommandAlertService _ccommandAlertService;
        private readonly IEventSubService _eventSubService;
        private readonly ILogger<ChatBotController> _logger;
        private readonly ITtsService _tsService;
        private readonly ITwitchClientWrapper _twitchClient;
        private readonly IWebSocketServer _webSocketServer;
        private readonly IWebHostWrapper _webHostWrapper;
        private readonly IWatchStreakService _watchStreakService;
        
        private int _started; // 0 = false, 1 = true
        private bool _twitchEventsWired;
        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter
        private readonly TimeSpan _nukeResetInterval = TimeSpan.FromMinutes(25);

        public ChatBotController(
        ICommandAlertService ccommandAlertService,
        IEventSubService eventSubService,
        ILogger<ChatBotController> logger,
        ITtsService tsService,
        ITwitchClientWrapper twitchClient,
        IWebSocketServer webSocketServer,
        IWebHostWrapper webHostWrapper,
        IWatchStreakService watchStreakService)
        {
            _ccommandAlertService = ccommandAlertService;
            _eventSubService = eventSubService;
            _logger = logger;
            _twitchClient = twitchClient;
            _tsService = tsService;
            _webSocketServer = webSocketServer;
            _webHostWrapper = webHostWrapper;
            _watchStreakService = watchStreakService;
        }

        public bool IsStarted => Interlocked.CompareExchange(ref _started, 1, 1) == 1;

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🚀 Starting ChatBotController...");

            if (Interlocked.Exchange(ref _started, 1) == 1)
            {
                _logger.LogWarning("StartAsync ignored; already started.");
                return;
            }

            try
            {
                _logger.LogInformation("Start: WebSocketServer.Start()");
                _webSocketServer.Start();

                _logger.LogInformation("Start: WebHostWrapper.StartAsync()");
                await _webHostWrapper.StartAsync(cancellationToken);

                // Wire Twitch events BEFORE connect so we don't miss early messages
                WireTwitchEvents();

                _logger.LogInformation("Start: TwitchClient.ConnectAsync()");
                await _twitchClient.ConnectAsync();
                _logger.LogInformation("✅ Connected to Twitch.");

                _logger.LogInformation("Start: EventSub.StartAsync()");
                await _eventSubService.StartAsync(cancellationToken);

                _logger.LogInformation("Start: WatchStreak.BeginStreamAsync()");
                await _watchStreakService.BeginStreamAsync();

                _logger.LogInformation("Start: TwitchClient.StartAdTimer()");
                _twitchClient.StartAdTimer();

                _logger.LogInformation("🎉 ChatBotController started successfully.");
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref _started, 0);
                _logger.LogError(ex, "❌ Failed to start ChatBotController (see exception).");

                // Optional: rollback partial start so next Start works
                try 
                { 
                    await StopBotAsync(cancellationToken); 
                } 
                catch 
                { /* ignore */ }
            }
        }

        public async Task StopBotAsync(CancellationToken ct = default)
        {
            if (Interlocked.Exchange(ref _started, 0) == 0)
            {
                return; // already stopped / never started
            }

            _logger.LogInformation("🛑 StopBotAsync: stopping bot activity (restart-safe)…");

            try { _twitchClient.StopAdTimer(); } catch (Exception ex) { _logger.LogWarning(ex, "Stop ads"); }

            try { await _watchStreakService.EndStreamAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "End stream"); }
            try { await _watchStreakService.FlushSavesAsync(ct); } catch (Exception ex) { _logger.LogWarning(ex, "Flush watch streak saves"); }

            try { await _eventSubService.StopAsync(ct); } catch (Exception ex) { _logger.LogWarning(ex, "Stop EventSub"); }

            try { await _twitchClient.DisconnectAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "Disconnect Twitch"); }

            // Unwire after disconnect (either is fine), but do it consistently
            UnwireTwitchEvents();

            try { _webSocketServer.Stop(); } catch (Exception ex) { _logger.LogWarning(ex, "Stop WebSocketServer"); }
            try { await _webHostWrapper.StopAsync(ct); } catch (Exception ex) { _logger.LogWarning(ex, "Stop WebHost"); }

            _logger.LogInformation("🏁 StopBotAsync complete. OverlayClientsConnected={OverlayClientsConnected}", _webSocketServer.HasClientsConnected);
        }

        public async Task ShutdownAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("🛑 ShutdownAsync: full application shutdown…");

            try 
            { 
                await StopBotAsync(ct); 
            } 
            catch (Exception ex) 
            { 
                _logger.LogWarning(ex, "StopBot during shutdown"); 
            }

            // Dispose only on app exit (singletons must not be reused after this)
            try 
            { 
                (_tsService as IDisposable)?.Dispose();
            } 
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, "Dispose TTS"); 
            }

            try 
            { 
                _twitchClient.Dispose();
            } 
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, "Dispose Twitch"); 
            }

            _logger.LogInformation("🏁 ShutdownAsync complete.");
        }

        private void WireTwitchEvents()
        {
            if (_twitchEventsWired)
            {
                return;
            }

            _twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
            _twitchClient.OnViewerListChanged += TwitchClient_OnViewerListChanged;

            _twitchEventsWired = true;

            _logger.LogInformation("🔗 Twitch events wired.");
        }

        private void UnwireTwitchEvents()
        {
            if (!_twitchEventsWired)
            {
                return;
            }

            _twitchClient.OnMessageReceived -= TwitchClient_OnMessageReceived;
            _twitchClient.OnViewerListChanged -= TwitchClient_OnViewerListChanged;

            _twitchEventsWired = false;

            _logger.LogInformation("🧵 Twitch events unwired.");
        }

        private void TwitchClient_OnMessageReceived(object? sender, TwitchMessageEventArgs e)
        {
            if (_uiBridge == null)
            {
                return;
            }

            _uiBridge.AppendChat(e.Username, e.Message, e.Color);
        }

        private void TwitchClient_OnViewerListChanged(object? sender, List<ViewerEntry> viewers)
        {
            if (_uiBridge == null)
            {
                return;
            }

            _logger.LogInformation("✅ SetViewerListByGroup started.");
            _uiBridge.SetViewerListByGroup(viewers);
            _logger.LogInformation("✅ SetViewerListByGroup finished.");
        }
    }
}