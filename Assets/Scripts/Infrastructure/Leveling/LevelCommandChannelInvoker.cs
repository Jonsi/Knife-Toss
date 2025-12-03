using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Events;

namespace KnifeToss.Infrastructure.Leveling
{
    public sealed class LevelCommandChannelInvoker : ILevelCommandInvoker
    {
        private readonly LevelCommandEventChannel channel;

        /// <summary>
        /// Performs the LevelCommandChannelInvoker operation.
        /// </summary>
        public LevelCommandChannelInvoker(LevelCommandEventChannel channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Performs the Retry operation.
        /// </summary>
        public void Retry()
        {
            channel?.RaiseRetry();
        }

        /// <summary>
        /// Performs the Next operation.
        /// </summary>
        public void Next()
        {
            channel?.RaiseNext();
        }
    }
}
