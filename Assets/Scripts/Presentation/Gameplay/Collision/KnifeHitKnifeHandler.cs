using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Domain.Collision;
using Presentation.Gameplay.Knife;

namespace Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives (in-flight knife hitting another knife results in bounce/fail).
    /// </summary>
    public class KnifeHitKnifeHandler : CollisionHandler<KnifeView, KnifeView>
    {
        private readonly ILevelSessionService _sessionService;

        public KnifeHitKnifeHandler(ILevelSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override void HandleCollision(KnifeView knife, KnifeView otherKnife, CollisionData data)
        {
            if (knife == null || otherKnife == null || !_sessionService.IsPlaying)
                return;

            if (!knife.IsInFlight)
                return;

            knife.Bounce();
            _sessionService.RegisterBounce(data.ImpactPoint);
        }
    }
}

