using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot.Core.Services
{
    public class AlertHistoryService : IAlertHistoryService
    {
        private readonly ILogger<AlertHistoryService> _logger;
        private readonly IAppFlags _flags;
        private readonly List<AlertHistoryEntry> _items = new();
        private readonly object _lock = new();
        private const int MaxItems = 500;

        public AlertHistoryService(ILogger<AlertHistoryService> logger, IAppFlags flags)
        {
            _logger = logger;
            _flags = flags;
        }

        public event Action<AlertHistoryEntry>? EntryAdded;

        public void Add(AlertHistoryEntry entry)
        {
            if(_flags.IsReplay)
            {
                return;
            }

            lock (_lock)
            {
                _items.Add(entry);
                if (_items.Count > MaxItems)
                    _items.RemoveAt(0);
            }
            try 
            { 
                EntryAdded?.Invoke(entry); 
            } 
            catch { /* ignore */ }
        }

        public IReadOnlyList<AlertHistoryEntry> Snapshot()
        {
            lock (_lock) { return _items.ToList().AsReadOnly(); }
        }

        public void Clear()
        {
            lock (_lock) { _items.Clear(); }
        }
    }
}