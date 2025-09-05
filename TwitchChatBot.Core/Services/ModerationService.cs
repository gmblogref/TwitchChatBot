using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class ModerationService : IModerationService
    {
        // PROPERTIES
        public string ClientId { get; }
        public string BotBearer { get; } // No "oauth:" prefix
        public HttpClient Http { get; }

        // PUBLIC
        public ModerationService(HttpClient http)
        {
            Http = http ?? throw new ArgumentNullException(nameof(http));
            ClientId = AppSettings.TWITCH_CLIENT_ID!;
            BotBearer = AppSettings.TWITCH_OAUTH_TOKEN!.Substring("oauth:".Length);
        }

        public async Task<string> GetUserIdAsync(string login, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Login is required.", nameof(login));
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users?login={login.Trim()}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BotBearer);
            req.Headers.Add("Client-ID", ClientId);

            using var res = await Http.SendAsync(req, ct);
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
        public async Task TimeoutAsync(string broadcasterId, string moderatorId, string targetUserId, int seconds, CancellationToken ct = default)
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

            var url = $"https://api.twitch.tv/helix/moderation/bans?broadcaster_id={broadcasterId}&moderator_id={moderatorId}";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BotBearer);
            req.Headers.Add("Client-ID", ClientId);

            var payload = new
            {
                data = new
                {
                    user_id = targetUserId,
                    duration = seconds
                }
            };
            var body = JsonSerializer.Serialize(payload);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var res = await Http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Helix timeout failed {(int)res.StatusCode}: {text}");
            }
        }
    }
}
