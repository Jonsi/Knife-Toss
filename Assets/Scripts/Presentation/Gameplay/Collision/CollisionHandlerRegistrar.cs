using KnifeToss.Core.Abstractions.Collision;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay.Collision
{
    /// <summary>
    /// Registers all collision handlers with the collision router on startup.
    /// This service lives in Presentation layer where handlers are defined.
    /// </summary>
    public class CollisionHandlerRegistrar : IStartable
    {
        private readonly ICollisionRouter _Router;
        private readonly KnifeHitTargetHandler _KnifeHitTargetHandler;
        private readonly KnifeHitKnifeHandler _KnifeHitKnifeHandler;

        public CollisionHandlerRegistrar(
            ICollisionRouter router,
            KnifeHitTargetHandler knifeHitTargetHandler,
            KnifeHitKnifeHandler knifeHitKnifeHandler)
        {
            _Router = router;
            _KnifeHitTargetHandler = knifeHitTargetHandler;
            _KnifeHitKnifeHandler = knifeHitKnifeHandler;
        }

        public void Start()
        {
            _Router.Register(_KnifeHitTargetHandler);
            _Router.Register(_KnifeHitKnifeHandler);
        }
    }
}

