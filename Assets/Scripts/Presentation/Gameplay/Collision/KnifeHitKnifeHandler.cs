using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Domain.Collision;
using KnifeToss.Core.Domain.Level;
using KnifeToss.Presentation.Gameplay.Knife;

namespace KnifeToss.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives (in-flight knife hitting another knife results in bounce/fail).
    /// </summary>
    public class KnifeHitKnifeHandler : CollisionHandler<KnifeView, KnifeView>
    {
        private readonly StageData _stageData;
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;

        public KnifeHitKnifeHandler(StageData stageData, IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _stageData = stageData;
            _impactChannel = impactChannel;
        }

        protected override void HandleCollision(KnifeView knife, KnifeView otherKnife, CollisionData data)
        {
            if (knife == null || otherKnife == null || !_stageData.IsThrowingActive)
                return;

            if (!knife.IsInFlight)
                return;

            knife.Bounce();
            
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced, data.ImpactPoint));
        }
    }
}
