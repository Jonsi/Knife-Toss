using KnifeToss.Core.Data.Collision;

namespace KnifeToss.Core.Abstractions.Collision
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

