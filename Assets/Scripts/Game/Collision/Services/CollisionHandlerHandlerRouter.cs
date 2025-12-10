using System;
using System.Collections.Generic;
using Common.Events;
using Game.Collision.Data;
using Game.Collision.Interfaces;
using Game.Collision.Logic;
using VContainer.Unity;

namespace Game.Collision.Services
{
    /// <summary>
    /// Routes collision events to appropriate handlers based on the types involved.
    /// Supports bidirectional collision detection (source-target or target-source).
    /// </summary>
    public class CollisionHandlerHandlerRouter : ICollisionHandlerRouter, IStartable, IDisposable
    {
        private readonly IEventChannel<CollisionData> _CollisionChannel;
        private readonly Dictionary<(Type, Type), ICollisionHandler> _Handlers = new();

        /// <summary>
        /// Initializes a new instance of the router with required dependencies.
        /// </summary>
        /// <param name="collisionChannel">Event channel that emits collision events.</param>
        public CollisionHandlerHandlerRouter(IEventChannel<CollisionData> collisionChannel)
        {
            _CollisionChannel = collisionChannel;
        }

        /// <summary>
        /// Begins listening for collision events.
        /// </summary>
        public void Start()
        {
            _CollisionChannel.Subscribe(OnCollision);
        }

        /// <summary>
        /// Stops listening for collision events and releases subscriptions.
        /// </summary>
        public void Dispose()
        {
            _CollisionChannel.Unsubscribe(OnCollision);
        }

        /// <summary>
        /// Registers a collision handler for a specific pair of types.
        /// </summary>
        /// <typeparam name="THit">The type of the source object.</typeparam>
        /// <typeparam name="THitBy">The type of the target object.</typeparam>
        /// <param name="handler">The handler to register.</param>
        public void Register<THit, THitBy>(CollisionHandler<THit, THitBy> handler)
            where THit : class, IHitable
            where THitBy : class, IHitable
        {
            _Handlers[(typeof(THit), typeof(THitBy))] = handler;
        }

        private void Route(CollisionData data)
        {
            if (data.source == null || data.target == null)
                return;

            var sourceType = data.source.HitableType;
            var targetType = data.target.HitableType;

            var key = (sourceType, targetType);
            if (_Handlers.TryGetValue(key, out var handler))
            {
                handler.Handle(data);
                return;
            }

            var reversedKey = (targetType, sourceType);
            if (!_Handlers.TryGetValue(reversedKey, out var reversedHandler))
            {
                return;
            }
            var reversedData = new CollisionData(data.target, data.source, data.impactPoint);
            reversedHandler.Handle(reversedData);
        }

        private void OnCollision(CollisionData data)
        {
            Route(data);
        }
    }
}
