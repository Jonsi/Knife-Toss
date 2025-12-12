using System;
using Game.Pooling;
using VContainer.Unity;

namespace Game.Pooling
{
    /// <summary>
    /// Service that manages knife lifecycle and returns them to the pool when appropriate.
    /// </summary>
    public class KnifePoolService : IStartable, IDisposable
    {
        private readonly KnifePoolProvider _KnifePoolProvider;
        private bool _IsStarted;

        /// <summary>
        /// Initializes a new instance of the KnifePoolService.
        /// </summary>
        public KnifePoolService(
            KnifePoolProvider knifePoolProvider)
        {
            _KnifePoolProvider = knifePoolProvider;
        }

        /// <summary>
        /// Called automatically by VContainer when the LifetimeScope starts.
        /// </summary>
        public void Start()
        {
            if (_IsStarted)
                return;

            _IsStarted = true;
        }

        /// <summary>
        /// Called automatically by VContainer when the LifetimeScope ends.
        /// </summary>
        public void Dispose()
        {
            if (!_IsStarted)
                return;

            _IsStarted = false;
        }
    }
}