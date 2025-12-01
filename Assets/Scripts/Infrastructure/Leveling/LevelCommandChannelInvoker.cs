using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Events;

namespace KnifeToss.Infrastructure.Leveling
{
    public sealed class LevelCommandChannelInvoker : ILevelCommandInvoker
    {
        private readonly LevelCommandEventChannel channel;

        public LevelCommandChannelInvoker(LevelCommandEventChannel channel)
        {
            this.channel = channel;
        }

        public void Retry()
        {
            channel?.RaiseRetry();
        }

        public void Next()
        {
            channel?.RaiseNext();
        }
    }
}
