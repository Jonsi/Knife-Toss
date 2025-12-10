using Common.Collision.Data;
using Common.Collision.Logic;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Views;
using Game.Level.Data;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandlerBase<KnifeView, TargetView>
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

            if (data.Source is not KnifeView knife || data.Target is not TargetView target)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            
            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck));
        }
    }
}
