using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class WatchStreakService : IWatchStreakService
    {
        private readonly ILogger<WatchStreakService> _logger;
        private readonly IExcludedUsersService _excludedUsersService;
        private readonly IAppFlags _appFlags;
        private readonly IWatchStreakRepository _watchStreakRepository;

        private readonly object _sync = new();
        private bool _streamOpen;
        private WatchStreakState? _watchStreakState;
        private readonly HashSet<string> _attendeesThisStream = new(StringComparer.OrdinalIgnoreCase); // tracked by UserId since it doesn't change


        public WatchStreakService(
            ILogger<WatchStreakService> logger,
            IExcludedUsersService excludedUsersService,
            IAppFlags appFlags,
            IWatchStreakRepository watchStreakRepository)
        {
            _logger = logger;
            _excludedUsersService = excludedUsersService;
            _appFlags = appFlags;
            _watchStreakRepository = watchStreakRepository;
        }

        public void BeginStream()
        {
            WatchStreakState? snapshotToSave = null;

            lock (_sync)
            {
                EnsureStateLoaded_NoAwait();

                if (_streamOpen && _appFlags.IsTesting)
                {
                    return;
                }

                _streamOpen = true;
                _attendeesThisStream.Clear();

                if (_appFlags.IsTesting)
                {
                    _logger.LogInformation("🧪 Stream started (TESTING). Stats will not be updated.");
                    return;
                }

                _watchStreakState!.CurrentStreamIndex += 1;
                _logger.LogInformation("📡 Stream started (LIVE). Index now {Index}.", _watchStreakState.CurrentStreamIndex);

                snapshotToSave = _watchStreakState;
            }

            _ = SaveStateAsync(snapshotToSave);
        }

        public void EndStream()
        {
            WatchStreakState? snapshotToSave = null;

            lock (_sync)
            {
                EnsureStateLoaded_NoAwait();

                if (!_streamOpen || _appFlags.IsTesting)
                {
                    return;
                }

                _streamOpen = false;
                _attendeesThisStream.Clear();

                snapshotToSave = _watchStreakState;
            }

            _ = SaveStateAsync(snapshotToSave);
        }

        public async Task MarkAttendanceAsync(string userId, string userName)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(userName) ||
                !_streamOpen ||
                _appFlags.IsTesting)
            {
                return;
            }

            userName = userName.Trim();

            if (await _excludedUsersService.IsUserExcludedAsync(userId, userName))
            {
                _logger.LogInformation("🙈 Ignoring attendance for excluded user: {Username}", userName);
                return;
            }

            WatchStreakState? snapshotToSave = null;

            lock (_sync)
            {
                EnsureStateLoaded_NoAwait();

                // Only count the user once per stream
                if (!_attendeesThisStream.Add(userId))
                {
                    return;
                }

                // Get-or-create stats keyed by userId
                // With legacy username-key migration
                var u = GetOrCreateUserStats_LegacyMigration(userId, userName);

                if (u.LastAttendedIndex == _watchStreakState!.CurrentStreamIndex - 1)
                {
                    u.Consecutive += 1;
                }
                else
                {
                    u.Consecutive = 1;
                }

                u.TotalStreams += 1;
                u.LastAttendedIndex = _watchStreakState.CurrentStreamIndex;
                u.LastSeenUtc = DateTimeOffset.UtcNow;

                snapshotToSave = _watchStreakState;
            }

            await SaveStateAsync(snapshotToSave);
        }

        public Task<(int Consecutive, int Total)> GetStatsTupleAsync(string userId, string userName)
        {
            if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(userName))
            {
                return Task.FromResult((0, 0));
            }

            lock (_sync)
            {
                EnsureStateLoaded_NoAwait();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    if (_watchStreakState!.Users.TryGetValue(userId, out var sById))
                    {
                        return Task.FromResult((sById.Consecutive, sById.TotalStreams));
                    }
                }

                // Fallback: legacy username lookup (in case something still calls by username)
                var legacy = FindLegacyByUserName_NoAlloc(userName);
                if (legacy != null)
                {
                    return Task.FromResult((legacy.Consecutive, legacy.TotalStreams));
                }
            }

            return Task.FromResult((0, 0));
        }

        private void EnsureStateLoaded_NoAwait()
        {
            if (_watchStreakState != null)
            {
                return;
            }

            // Repo handles "file missing -> new state" via LoadOrCreateAsync
            // We block here to keep the service API unchanged (BeginStream is sync).
            _watchStreakState = _watchStreakRepository.GetStateAsync().GetAwaiter().GetResult();

            if (_watchStreakState.Users == null)
            {
                _watchStreakState.Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private WatchStreakUserStats GetOrCreateUserStats_LegacyMigration(string userId, string userName)
        {
            // Preferred: already keyed correctly by userId
            if (_watchStreakState!.Users.TryGetValue(userId, out var existingById))
            {
                // Keep latest known username for display
                if (!string.Equals(existingById.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    existingById.UserName = userName;
                }

                if (string.IsNullOrWhiteSpace(existingById.UserId))
                {
                    existingById.UserId = userId;
                }

                return existingById;
            }

            // Legacy migration path:
            // Old files likely have dictionary key == username, stats.UserId empty.
            // We search for a record matching username and move it under userId.
            var legacyKey = FindLegacyKeyByUserName(userName);
            if (!string.IsNullOrWhiteSpace(legacyKey) && _watchStreakState.Users.TryGetValue(legacyKey, out var legacy))
            {
                _watchStreakState.Users.Remove(legacyKey);

                legacy.UserId = userId;
                legacy.UserName = userName;

                if (legacy.FirstSeenUtc == null)
                {
                    legacy.FirstSeenUtc = DateTimeOffset.UtcNow;
                }

                _watchStreakState.Users[userId] = legacy;

                _logger.LogInformation("🔁 Migrated watch streak stats from legacy key '{LegacyKey}' to UserId '{UserId}'.", legacyKey, userId);

                return legacy;
            }

            // Brand new user
            var u = new WatchStreakUserStats
            {
                UserId = userId,
                UserName = userName,
                FirstSeenUtc = DateTimeOffset.UtcNow
            };

            _watchStreakState.Users[userId] = u;
            return u;
        }

        private string? FindLegacyKeyByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            foreach (var kvp in _watchStreakState!.Users)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                // Legacy: key was username and/or UserId was not set yet
                if (string.Equals(key, userName, StringComparison.OrdinalIgnoreCase))
                {
                    return key;
                }

                if (!string.IsNullOrWhiteSpace(value?.UserName) &&
                    string.Equals(value.UserName, userName, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrWhiteSpace(value.UserId) || !string.Equals(key, value.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    return key;
                }
            }

            return null;
        }

        private WatchStreakUserStats? FindLegacyByUserName_NoAlloc(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            foreach (var kvp in _watchStreakState!.Users)
            {
                if (kvp.Value != null &&
                    !string.IsNullOrWhiteSpace(kvp.Value.UserName) &&
                    string.Equals(kvp.Value.UserName, userName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private async Task SaveStateAsync(WatchStreakState? state, CancellationToken ct = default)
        {
            if (state == null)
            {
                return;
            }

            try
            {
                await _watchStreakRepository.SaveAsync(state, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to save watch streak state.");
            }
        }
    }
}