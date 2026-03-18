using System;

namespace Game.Entities.Target.Interfaces
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

        /// <summary>
        /// Initializes the target with configuration.
        /// </summary>
        /// <param name="config">Target configuration.</param>
        void Initialize(Game.Entities.Target.Data.TargetConfig config);

        /// <summary>
        /// Applies a speed multiplier to target behaviors.
        /// </summary>
        /// <param name="multiplier">Speed multiplier to apply.</param>
        void SetSpeedMultiplier(float multiplier);

        /// <summary>
        /// Clears all behavior speed modifiers.
        /// </summary>
        void ClearRotationModifiers();
    }
}
