using Game.Entities.Target.Interfaces;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Defines a power-up effect that can be applied to targets.
    /// </summary>
    public interface IPowerUpEffect
    {
        /// <summary>
        /// Applies the effect to the specified target.
        /// </summary>
        /// <param name="target">Target to apply the effect to.</param>
        void Apply(ITargetView target);

        /// <summary>
        /// Removes the effect from the specified target.
        /// </summary>
        /// <param name="target">Target to remove the effect from.</param>
        void Remove(ITargetView target);
    }
}
