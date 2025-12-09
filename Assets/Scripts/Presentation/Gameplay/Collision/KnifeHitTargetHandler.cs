using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Domain.Collision;
using Presentation.Gameplay.Knife;
using Presentation.Gameplay.Target;

namespace Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandler<KnifeView, TargetView>
    {
        private readonly ILevelSessionService _sessionService;

        public KnifeHitTargetHandler(ILevelSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override void HandleCollision(KnifeView knife, TargetView target, CollisionData data)
        {
            if (knife == null || target == null || !_sessionService.IsPlaying)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            _sessionService.RegisterStick(data.ImpactPoint);
        }
    }
}

