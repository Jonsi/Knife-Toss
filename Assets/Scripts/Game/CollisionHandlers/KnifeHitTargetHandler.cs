using Common.Collision.Data;
using Common.Collision.Logic;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Views;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandlerBase<KnifeView, TargetView>
    {
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;

        public KnifeHitTargetHandler(IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _ImpactChannel = impactChannel;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (data.Source is not KnifeView knife || data.Target is not TargetView target)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            target.PlayHitFeedback();
            
            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, data.ImpactPoint));
        }
    }
}
