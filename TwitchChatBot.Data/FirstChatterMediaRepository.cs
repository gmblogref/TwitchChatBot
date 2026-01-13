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

        private readonly SemaphoreSlim _gate = new(1, 1);
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
            userId = userId?.Trim() ?? "";
            username = username?.Trim() ?? "";

            await EnsureLoadedAsync(cancellationToken).ConfigureAwait(false);

            FirstChatterMediaItem? item = null;
            var shouldSave = false;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_firstChattersMediaMap == null || _firstChattersMediaMap.FirstChatterMediaItems.Count == 0)
                {
                    return null;
                }

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
                    }
                }

                // 2) Fallback username match (legacy / pre-backfill)
                if (item == null && !string.IsNullOrWhiteSpace(username))
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
                    }
                }
            }
            finally
            {
                _gate.Release();
            }

            if (shouldSave)
            {
                await SaveAsync(cancellationToken).ConfigureAwait(false);
            }

            return item;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            FirstChatterMediaMap? snapshot;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                snapshot = _firstChattersMediaMap;
            }
            finally
            {
                _gate.Release();
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
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to save FirstChatterMediaMap.");
            }
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                _isLoaded = true;
            }
            finally
            {
                _gate.Release();
            }

            // Load outside the gate (no long awaits while holding the gate)
            FirstChatterMediaMap? map = null;

            map = await TryLoadNewFormatAsync(cancellationToken).ConfigureAwait(false);
            if (map == null)
            {
                map = await TryLoadAndMigrateLegacyAsync(cancellationToken).ConfigureAwait(false);

                if (map != null)
                {
                    // Save migrated result once
                    await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        _firstChattersMediaMap = map;
                    }
                    finally
                    {
                        _gate.Release();
                    }

                    await SaveAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _firstChattersMediaMap = map ?? new FirstChatterMediaMap();
            }
            finally
            {
                _gate.Release();
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