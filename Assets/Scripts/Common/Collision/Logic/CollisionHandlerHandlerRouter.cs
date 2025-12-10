using System;
using System.Collections.Generic;
using Common.Collision.Data;
using Common.Collision.Interfaces;
using Common.Events;

namespace Common.Collision.Logic
{
    /// <summary>
    /// Routes collision events to appropriate handlers based on the types involved.
    /// Supports bidirectional collision detection (source-target or target-source).
    /// </summary>
    public class CollisionHandlerHandlerRouter : ICollisionHandlerRouter, IDisposable
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
        /// Registers a collision handlerBase for a specific pair of types.
        /// </summary>
        /// <typeparam name="THit">The type of the source object.</typeparam>
        /// <typeparam name="THitBy">The type of the target object.</typeparam>
        /// <param name="handlerBase">The handlerBase to register.</param>
        public void Register<THit, THitBy>(CollisionHandlerBase<THit, THitBy> handlerBase)
            where THit : class, IHitable
            where THitBy : class, IHitable
        {
            _Handlers[(typeof(THit), typeof(THitBy))] = handlerBase;
        }

        private void Route(CollisionData data)
        {
            if (data.Source == null || data.Target == null)
                return;

            var sourceType = data.Source.HitableType;
            var targetType = data.Target.HitableType;

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
            var reversedData = new CollisionData(data.Target, data.Source, data.ImpactPoint);
            reversedHandler.Handle(reversedData);
        }

        private void OnCollision(CollisionData data)
        {
            Route(data);
        }
    }
}
