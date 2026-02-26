using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class HelixLookupService : IHelixLookupService
    {
        private const int MaxLoginToIdCacheEntries = 4096;
        private const int MaxIdToLastGameCacheEntries = 4096;

        private readonly HttpClient _httpClient;
        private readonly ILogger<HelixLookupService> _logger;

        private readonly object _cacheSync = new();

        private readonly Dictionary<string, string> _loginToId = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _loginToIdInsertionOrder = new();

        private readonly Dictionary<string, string> _idToLastGame = new();
        private readonly Queue<string> _idToLastGameInsertionOrder = new();

        public HelixLookupService(ILogger<HelixLookupService> logger, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _httpClient = httpFactory.CreateClient("twitch-helix");
        }

        public async Task<string?> GetUserIdByLoginAsync(string login, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            login = login.Trim();

            lock (_cacheSync)
            {
                if (_loginToId.TryGetValue(login, out var cached))
                {
                    return cached;
                }
            }

            var url = AppSettings.Commands.UsersByLogin.Replace("{login}", Uri.EscapeDataString(login));
            using var resp = await _httpClient.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUsers {Login}: {Status}", login, resp.StatusCode);
                return null;
            }

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0)
            {
                return null;
            }

            var id = data[0].GetProperty("id").GetString();
            if (!string.IsNullOrEmpty(id))
            {
                lock (_cacheSync)
                {
                    AddToBoundedCache(_loginToId, _loginToIdInsertionOrder, MaxLoginToIdCacheEntries, login, id!);
                }
            }

            return id;
        }

        public async Task<string?> GetLastKnownGameByUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            userId = userId.Trim();

            lock (_cacheSync)
            {
                if (_idToLastGame.TryGetValue(userId, out var cached) && !string.IsNullOrEmpty(cached))
                {
                    return cached;
                }
            }

            // 1) Streams (live)
            {
                var url = AppSettings.Commands.StreamsByUserId.Replace("{id}", Uri.EscapeDataString(userId));
                using var resp = await _httpClient.GetAsync(url, ct);
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                    var data = doc.RootElement.GetProperty("data");
                    if (data.GetArrayLength() > 0 &&
                        data[0].TryGetProperty("game_name", out var g) &&
                        g.ValueKind == JsonValueKind.String)
                    {
                        var gameName = g.GetString()!;
                        lock (_cacheSync)
                        {
                            AddToBoundedCache(_idToLastGame, _idToLastGameInsertionOrder, MaxIdToLastGameCacheEntries, userId, gameName);
                        }
                        return gameName;
                    }
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
                    if (data.GetArrayLength() > 0 &&
                        data[0].TryGetProperty("game_name", out var g) &&
                        g.ValueKind == JsonValueKind.String)
                    {
                        var gameName = g.GetString()!;
                        lock (_cacheSync)
                        {
                            AddToBoundedCache(_idToLastGame, _idToLastGameInsertionOrder, MaxIdToLastGameCacheEntries, userId, gameName);
                        }
                        return gameName;
                    }
                }
            }

            // 3) Last VOD
            {
                var url = AppSettings.Commands.VideosByUserId.Replace("{id}", Uri.EscapeDataString(userId));
                using var resp = await _httpClient.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    return null;
                }

                using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                var data = doc.RootElement.GetProperty("data");
                if (data.GetArrayLength() == 0)
                {
                    return null;
                }

                var item = data[0];
                if (item.TryGetProperty("game_name", out var gn) && gn.ValueKind == JsonValueKind.String)
                {
                    var gameName = gn.GetString()!;
                    lock (_cacheSync)
                    {
                        AddToBoundedCache(_idToLastGame, _idToLastGameInsertionOrder, MaxIdToLastGameCacheEntries, userId, gameName);
                    }
                    return gameName;
                }

                if (item.TryGetProperty("game_id", out var gid) && gid.ValueKind == JsonValueKind.String)
                {
                    var name = await ResolveGameNameAsync(gid.GetString()!, ct);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        lock (_cacheSync)
                        {
                            AddToBoundedCache(_idToLastGame, _idToLastGameInsertionOrder, MaxIdToLastGameCacheEntries, userId, name!);
                        }
                        return name;
                    }
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
                        {
                            result.Add(login.GetString()!);
                        }
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
                        {
                            result.Add(login.GetString()!);
                        }
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
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var data = doc.RootElement.GetProperty("data");
            return data.GetArrayLength() == 0 ? null : data[0].TryGetProperty("name", out var n) ? n.GetString() : null;
        }

        private static void AddToBoundedCache(
            Dictionary<string, string> cache,
            Queue<string> insertionOrder,
            int maxEntries,
            string key,
            string value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key] = value;
                return;
            }

            cache[key] = value;
            insertionOrder.Enqueue(key);

            while (cache.Count > maxEntries && insertionOrder.Count > 0)
            {
                var oldestKey = insertionOrder.Dequeue();
                cache.Remove(oldestKey);
            }
        }
    }
}