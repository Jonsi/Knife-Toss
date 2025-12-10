using Game.Collision.Interfaces;
using UnityEngine;

namespace Game.Collision.Data
{
    /// <summary>
    /// Immutable data structure containing collision information.
    /// </summary>
    public readonly struct CollisionData
    {
        /// <summary>
        /// The object that initiated the collision (e.g., the knife).
        /// </summary>
        public readonly IHitable source;

        /// <summary>
        /// The object that was hit (e.g., the target).
        /// </summary>
        public readonly IHitable target;

        /// <summary>
        /// The world position where the impact occurred.
        /// </summary>
        public readonly Vector2 impactPoint;

        public CollisionData(IHitable source, IHitable target, Vector2 impactPoint)
        {
            this.source = source;
            this.target = target;
            this.impactPoint = impactPoint;
        }
    }
}

