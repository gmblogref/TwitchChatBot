using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatBot.Core.Services.Contracts
{
    public interface INukeService
    {
        bool TryUseNuke(string username);
        void ClearNukes();
        void StartNukeResetTimer();
        void StopNukeResetTimer();
    }
}