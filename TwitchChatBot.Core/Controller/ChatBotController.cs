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
        private readonly IAppFlags _appFlags;

        private int _started; // 0 = false, 1 = true
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
        IWatchStreakService watchStreakService,
        IAppFlags appFlags)
        {
            _ccommandAlertService = ccommandAlertService;
            _eventSubService = eventSubService;
            _logger = logger;
            _twitchClient = twitchClient;
            _tsService = tsService;
            _webSocketServer = webSocketServer;
            _webHostWrapper = webHostWrapper;
            _watchStreakService = watchStreakService;
            _appFlags = appFlags;
        }

        public bool IsStarted => Interlocked.CompareExchange(ref _started, 1, 1) == 1;

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🚀 Starting ChatBotController...");

                if (Interlocked.Exchange(ref _started, 1) == 1)
                    return; // already started

                // Start WebSocket server to communicate with front end
                _webSocketServer.Start();

                await _webHostWrapper.StartAsync(cancellationToken);

                // Connect to Twitch
                _twitchClient.Connect();
                _logger.LogInformation("✅ Connected to Twitch.");

                _twitchClient.OnMessageReceived += (s, e) =>
                {
                    _uiBridge!.AppendChat(e.Username, e.Message, e.Color);
                };

                _twitchClient.OnViewerListChanged += (s, viewers) =>
                {
                    _logger.LogInformation("✅ SetViewerListByGroup started.");
                    _uiBridge!.SetViewerListByGroup(viewers);
                    _logger.LogInformation("✅ SetViewerListByGroup finished.");
                };

                // Optional: Start any periodic timers like !ads
                _twitchClient.StartAdTimer();
                _logger.LogInformation("✅ Timer for ads started.");

                // Connect to Twitch EventSub
                await _eventSubService.StartAsync(cancellationToken);
                _logger.LogInformation("✅ EventSub WebSocket started.");

                await _watchStreakService.BeginStreamAsync();

                _logger.LogInformation("🎉 ChatBotController started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start ChatBotController.");
            }
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            if (Interlocked.Exchange(ref _started, 0) == 0)
                return; // already stopped / never started

            _logger.LogInformation("🛑 Stopping ChatBotController…");

            // stop in a forgiving way; each try/catch prevents one failure from blocking others
            try { _twitchClient.StartAdTimer(); } catch (Exception ex) { _logger.LogWarning(ex, "Stop ads"); }
                    
            try { await _eventSubService.StopAsync(ct); } catch (Exception ex) { _logger.LogWarning(ex, "Stop EventSub"); }

            try { _twitchClient.Dispose(); } catch (Exception ex) { _logger.LogWarning(ex, "Dispose Twitch"); }

            try { _twitchClient.Disconnect(); } catch (Exception ex) { _logger.LogWarning(ex, "Disconnect Twitch"); }

            try { (_tsService as IDisposable)?.Dispose(); } catch (Exception ex) { _logger.LogWarning(ex, "Dispose TTS"); }

            try { await _watchStreakService.EndStreamAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "End stream"); }

            _logger.LogInformation("🏁 ChatBotController stopped.");
        }
    }
}