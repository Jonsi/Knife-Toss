using System;
using KnifeToss.Core.Events;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{




    public sealed class RotatingTargetFailHandler : IStartable, IDisposable
    {
        private readonly GameplayObjectProvider gameplayObjectProvider;
        private readonly KnifeImpactEventChannel impactEventChannel;

        /// <summary>
        /// Performs the RotatingTargetFailHandler operation.
        /// </summary>
        public RotatingTargetFailHandler(
            GameplayObjectProvider gameplayObjectProvider,
            KnifeImpactEventChannel impactEventChannel)
        {
            this.gameplayObjectProvider = gameplayObjectProvider;
            this.impactEventChannel = impactEventChannel;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeFailed += HandleKnifeFailed;
            }
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeFailed -= HandleKnifeFailed;
            }
        }

        /// <summary>
        /// Performs the HandleKnifeFailed operation.
        /// </summary>
        private void HandleKnifeFailed()
        {
            TargetGroupManager targetGroupManager = gameplayObjectProvider?.GetTargetGroupManager();
            targetGroupManager?.FreezeAll();
        }
    }
}
