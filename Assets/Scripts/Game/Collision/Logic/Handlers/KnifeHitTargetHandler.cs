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
        private readonly StageData _stageData;
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;

        public KnifeHitTargetHandler(StageData stageData, IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _stageData = stageData;
            _impactChannel = impactChannel;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (!_stageData.IsThrowingActive)
                return;

            if (data.Source is not KnifeView knife || data.Target is not TargetView target)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.ImpactPoint));
        }
    }
}
