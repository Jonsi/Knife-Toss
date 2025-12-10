using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Domain.Collision;
using KnifeToss.Core.Domain.Level;
using KnifeToss.Presentation.Gameplay.Knife;
using KnifeToss.Presentation.Gameplay.Target;

namespace KnifeToss.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandler<KnifeView, TargetView>
    {
        private readonly StageData _stageData;
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;

        public KnifeHitTargetHandler(StageData stageData, IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _stageData = stageData;
            _impactChannel = impactChannel;
        }

        protected override void HandleCollision(KnifeView knife, TargetView target, CollisionData data)
        {
            if (knife == null || target == null || !_stageData.IsThrowingActive)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.ImpactPoint));
        }
    }
}
