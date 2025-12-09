using System;
using System.Collections.Generic;
using Core.Abstractions;
using Core.Data;

namespace App.Services
{
    /// <summary>
    /// Routes collision events to appropriate handlers based on the types involved.
    /// Supports bidirectional collision detection (source-target or target-source).
    /// </summary>
    public class CollisionRouter : ICollisionRouter
    {
        private readonly Dictionary<(Type, Type), ICollisionHandler> _handlers = new();

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
            _handlers[(typeof(THit), typeof(THitBy))] = handler;
        }

        /// <summary>
        /// Routes a collision to the appropriate handler based on the cached types.
        /// </summary>
        /// <param name="data">The collision data containing source and target.</param>
        public void Route(CollisionData data)
        {
            if (data.Source == null || data.Target == null)
                return;

            var sourceType = data.Source.HitableType;
            var targetType = data.Target.HitableType;

            var key = (sourceType, targetType);
            if (_handlers.TryGetValue(key, out var handler))
            {
                handler.Handle(data);
                return;
            }

            var reversedKey = (targetType, sourceType);
            if (_handlers.TryGetValue(reversedKey, out var reversedHandler))
            {
                var reversedData = new CollisionData(data.Target, data.Source, data.ImpactPoint);
                reversedHandler.Handle(reversedData);
            }
        }
    }
}
