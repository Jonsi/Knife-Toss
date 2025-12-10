using Common.Collision.Logic;

namespace Common.Collision.Interfaces
{
    /// <summary>
    /// Interface for routing collision events to appropriate handlers.
    /// </summary>
    public interface ICollisionHandlerRouter
    {
        /// <summary>
        /// Registers a collision handlerBase for a specific pair of types.
        /// </summary>
        /// <typeparam name="THit">The type of the source object.</typeparam>
        /// <typeparam name="THitBy">The type of the target object.</typeparam>
        /// <param name="handlerBase">The handlerBase to register.</param>
        void Register<THit, THitBy>(CollisionHandlerBase<THit, THitBy> handlerBase)
            where THit : class, IHitable
            where THitBy : class, IHitable;
    }
}

