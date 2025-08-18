using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot.Core.Services
{
    public sealed class AppFlags : IAppFlags
    {
        // Backing fields (use int for atomic ops)
        private int _isTesting;
        private int _isReplayDepth; // support nested replay calls safely

        public bool IsTesting
        {
            get => Interlocked.CompareExchange(ref _isTesting, 0, 0) != 0;
            set => Interlocked.Exchange(ref _isTesting, value ? 1 : 0);
        }

        public bool IsReplay => Interlocked.CompareExchange(ref _isReplayDepth, 0, 0) > 0;

        public IDisposable BeginReplayScope()
        {
            Interlocked.Increment(ref _isReplayDepth);
            return new Scope(this);
        }

        private sealed class Scope : IDisposable
        {
            private AppFlags _owner;
            private int _disposed;
            public Scope(AppFlags owner) => _owner = owner;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
                Interlocked.Decrement(ref _owner._isReplayDepth);
            }
        }
    }
}