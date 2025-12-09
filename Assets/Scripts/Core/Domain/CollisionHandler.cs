using Core.Abstractions;
using Core.Data;

namespace Core.Domain
{
    /// <summary>
    /// Generic base class for type-safe collision handling.
    /// Inherit from this class to handle collisions between specific types.
    /// </summary>
    /// <typeparam name="THit">The type of the source object.</typeparam>
    /// <typeparam name="THitBy">The type of the target object.</typeparam>
    public abstract class CollisionHandler<THit, THitBy> : ICollisionHandler
        where THit : class, IHitable
        where THitBy : class, IHitable
    {
        public void Handle(CollisionData data)
        {
            HandleCollision((THit)data.Source, (THitBy)data.Target, data);
        }

        /// <summary>
        /// Handles the collision with strongly-typed parameters.
        /// </summary>
        /// <param name="source">The strongly-typed source object.</param>
        /// <param name="target">The strongly-typed target object.</param>
        /// <param name="data">The collision data containing impact point and other information.</param>
        protected abstract void HandleCollision(THit source, THitBy target, CollisionData data);
    }
}

