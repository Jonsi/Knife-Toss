using KnifeToss.Game.Collision.Data;
using KnifeToss.Game.Collision.Logic;

namespace KnifeToss.Game.Collision.Interfaces
{
    /// <summary>
    /// Interface for routing collision events to appropriate handlers.
    /// </summary>
    public interface ICollisionHandlerRouter
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
    }
}

