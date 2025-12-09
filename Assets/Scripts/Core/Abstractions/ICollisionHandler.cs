using KnifeToss.Core.Data;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Interface for handling collisions between two IHitable objects.
    /// </summary>
    public interface ICollisionHandler
    {
        /// <summary>
        /// Handles a collision between source and target.
        /// </summary>
        /// <param name="data">The collision data containing source, target, and impact point.</param>
        void Handle(CollisionData data);
    }
}

