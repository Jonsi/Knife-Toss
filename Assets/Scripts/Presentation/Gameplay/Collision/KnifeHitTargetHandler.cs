using Core.Abstractions;
using Core.Data;
using Presentation.Presentation.Gameplay.Knife;
using Presentation.Presentation.Gameplay.Target;

namespace Presentation.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Tells the knife to stop moving and passes impact point to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandler<KnifeView, TargetView>
    {
        protected override void HandleCollision(KnifeView knife, TargetView target, CollisionData data)
        {
            knife.StopMovement();
            target.Stick(knife, data.ImpactPoint);
        }
    }
}

