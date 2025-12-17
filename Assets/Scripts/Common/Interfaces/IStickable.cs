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
        /// Applies a sorting order so the stickable renders in sync with its parent target.
        /// </summary>
        /// <param name="sortingOrder">Calculated sorting order.</param>
        void ApplySortingOrder(int sortingOrder);
    }
}
