using Microsoft.Extensions.Logging;
using TwitchChatBot.Data.Contracts;
using TwitchChatBot.Data.Utilities;
using TwitchChatBot.Models;

namespace TwitchChatBot.Data
{
    public class WatchStreakRepository : IWatchStreakRepository
    {
        private readonly ILogger<WatchStreakRepository> _logger;
        private readonly string _filePath;
        private WatchStreakState? _watchStreakState;

        private readonly object _sync = new();

        public WatchStreakRepository(ILogger<WatchStreakRepository> logger)
        {
            _logger = logger;
            _filePath = DataHelperMethods.GetUserWatchStreakMediaPath();
        }

        public async Task<WatchStreakState> GetStateAsync(CancellationToken cancellationToken = default)
        {
            await EnsureLoadedAsync(cancellationToken);

            // Always return a non-null usable shape
            return _watchStreakState!;
        }

        public async Task SaveAsync(WatchStreakState state, CancellationToken cancellationToken = default)
        {
            if (state == null)
            {
                return;
            }

            lock (_sync)
            {
                _watchStreakState = state;
                EnsureShape(_watchStreakState);
            }

            // If you already have a DataHelperMethods.SaveAsync<T>(), use that.
            // If not, I included a drop-in SaveAsync implementation below.
            await DataHelperMethods.SaveAsync(
                _filePath,
                _watchStreakState,
                _logger,
                AppSettings.MediaMapFiles.UserWatchStreakMedia,
                cancellationToken
            );
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (_watchStreakState != null)
                {
                    return;
                }
            }

            var loaded = await DataHelperMethods.LoadAsync<WatchStreakState>(
                _filePath,
                _logger,
                AppSettings.MediaMapFiles.UserWatchStreakMedia,
                cancellationToken
            );

            lock (_sync)
            {
                _watchStreakState = loaded ?? new WatchStreakState();
                EnsureShape(_watchStreakState);
            }
        }

        private static void EnsureShape(WatchStreakState state)
        {
            if (state.Users == null)
            {
                state.Users = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase);
            }
            MigrateAndNormalizeState(state);
        }

        private static void MigrateAndNormalizeState(WatchStreakState state)
        {
            if (state.Users.Count == 0)
            {
                return;
            }

            // Normalize fields and opportunistically re-key by UserId when present
            var rekeyed = new Dictionary<string, WatchStreakUserStats>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in state.Users)
            {
                var key = kvp.Key ?? string.Empty;
                var stats = kvp.Value ?? new WatchStreakUserStats();

                NormalizeUserStats(stats, key);

                var targetKey = !string.IsNullOrWhiteSpace(stats.UserId)
                    ? stats.UserId
                    : key;

                if (string.IsNullOrWhiteSpace(targetKey))
                {
                    // Worst case: no key and no userId; skip safely
                    continue;
                }

                if (rekeyed.TryGetValue(targetKey, out var existing))
                {
                    rekeyed[targetKey] = Merge(existing, stats);
                }
                else
                {
                    rekeyed[targetKey] = stats;
                }
            }

            state.Users = rekeyed;

            // Optional: bump version once you've shipped the code that understands the new format.
            // Keeping at 1 is fine until you're ready to "declare" the migration complete.
            // state.Version = Math.Max(state.Version, 2);
        }

        private static void NormalizeUserStats(WatchStreakUserStats stats, string fallbackUserName)
        {
            stats.UserId ??= string.Empty;
            stats.UserName ??= string.Empty;

            // If UserName is empty, fall back to whatever the dictionary key was.
            if (string.IsNullOrWhiteSpace(stats.UserName) && !string.IsNullOrWhiteSpace(fallbackUserName))
            {
                stats.UserName = fallbackUserName;
            }

            // FailedUserIdLookups default is 0, but this guards any legacy JSON weirdness.
            if (stats.FailedUserIdLookups < 0)
            {
                stats.FailedUserIdLookups = 0;
            }
        }

        private static WatchStreakUserStats Merge(WatchStreakUserStats a, WatchStreakUserStats b)
        {
            // Prefer whichever has a UserId (they should match if we're merging by userId-key)
            var merged = new WatchStreakUserStats
            {
                UserId = !string.IsNullOrWhiteSpace(a.UserId) ? a.UserId : b.UserId,
                UserName = PickBestUserName(a.UserName, b.UserName),

                // Keep the max fails so you don't lose the "gave up" history
                FailedUserIdLookups = Math.Max(a.FailedUserIdLookups, b.FailedUserIdLookups),

                // Streams/streaks: take the higher totals; you can change strategy if you prefer
                TotalStreams = Math.Max(a.TotalStreams, b.TotalStreams),
                Consecutive = Math.Max(a.Consecutive, b.Consecutive),

                // Attendance: take the most recent index
                LastAttendedIndex = Math.Max(a.LastAttendedIndex, b.LastAttendedIndex),

                // Dates: FirstSeen = earliest, LastSeen = latest
                FirstSeenUtc = Min(a.FirstSeenUtc, b.FirstSeenUtc),
                LastSeenUtc = Max(a.LastSeenUtc, b.LastSeenUtc)
            };

            return merged;
        }

        private static string PickBestUserName(string a, string b)
        {
            if (!string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b))
            {
                // Prefer the longer one if they differ (usually catches "actual" username vs empty/short junk)
                return a.Length >= b.Length ? a : b;
            }

            return !string.IsNullOrWhiteSpace(a) ? a : (b ?? string.Empty);
        }

        private static DateTimeOffset? Min(DateTimeOffset? a, DateTimeOffset? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return a.Value <= b.Value ? a : b;
        }

        private static DateTimeOffset? Max(DateTimeOffset? a, DateTimeOffset? b)
        {
            if (a == null) return b;
            if (b == null) return a;
            return a.Value >= b.Value ? a : b;
        }
    }
}