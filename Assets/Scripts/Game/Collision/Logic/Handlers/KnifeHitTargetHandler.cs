using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Knife.Data;
using KnifeToss.Game.Knife.Presentation;
using KnifeToss.Game.Level.Data;
using KnifeToss.Game.Target.Presentation;

namespace KnifeToss.Game.Collision.Logic.Handlers
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandler<KnifeView, TargetView>
    {
        private readonly StageData _StageData;
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;

        public KnifeHitTargetHandler(StageData stageData, IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _StageData = stageData;
            _ImpactChannel = impactChannel;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (!_StageData.IsThrowingActive)
                return;

            if (data.source is not KnifeView knife || data.target is not TargetView target)
                return;

            knife.StopMovement();
            target.Stick(knife, data.impactPoint);
            
            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.impactPoint));
        }
    }
}
