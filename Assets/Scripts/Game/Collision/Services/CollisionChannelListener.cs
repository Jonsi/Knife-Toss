using System;
using KnifeToss.Common.Events;
using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Collision.Interfaces;
using VContainer.Unity;

namespace KnifeToss.Game.Collision.Services
{
    /// <summary>
    /// Listens to the collision event channel and routes events to handlers.
    /// </summary>
    public class CollisionChannelListener : IStartable, IDisposable
    {
        private readonly IEventChannel<CollisionData> _CollisionChannel;
        private readonly ICollisionHandlerRouter _HandlerRouter;

        public CollisionChannelListener(
            IEventChannel<CollisionData> collisionChannel,
            ICollisionHandlerRouter handlerRouter)
        {
            _CollisionChannel = collisionChannel;
            _HandlerRouter = handlerRouter;
        }

        public void Start()
        {
            _CollisionChannel.Subscribe(OnCollision);
        }

        public void Dispose()
        {
            _CollisionChannel.Unsubscribe(OnCollision);
        }

        private void OnCollision(CollisionData data)
        {
            _HandlerRouter.Route(data);
        }
    }
}
