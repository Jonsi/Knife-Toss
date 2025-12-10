using Common.Events;
using Game.Collision.Data;
using Game.Knife.Data;
using Game.Knife.Views;
using Game.Level.Data;

namespace Game.Collision.Logic.Handlers
{
    /// <summary>
    /// Handles collisions between knives (in-flight knife hitting another knife results in bounce/fail).
    /// </summary>
    public class KnifeHitKnifeHandler : CollisionHandler<KnifeView, KnifeView>
    {
        private readonly StageData _StageData;
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;

        public KnifeHitKnifeHandler(StageData stageData, IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _StageData = stageData;
            _ImpactChannel = impactChannel;
        }

        protected override void HandleCollision(CollisionData data)
        {
            if (!_StageData.IsThrowingActive)
                return;

            if (data.Source is not KnifeView knife || data.Target is not KnifeView)
                return;

            if (!knife.IsInFlight)
                return;

            knife.Bounce();
            
            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced));
        }
    }
}
