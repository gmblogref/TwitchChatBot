using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services;
using TwitchChatBot.Core.Services.Contracts;
using TwitchLib.Communication.Interfaces;

namespace TwitchChatBot.Core.Controller
{
    public class ChatBotController
    {
        private readonly IAlertService _alertService;
        private readonly IEventSubService _eventSubService;
        private readonly ILogger<ChatBotController> _logger;
        private readonly IStreamlabsService _streamlabsService;
        private readonly ITwitchClientWrapper _twitchClient;
        private readonly IWebSocketServer _webSocketServer;
        private readonly IWebHostWrapper _webHostWrapper;

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter

        public ChatBotController(
        IAlertService alertService,
        IEventSubService eventSubService,
        ILogger<ChatBotController> logger,
        IStreamlabsService streamlabsService,
        ITwitchClientWrapper twitchClient,
        IWebSocketServer webSocketServer,
        IWebHostWrapper webHostWrapper)
        {
            _alertService = alertService;
            _eventSubService = eventSubService;
            _logger = logger;
            _streamlabsService = streamlabsService;
            _twitchClient = twitchClient;
            _webSocketServer = webSocketServer;
            _webHostWrapper = webHostWrapper;
        }

        public void SetUiBridge(IUiBridge bridge)
        {
            _uiBridge = bridge;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🚀 Starting ChatBotController...");

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

                // 🔁 start polling on construct
                //_twitchClient.StartTmiFallbackTimer();
                //_logger.LogInformation("✅ Timer for get current views started.");

                // Connect to Streamlabs
                _streamlabsService.Start(_alertService.EnqueueAlert);
                _logger.LogInformation("✅ Streamlabs WebSocket started.");

                // Connect to Twitch EventSub
                await _eventSubService.StartAsync(cancellationToken);
                _logger.LogInformation("✅ EventSub WebSocket started.");

                _logger.LogInformation("🎉 ChatBotController started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start ChatBotController.");
            }
        }
    }
}