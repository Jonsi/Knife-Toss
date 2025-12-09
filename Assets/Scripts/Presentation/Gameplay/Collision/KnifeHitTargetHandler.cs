using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Data.Collision;
using KnifeToss.Core.Domain.Collision;
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
        private readonly ILevelSessionService _SessionService;

        public KnifeHitTargetHandler(ILevelSessionService sessionService)
        {
            _SessionService = sessionService;
        }

        protected override void HandleCollision(KnifeView knife, TargetView target, CollisionData data)
        {
            if (knife == null || target == null || !_SessionService.IsPlaying)
                return;

            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
            _SessionService.RegisterStick(data.ImpactPoint);
        }
    }
}

