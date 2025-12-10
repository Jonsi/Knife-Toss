using KnifeToss.Game.Collision.Data;

namespace KnifeToss.Game.Collision.Interfaces
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

