using KnifeToss.Game.Collision.Interfaces;
using KnifeToss.Game.Collision.Logic.Handlers;
using VContainer.Unity;

namespace KnifeToss.Game.Collision.Logic
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

        public CollisionHandlerRegistrar(
            ICollisionHandlerRouter handlerRouter,
            KnifeHitTargetHandler knifeHitTargetHandler,
            KnifeHitKnifeHandler knifeHitKnifeHandler)
        {
            _HandlerRouter = handlerRouter;
            _KnifeHitTargetHandler = knifeHitTargetHandler;
            _KnifeHitKnifeHandler = knifeHitKnifeHandler;
        }

        public void Start()
        {
            _HandlerRouter.Register(_KnifeHitTargetHandler);
            _HandlerRouter.Register(_KnifeHitKnifeHandler);
        }
    }
}

