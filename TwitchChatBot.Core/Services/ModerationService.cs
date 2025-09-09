using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IHttpClientFactory _httpFactory;

        public ModerationService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        }

        public async Task<string> GetUserIdAsync(string login, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Login is required.", nameof(login));
            }

            var http = _httpFactory.CreateClient("twitch-bot-helix");

            using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users?login={login.Trim()}");
            using var res = await http.SendAsync(req, ct);
            var json = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"GetUserId failed {(int)res.StatusCode}: {json}");
            }

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"User not found: {login}");
            }

            return data[0].GetProperty("id").GetString()!;
        }

        /// <summary>
        /// Timeouts a user via Helix moderation/bans. Requires bot to be a mod in broadcaster's channel and bot token to include moderator:manage:banned_users.
        /// </summary>
        public async Task TimeoutAsync(
            string broadcasterId, 
            string moderatorId, 
            string targetUserId, 
            int seconds, 
            bool useBot = true,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(broadcasterId))
            {
                throw new ArgumentException("Broadcaster ID is required.", nameof(broadcasterId));
            }

            if (string.IsNullOrWhiteSpace(moderatorId))
            {
                throw new ArgumentException("Moderator ID is required.", nameof(moderatorId));
            }

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                throw new ArgumentException("Target user ID is required.", nameof(targetUserId));
            }

            if (seconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Duration must be > 0.");
            }

            // Pick the right identity for Helix call
            var http = useBot
                ? _httpFactory.CreateClient("twitch-bot-helix")
                : _httpFactory.CreateClient("twitch-helix");

            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"https://api.twitch.tv/helix/moderation/bans?broadcaster_id={broadcasterId}&moderator_id={moderatorId}");

            // Pull tokens fresh (however you store them). Example using AppSettings:
            var bearer = useBot
                ? AppSettings.TWITCH_OAUTH_BEARER_BOT
                : AppSettings.TWITCH_ACCESS_TOKEN;

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            req.Headers.Remove("Client-Id");
            req.Headers.Add("Client-Id", AppSettings.TWITCH_CLIENT_ID);

            var body = JsonSerializer.Serialize(new { data = new { user_id = targetUserId, duration = seconds } });
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var res = await http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Helix timeout failed {(int)res.StatusCode}: {text}");
            }
        }
    }
}