using System;
using KnifeToss.Core.Abstractions.Collision;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Data.Collision;
using VContainer.Unity;

namespace App.Services.Collision
{
    /// <summary>
    /// Connects the collision event channel to the collision router.
    /// Subscribes to collision events and routes them to appropriate handlers.
    /// </summary>
    public class CollisionProxy : IStartable, IDisposable
    {
        private readonly IEventChannel<CollisionData> _CollisionChannel;
        private readonly ICollisionRouter _Router;

        public CollisionProxy(
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
