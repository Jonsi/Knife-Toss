using Game.Collision.Data;
using Game.Collision.Interfaces;

namespace Game.Collision.Logic
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
            HandleCollision(data);
        }

        /// <summary>
        /// Handles the collision using the provided collision data.
        /// </summary>
        /// <param name="data">The collision data containing impact point and other information.</param>
        protected abstract void HandleCollision(CollisionData data);
    }
}

