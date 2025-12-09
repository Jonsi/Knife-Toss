using KnifeToss.Core.Abstractions.Collision;
using VContainer.Unity;

namespace Presentation.Gameplay.Collision
{
    /// <summary>
    /// Registers all collision handlers with the collision router on startup.
    /// This service lives in Presentation layer where handlers are defined.
    /// </summary>
    public class CollisionHandlerRegistrar : IStartable
    {
        private readonly ICollisionRouter _router;
        private readonly KnifeHitTargetHandler _knifeHitTargetHandler;
        private readonly KnifeHitKnifeHandler _knifeHitKnifeHandler;

        public CollisionHandlerRegistrar(
            ICollisionRouter router,
            KnifeHitTargetHandler knifeHitTargetHandler,
            KnifeHitKnifeHandler knifeHitKnifeHandler)
        {
            _router = router;
            _knifeHitTargetHandler = knifeHitTargetHandler;
            _knifeHitKnifeHandler = knifeHitKnifeHandler;
        }

        public void Start()
        {
            _router.Register(_knifeHitTargetHandler);
            _router.Register(_knifeHitKnifeHandler);
        }
    }
}

