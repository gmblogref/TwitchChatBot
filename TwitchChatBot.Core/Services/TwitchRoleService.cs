using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class TwitchRoleService : ITwitchRoleService
    {
        private readonly ILogger<TwitchRoleService> _logger;
        private readonly IHelixLookupService _helixLookupService;

        public TwitchRoleService(ILogger<TwitchRoleService> logger,
            IHelixLookupService helixLookupService)
        {
            _logger = logger;
            _helixLookupService = helixLookupService;
        }

        public async Task<List<string>> GetModeratorsAsync(string broadcasterId)
        {
            try
            {
                var mods = await _helixLookupService.GetModeratorLoginsAsync(broadcasterId);
                return mods.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to fetch moderator list from Twitch.");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetVipsAsync(string broadcasterId)
        {
            try
            {
                var vips = await _helixLookupService.GetVipLoginsAsync(broadcasterId);
                return vips.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to fetch VIP list from Twitch.");
                return new List<string>();
            }
        }
    }
}