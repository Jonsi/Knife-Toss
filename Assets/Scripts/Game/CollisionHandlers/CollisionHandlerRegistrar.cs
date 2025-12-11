using Common.Collision.Interfaces;
using VContainer.Unity;

namespace Game.CollisionHandlers
{
    /// <summary>
    /// Registers all collision handlers with the collision handlerRouter on startup.
    /// This service lives in Presentation layer where handlers are defined.
    /// </summary>
    public class CollisionHandlerRegistrar : IStartable
    {
        private readonly ICollisionHandlerRouter _HandlerRouter;
        private readonly KnifeHitTargetHandler _KnifeHitTargetHandler;
        private readonly KnifeHitKnifeHandler _KnifeHitKnifeHandler;
        private readonly KnifeHitCollectibleHandler _KnifeHitCollectibleHandler;

        public CollisionHandlerRegistrar(
            ICollisionHandlerRouter handlerRouter,
            KnifeHitTargetHandler knifeHitTargetHandler,
            KnifeHitKnifeHandler knifeHitKnifeHandler,
            KnifeHitCollectibleHandler knifeHitCollectibleHandler)
        {
            _HandlerRouter = handlerRouter;
            _KnifeHitTargetHandler = knifeHitTargetHandler;
            _KnifeHitKnifeHandler = knifeHitKnifeHandler;
            _KnifeHitCollectibleHandler = knifeHitCollectibleHandler;
        }

        public void Start()
        {
            _HandlerRouter.Register(_KnifeHitTargetHandler);
            _HandlerRouter.Register(_KnifeHitKnifeHandler);
            _HandlerRouter.Register(_KnifeHitCollectibleHandler);
        }
    }
}
