using Microsoft.Extensions.Logging;
using System.Text.Json;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class FirstChatterMediaRepository : IFirstChatterMediaRepository
    {
        private readonly ILogger<FirstChatterMediaRepository> _logger;
        private readonly string _filePath;

        private readonly object _sync = new();
        private bool _isLoaded;
        private FirstChatterMediaMap? _firstChattersMediaMap;

        public FirstChatterMediaRepository(ILogger<FirstChatterMediaRepository> logger)
        {
            _logger = logger;
            _filePath = DataHelperMethods.GetFirstChattersMediaPath();
        }

        public async Task<FirstChatterMediaItem?> GetFirstChatterMediaAsync(
            string userId,
            string username,
            CancellationToken cancellationToken = default)
        {
            await EnsureLoadedAsync(cancellationToken);

            if (_firstChattersMediaMap == null || _firstChattersMediaMap.FirstChatterMediaItems.Count == 0)
            {
                return null;
            }

            FirstChatterMediaItem? item = null;
            var shouldSave = false;

            lock (_sync)
            {
                // 1) Prefer userId match (stable)
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    item = _firstChattersMediaMap.FirstChatterMediaItems
                        .FirstOrDefault(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

                    if (item != null)
                    {
                        // Update username if it changed (rename)
                        if (!string.IsNullOrWhiteSpace(username) &&
                            !string.Equals(item.CurrentUserName, username, StringComparison.OrdinalIgnoreCase))
                        {
                            item.CurrentUserName = username;
                            shouldSave = true;
                        }

                        return item;
                    }
                }

                // 2) Fallback username match (legacy / pre-backfill)
                if (!string.IsNullOrWhiteSpace(username))
                {
                    item = _firstChattersMediaMap.FirstChatterMediaItems
                        .FirstOrDefault(x => string.Equals(x.CurrentUserName, username, StringComparison.OrdinalIgnoreCase));

                    if (item != null)
                    {
                        // Backfill userId if we have it now
                        if (!string.IsNullOrWhiteSpace(userId) &&
                            string.IsNullOrWhiteSpace(item.UserId))
                        {
                            item.UserId = userId;
                            shouldSave = true;
                        }

                        return item;
                    }
                }
            }

            if (shouldSave)
            {
                await SaveAsync(cancellationToken);
            }

            return item;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            FirstChatterMediaMap? snapshot;

            lock (_sync)
            {
                snapshot = _firstChattersMediaMap;
            }

            if (snapshot == null)
            {
                return;
            }

            try
            {
                await DataHelperMethods.SaveAsync(
                    _filePath,
                    snapshot,
                    _logger,
                    AppSettings.MediaMapFiles.FirstChattersMedia!,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to save FirstChatterMediaMap.");
            }
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (_isLoaded)
                {
                    return;
                }

                _isLoaded = true;
            }

            // Try load new format
            var map = await TryLoadNewFormatAsync(cancellationToken);
            if (map != null)
            {
                lock (_sync)
                {
                    _firstChattersMediaMap = map;
                }

                return;
            }

            // If new format isn't there yet, try legacy dictionary and migrate
            var migrated = await TryLoadAndMigrateLegacyAsync(cancellationToken);
            if (migrated != null)
            {
                lock (_sync)
                {
                    _firstChattersMediaMap = migrated;
                }

                await SaveAsync(cancellationToken);
                return;
            }

            // Default empty map
            lock (_sync)
            {
                _firstChattersMediaMap = new FirstChatterMediaMap();
            }
        }

        private async Task<FirstChatterMediaMap?> TryLoadNewFormatAsync(CancellationToken cancellationToken)
        {
            try
            {
                var map = await DataHelperMethods.LoadAsync<FirstChatterMediaMap>(
                    _filePath,
                    _logger,
                    AppSettings.MediaMapFiles.FirstChattersMedia!,
                    cancellationToken);

                if (map != null && map.FirstChatterMediaItems != null)
                {
                    // Normalize null lists
                    foreach (var item in map.FirstChatterMediaItems)
                    {
                        if (item.AllowedDaysOfWeek == null)
                        {
                            item.AllowedDaysOfWeek = new List<DayOfWeek>();
                        }
                    }

                    return map;
                }
            }
            catch
            {
                // Intentionally swallow here; we'll try legacy.
            }

            return null;
        }

        private async Task<FirstChatterMediaMap?> TryLoadAndMigrateLegacyAsync(CancellationToken cancellationToken)
        {
            try
            {
                // If your LoadAsync can load raw JSON, use it. If not, this is the simplest:
                var json = await File.ReadAllTextAsync(_filePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                // Legacy is: { "username": "path.mp4", ... }
                var legacy = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (legacy == null || legacy.Count == 0)
                {
                    return null;
                }

                var migrated = new FirstChatterMediaMap();

                foreach (var kvp in legacy)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        continue;
                    }

                    migrated.FirstChatterMediaItems.Add(new FirstChatterMediaItem
                    {
                        UserId = "",
                        CurrentUserName = kvp.Key,
                        Media = kvp.Value,
                        AllowedDaysOfWeek = new List<DayOfWeek>()
                    });
                }

                _logger.LogInformation("🔁 Migrated legacy first-chatter media map to new format. Count: {Count}", migrated.FirstChatterMediaItems.Count);

                return migrated;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to load/migrate legacy first-chatter media map.");
                return null;
            }
        }
    }
}