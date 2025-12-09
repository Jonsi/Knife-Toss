using KnifeToss.Core.Abstractions.Collision;
using UnityEngine;

namespace KnifeToss.Core.Data.Collision
{
    /// <summary>
    /// Immutable data structure containing collision information.
    /// </summary>
    public readonly struct CollisionData
    {
        /// <summary>
        /// The object that initiated the collision (e.g., the knife).
        /// </summary>
        public readonly IHitable Source;

        /// <summary>
        /// The object that was hit (e.g., the target).
        /// </summary>
        public readonly IHitable Target;

        /// <summary>
        /// The world position where the impact occurred.
        /// </summary>
        public readonly Vector2 ImpactPoint;

        public CollisionData(IHitable source, IHitable target, Vector2 impactPoint)
        {
            Source = source;
            Target = target;
            ImpactPoint = impactPoint;
        }
    }
}

