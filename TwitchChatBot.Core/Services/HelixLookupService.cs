using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class HelixLookupService : IHelixLookupService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<HelixLookupService> _logger;
        private readonly Dictionary<string, string> _loginToId = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _idToLastGame = new();

        public HelixLookupService(ILogger<HelixLookupService> logger, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _httpClient = httpFactory.CreateClient("twitch-helix");
        }

        public async Task<string?> GetUserIdByLoginAsync(string login, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(login)) return null;
            if (_loginToId.TryGetValue(login, out var cached)) return cached;

            var url = AppSettings.Commands.UsersByLogin.Replace("{login}", Uri.EscapeDataString(login));
            using var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) { _logger.LogWarning("GetUsers {Login}: {Status}", login, resp.StatusCode); return null; }

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return null;

            var id = data[0].GetProperty("id").GetString();
            if (!string.IsNullOrEmpty(id)) _loginToId[login] = id!;
            return id;
        }

        public async Task<string?> GetLastKnownGameByUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            if (_idToLastGame.TryGetValue(userId, out var cached) && !string.IsNullOrEmpty(cached)) return cached;

            // 1) Streams (live)
            {
                var url = AppSettings.Commands.StreamsByUserId.Replace("{id}", Uri.EscapeDataString(userId));
                using var resp = await _httpClient.GetAsync(url, ct);
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                    var data = doc.RootElement.GetProperty("data");
                    if (data.GetArrayLength() > 0 && data[0].TryGetProperty("game_name", out var g) && g.ValueKind == JsonValueKind.String)
                        return _idToLastGame[userId] = g.GetString()!;
                }
            }

            // 2) Channel info (offline category)
            {
                var url = AppSettings.Commands.ChannelInfoByBroadcasterId.Replace("{id}", Uri.EscapeDataString(userId));
                using var resp = await _httpClient.GetAsync(url, ct);
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                    var data = doc.RootElement.GetProperty("data");
                    if (data.GetArrayLength() > 0 && data[0].TryGetProperty("game_name", out var g) && g.ValueKind == JsonValueKind.String)
                        return _idToLastGame[userId] = g.GetString()!;
                }
            }

            // 3) Last VOD
            {
                var url = AppSettings.Commands.VideosByUserId.Replace("{id}", Uri.EscapeDataString(userId));
                using var resp = await _httpClient.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return null;

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                var data = doc.RootElement.GetProperty("data");
                if (data.GetArrayLength() == 0) return null;

                var item = data[0];
                if (item.TryGetProperty("game_name", out var gn) && gn.ValueKind == JsonValueKind.String)
                    return _idToLastGame[userId] = gn.GetString()!;

                if (item.TryGetProperty("game_id", out var gid) && gid.ValueKind == JsonValueKind.String)
                {
                    var name = await ResolveGameNameAsync(gid.GetString()!, ct);
                    if (!string.IsNullOrWhiteSpace(name)) return _idToLastGame[userId] = name!;
                }
            }

            return null;
        }

        public async Task<IReadOnlyList<string>> GetModeratorLoginsAsync(string broadcasterId, CancellationToken ct = default)
        {
            var result = new List<string>();
            string? cursor = null;

            do
            {
                var url = AppSettings.Chatters.ModsUrl.Replace("{broadcasterId}", Uri.EscapeDataString(broadcasterId))
                        + (cursor is null ? "" : $"&after={Uri.EscapeDataString(cursor)}");

                using var resp = await _httpClient.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetModerators failed for {BroadcasterId}: {Status}", broadcasterId, resp.StatusCode);
                    break;
                }

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var data))
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        if (item.TryGetProperty("user_login", out var login) && login.ValueKind == JsonValueKind.String)
                            result.Add(login.GetString()!);
                    }
                }

                cursor = root.TryGetProperty("pagination", out var pag) &&
                         pag.TryGetProperty("cursor", out var c) &&
                         c.ValueKind == JsonValueKind.String
                         ? c.GetString()
                         : null;

            } while (cursor is not null);

            return result;
        }

        public async Task<IReadOnlyList<string>> GetVipLoginsAsync(string broadcasterId, CancellationToken ct = default)
        {
            var result = new List<string>();
            string? cursor = null;

            do
            {
                var url = AppSettings.Chatters.VipUrl.Replace("{broadcasterId}", Uri.EscapeDataString(broadcasterId))
                        + (cursor is null ? "" : $"&after={Uri.EscapeDataString(cursor)}");

                using var resp = await _httpClient.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetVips failed for {BroadcasterId}: {Status}", broadcasterId, resp.StatusCode);
                    break;
                }

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out var data))
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        if (item.TryGetProperty("user_login", out var login) && login.ValueKind == JsonValueKind.String)
                            result.Add(login.GetString()!);
                    }
                }

                cursor = root.TryGetProperty("pagination", out var pag) &&
                         pag.TryGetProperty("cursor", out var c) &&
                         c.ValueKind == JsonValueKind.String
                         ? c.GetString()
                         : null;

            } while (cursor is not null);

            return result;
        }

        private async Task<string?> ResolveGameNameAsync(string gameId, CancellationToken ct)
        {
            var url = AppSettings.Commands.GamesByGameId.Replace("{gameid}", Uri.EscapeDataString(gameId));
            using var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var data = doc.RootElement.GetProperty("data");
            return data.GetArrayLength() == 0 ? null : data[0].TryGetProperty("name", out var n) ? n.GetString() : null;
        }
    }
}