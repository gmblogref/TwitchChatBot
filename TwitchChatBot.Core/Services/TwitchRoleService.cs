using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class TwitchRoleService : ITwitchRoleService
    {
        private readonly ILogger<TwitchRoleService> _logger;
        private readonly HttpClient _httpClient;

        public TwitchRoleService(ILogger<TwitchRoleService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.TWITCH_ACCESS_TOKEN);
            _httpClient.DefaultRequestHeaders.Add("Client-Id", AppSettings.TWITCH_CLIENT_ID);
        }

        public async Task<List<string>> GetModeratorsAsync(string broadcasterId)
        {
            var mods = new List<string>();
            var url = $"https://api.twitch.tv/helix/moderation/moderators?broadcaster_id={broadcasterId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");

                foreach (var item in data.EnumerateArray())
                {
                    mods.Add(item.GetProperty("user_login").GetString()!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to fetch moderator list from Twitch.");
            }

            return mods;
        }

        public async Task<List<string>> GetVipsAsync(string broadcasterId)
        {
            var vips = new List<string>();
            var url = $"{AppSettings.Chatters.VipUrl}{broadcasterId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var data = doc.RootElement.GetProperty("data");

                foreach (var item in data.EnumerateArray())
                {
                    vips.Add(item.GetProperty("user_login").GetString()!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to fetch VIP list from Twitch.");
            }

            return vips;
        }
    }
}