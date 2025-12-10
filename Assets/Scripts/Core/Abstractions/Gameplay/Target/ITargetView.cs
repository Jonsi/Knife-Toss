using System;

namespace KnifeToss.Core.Abstractions.Gameplay.Target
{
    /// <summary>
    /// Interface for target view presentation components.
    /// </summary>
    public interface ITargetView
    {
        /// <summary>
        /// Gets the current rotation angle of the target in degrees.
        /// </summary>
        float Rotation { get; }

        /// <summary>
        /// Gets the radius of the target.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Event raised when the target is destroyed.
        /// </summary>
        event Action<ITargetView> OnDestroyed;
    }
}
