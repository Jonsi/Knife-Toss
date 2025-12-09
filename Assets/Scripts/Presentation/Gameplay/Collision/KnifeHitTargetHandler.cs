using Core.Abstractions;
using Presentation.Presentation.Gameplay.Knife;
using Presentation.Presentation.Gameplay.Target;

namespace Presentation.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Handles collisions between knives and targets.
    /// Calculates stick position and attaches the knife to the target.
    /// </summary>
    public class KnifeHitTargetHandler : CollisionHandler<KnifeView, TargetView>
    {
        private readonly ITargetService _targetService;

        public KnifeHitTargetHandler(ITargetService targetService)
        {
            _targetService = targetService;
        }

        protected override void HandleCollision(KnifeView knife, TargetView target)
        {
            var stickData = _targetService.GetStickPosition(
                knife.transform.position,
                target.Center,
                target.Radius);

            knife.transform.SetParent(target.Transform);
            knife.transform.localPosition = stickData.Position;
            knife.transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, stickData.RotationAngle);

            knife.Stick();
        }
    }
}

