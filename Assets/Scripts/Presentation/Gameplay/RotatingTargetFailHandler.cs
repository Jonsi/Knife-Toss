using System;
using KnifeToss.Core.Events;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{




    public sealed class RotatingTargetFailHandler : IStartable, IDisposable
    {
        private readonly GameplayObjectProvider gameplayObjectProvider;
        private readonly KnifeImpactEventChannel impactEventChannel;

        public RotatingTargetFailHandler(
            GameplayObjectProvider gameplayObjectProvider,
            KnifeImpactEventChannel impactEventChannel)
        {
            this.gameplayObjectProvider = gameplayObjectProvider;
            this.impactEventChannel = impactEventChannel;
        }

        public void Start()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeFailed += HandleKnifeFailed;
            }
        }

        public void Dispose()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeFailed -= HandleKnifeFailed;
            }
        }

        private void HandleKnifeFailed()
        {
            TargetGroupManager targetGroupManager = gameplayObjectProvider?.GetTargetGroupManager();
            targetGroupManager?.FreezeAll();
        }
    }
}
