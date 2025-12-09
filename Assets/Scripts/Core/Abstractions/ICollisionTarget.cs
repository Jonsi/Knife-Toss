using UnityEngine;

namespace Core.Abstractions
{
    /// <summary>
    /// Interface for objects that can be hit by a knife and provide collision data.
    /// </summary>
    public interface ICollisionTarget
    {
        /// <summary>
        /// Gets the world position of the target center.
        /// </summary>
        Vector2 Center { get; }

        /// <summary>
        /// Gets the radius of the target.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Gets the transform to parent stuck knives to.
        /// </summary>
        Transform Transform { get; }
    }
}

