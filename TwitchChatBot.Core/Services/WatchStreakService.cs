using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class WatchStreakService : IWatchStreakService
    {
        private readonly ILogger<WatchStreakService> _logger;
        private readonly IExcludedUsersService _excludedUsersService;
        private readonly string _filePath;
        private readonly object _sync = new();
        private StreamSessionMode _mode = StreamSessionMode.Off;
        private bool _streamOpen;
        private State _state = new();
        private readonly HashSet<string> _attendeesThisStream = new(StringComparer.OrdinalIgnoreCase);
        
        public StreamSessionMode Mode { get { lock (_sync) return _mode; } }
        public bool IsLive { get { lock (_sync) return _mode == StreamSessionMode.Live; } }

        public WatchStreakService(ILogger<WatchStreakService> logger, IExcludedUsersService excludedUsersService)
        {
            _logger = logger;
            var baseFolder = AppSettings.Media.TwitchAlertsFolder
                ?? throw new InvalidOperationException("AppSettings.Media.TwitchAlertsFolder is not set.");
            _filePath = Path.Combine(baseFolder, "watch_streaks.json");

            Directory.CreateDirectory(baseFolder);
            Load();
            _excludedUsersService = excludedUsersService;
        }

        public void BeginStream(StreamSessionMode mode)
        {
            lock (_sync)
            {
                if (_streamOpen && _mode == mode) return;

                _mode = mode;
                _streamOpen = true;
                _attendeesThisStream.Clear();

                if (_mode == StreamSessionMode.Live)
                {
                    _state.CurrentStreamIndex += 1;
                    _logger.LogInformation("📡 Stream started (LIVE). Index now {Index}.", _state.CurrentStreamIndex);
                }
                else
                {
                    _logger.LogInformation("🧪 Stream started (TESTING). Stats will not be updated.");
                }

                Save();
            }
        }

        public void EndStream()
        {
            lock (_sync)
            {
                if (!_streamOpen) return;
                _streamOpen = false;
                _attendeesThisStream.Clear();
                _logger.LogInformation("🏁 Stream ended. Mode was {Mode}.", _mode);
                _mode = StreamSessionMode.Off;
                Save();
            }
        }

        public async Task MarkAttendanceAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName) || await _excludedUsersService.IsUserExcludedAsync(userName))
            {
                return;
            }

            lock (_sync)
            {
                if (!_streamOpen || _mode != StreamSessionMode.Live) return;
                if (!_attendeesThisStream.Add(userName)) return;

                if (!_state.Users.TryGetValue(userName, out var u))
                {
                    u = new UserStats { UserName = userName, FirstSeenUtc = DateTimeOffset.UtcNow };
                    _state.Users[userName] = u;
                }

                if (u.LastAttendedIndex == _state.CurrentStreamIndex - 1)
                    u.Consecutive += 1;
                else
                    u.Consecutive = 1;

                u.TotalStreams += 1;
                u.LastAttendedIndex = _state.CurrentStreamIndex;
                u.LastSeenUtc = DateTimeOffset.UtcNow;

                Save();
            }
        }

        public async Task MarkAttendanceBatchAsync(IEnumerable<string> userNames)
        {
            if (userNames == null) return;

            // 1) Do async filtering outside the lock
            var toMark = new List<string>();
            foreach (var user in userNames)
            {
                if (string.IsNullOrWhiteSpace(user) || await _excludedUsersService.IsUserExcludedAsync(user))
                {
                    continue;
                }

                toMark.Add(user);
            }

            // 2) Lock only while mutating shared state
            lock (_sync)
            {
                if (!_streamOpen || _mode != StreamSessionMode.Live) return;

                foreach (var user in toMark)
                {
                    if (string.IsNullOrWhiteSpace(user))
                    {
                        continue;
                    }

                    var name = user.Trim();
                    if (!_attendeesThisStream.Add(name)) continue;

                    if (!_state.Users.TryGetValue(name, out var u))
                    {
                        u = new UserStats { UserName = name, FirstSeenUtc = DateTimeOffset.UtcNow };
                        _state.Users[name] = u;
                    }

                    if (u.LastAttendedIndex == _state.CurrentStreamIndex - 1)
                        u.Consecutive += 1;
                    else
                        u.Consecutive = 1;

                    u.TotalStreams += 1;
                    u.LastAttendedIndex = _state.CurrentStreamIndex;
                    u.LastSeenUtc = DateTimeOffset.UtcNow;
                }

                Save();
            }
        }

        public IReadOnlyList<WatchUserStats> TopByConsecutive(int take = 10)
        {
            lock (_sync)
            {
                return _state.Users.Values
                    .OrderByDescending(u => u.Consecutive)
                    .ThenBy(u => u.UserName, StringComparer.OrdinalIgnoreCase)
                    .Take(take)
                    .Select(ToPublic)
                    .ToList();
            }
        }

        public IReadOnlyList<WatchUserStats> TopByTotal(int take = 10)
        {
            lock (_sync)
            {
                return _state.Users.Values
                    .OrderByDescending(u => u.TotalStreams)
                    .ThenBy(u => u.UserName, StringComparer.OrdinalIgnoreCase)
                    .Take(take)
                    .Select(ToPublic)
                    .ToList();
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _state = new State();
                    _logger.LogInformation("🆕 No streak file found. Starting fresh at {Path}", _filePath);
                    return;
                }

                var json = File.ReadAllText(_filePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _state = JsonSerializer.Deserialize<State>(json, opts) ?? new State();
                if (_state.Users == null) _state.Users = new(StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation("📖 Loaded watch streaks from {Path}. Users: {Count}, Index: {Index}",
                    _filePath, _state.Users.Count, _state.CurrentStreamIndex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to load watch streak file. Using empty state.");
                _state = new State();
            }
        }

        private void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                var opts = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(_state, opts);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to save watch streaks to {Path}", _filePath);
            }
        }

        private static WatchUserStats ToPublic(UserStats u) => new()
        {
            UserName = u.UserName,
            TotalStreams = u.TotalStreams,
            Consecutive = u.Consecutive,
            LastAttendedIndex = u.LastAttendedIndex,
            LastSeenUtc = u.LastSeenUtc,
            FirstSeenUtc = u.FirstSeenUtc
        };

        private class State
        {
            public int Version { get; set; } = 1;
            public int CurrentStreamIndex { get; set; }
            public Dictionary<string, UserStats> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private class UserStats
        {
            public string UserName { get; set; } = "";
            public int TotalStreams { get; set; }
            public int Consecutive { get; set; }
            public int LastAttendedIndex { get; set; }
            public DateTimeOffset? LastSeenUtc { get; set; }
            public DateTimeOffset? FirstSeenUtc { get; set; }
        }
    }
}