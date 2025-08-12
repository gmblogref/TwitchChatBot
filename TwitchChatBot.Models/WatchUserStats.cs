using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatBot.Models
{
    public class WatchUserStats
    {
        public string UserName { get; set; } = "";
        public int TotalStreams { get; set; }
        public int Consecutive { get; set; }
        public int LastAttendedIndex { get; set; }
        public DateTimeOffset? LastSeenUtc { get; set; }
        public DateTimeOffset? FirstSeenUtc { get; set; }
    }
}