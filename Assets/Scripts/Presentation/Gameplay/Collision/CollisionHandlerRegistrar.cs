using App.Services;
using Core.Abstractions;
using VContainer.Unity;

namespace Presentation.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Registers all collision handlers with the collision router on startup.
    /// This service lives in Presentation layer where handlers are defined.
    /// </summary>
    public class CollisionHandlerRegistrar : IStartable
    {
        private readonly ICollisionRouter _router;
        private readonly KnifeHitTargetHandler _knifeHitTargetHandler;

        public CollisionHandlerRegistrar(
            ICollisionRouter router,
            KnifeHitTargetHandler knifeHitTargetHandler)
        {
            _router = router;
            _knifeHitTargetHandler = knifeHitTargetHandler;
        }

        public void Start()
        {
            _router.Register(_knifeHitTargetHandler);
        }
    }
}

