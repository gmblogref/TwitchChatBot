using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

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

        private IUiBridge? _uiBridge; // <- Now nullable and injected via setter

        public ChatBotController(
        IAlertService alertService,
        IEventSubService eventSubService,
        ILogger<ChatBotController> logger,
        IStreamlabsService streamlabsService,
        ITwitchClientWrapper twitchClient,
        IWebSocketServer webSocketServer)
        {
            _alertService = alertService;
            _eventSubService = eventSubService;
            _logger = logger;
            _streamlabsService = streamlabsService;
            _twitchClient = twitchClient;
            _webSocketServer = webSocketServer;
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

                // Connect to Twitch
                _twitchClient.Connect();
                _logger.LogInformation("✅ Connected to Twitch.");

                _twitchClient.OnMessageReceived += (s, e) =>
                {
                    var formatted = $"[{e.Channel}] {e.Username}: {e.Message}";
                    _uiBridge!.AppendChat(formatted);
                    _logger.LogInformation("💬 {Chat}", formatted);
                };

                // Connect to Streamlabs
                _streamlabsService.Start(_alertService.EnqueueAlert);
                _logger.LogInformation("✅ Streamlabs WebSocket started.");

                // Connect to Twitch EventSub
                await _eventSubService.StartAsync(cancellationToken);
                _logger.LogInformation("✅ EventSub WebSocket started.");

                // Optional: Start any periodic timers like !ads
                _alertService.StartAdTimer(TimeSpan.FromMinutes(60));

                _logger.LogInformation("🎉 ChatBotController started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to start ChatBotController.");
            }
        }
    }
}