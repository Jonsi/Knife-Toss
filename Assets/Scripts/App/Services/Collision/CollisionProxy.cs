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
        private readonly IEventChannel<CollisionData> _collisionChannel;
        private readonly ICollisionRouter _router;

        public CollisionProxy(
            IEventChannel<CollisionData> collisionChannel,
            ICollisionRouter router)
        {
            _collisionChannel = collisionChannel;
            _router = router;
        }

        public void Start()
        {
            _collisionChannel.Subscribe(OnCollision);
        }

        public void Dispose()
        {
            _collisionChannel.Unsubscribe(OnCollision);
        }

        private void OnCollision(CollisionData data)
        {
            _router.Route(data);
        }
    }
}
