using UnityEngine;

namespace Common.Interfaces
{
    /// <summary>
    /// Defines the contract for objects that can attach to targets.
    /// </summary>
    public interface IStickable
    {
        /// <summary>
        /// Gets the transform that should be parented when sticking to a target.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Gets the radial offset applied when calculating stick position.
        /// </summary>
        float RadiusOffset { get; }

        /// <summary>
        /// Gets the rotation offset applied when aligning to the target surface.
        /// </summary>
        float RotationOffset { get; }

        /// <summary>
        /// Applies the sorting layer and order so the stickable renders in sync with its parent target.
        /// </summary>
        /// <param name="sortingLayerId">Target sorting layer identifier.</param>
        /// <param name="sortingOrder">Calculated sorting order.</param>
        void ApplySorting(int sortingLayerId, int sortingOrder);
    }
}
