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
        private readonly ICollisionRouter _Router;

        public CollisionChannelListener(
            IEventChannel<CollisionData> collisionChannel,
            ICollisionRouter router)
        {
            _CollisionChannel = collisionChannel;
            _Router = router;
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
            _Router.Route(data);
        }
    }
}
