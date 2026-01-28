using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public sealed class NukeService : INukeService, IDisposable
    {
        private readonly ILogger<NukeService> _logger;
        private readonly HashSet<string> _nukeUsed = new(StringComparer.OrdinalIgnoreCase);
        private Timer? _nukeResetTimer;

        private readonly TimeSpan _resetInterval = TimeSpan.FromHours(1);

        public NukeService(ILogger<NukeService> logger)
        {
            _logger = logger;
        }

        public bool TryUseNuke(string username)
        {
            if (_nukeUsed.Contains(username))
            {
                return false;
            }

            _nukeUsed.Add(username);
            return true;
        }

        public void ClearNukes()
        {
            _nukeUsed.Clear();
            _logger.LogInformation("♻️ Nukes cleared.");
        }

        public void StartNukeResetTimer()
        {
            _nukeResetTimer = new Timer(_ =>
            {
                ClearNukes();
                _logger.LogInformation("♻️ Nuke reset timer fired.");
            }, null, _resetInterval, _resetInterval);
        }

        public void StopNukeResetTimer()
        {
            _nukeResetTimer?.Dispose();
            _nukeResetTimer = null;
        }

        public void Dispose()
        {
            StopNukeResetTimer();
        }
    }
}
