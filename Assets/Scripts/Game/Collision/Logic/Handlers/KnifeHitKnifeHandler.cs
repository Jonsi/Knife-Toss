using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Knife.Data;
using KnifeToss.Game.Knife.Presentation;
using KnifeToss.Game.Level.Data;

namespace KnifeToss.Game.Collision.Logic.Handlers
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

        protected override void HandleCollision(CollisionData data)
        {
            if (!_stageData.IsThrowingActive)
                return;

            if (data.Source is not KnifeView knife || data.Target is not KnifeView otherKnife)
                return;

            if (!knife.IsInFlight)
                return;

            knife.Bounce();
            
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced, data.ImpactPoint));
        }
    }
}
