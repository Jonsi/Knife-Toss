using Core.Data;
using Core.Domain;

namespace Core.Abstractions
{
    /// <summary>
    /// Interface for routing collision events to appropriate handlers.
    /// </summary>
    public interface ICollisionRouter
    {
        /// <summary>
        /// Registers a collision handler for a specific pair of types.
        /// </summary>
        /// <typeparam name="THit">The type of the source object.</typeparam>
        /// <typeparam name="THitBy">The type of the target object.</typeparam>
        /// <param name="handler">The handler to register.</param>
        void Register<THit, THitBy>(CollisionHandler<THit, THitBy> handler)
            where THit : class, IHitable
            where THitBy : class, IHitable;

        /// <summary>
        /// Routes a collision to the appropriate handler based on the types involved.
        /// </summary>
        /// <param name="data">The collision data containing source and target.</param>
        void Route(CollisionData data);
    }
}

