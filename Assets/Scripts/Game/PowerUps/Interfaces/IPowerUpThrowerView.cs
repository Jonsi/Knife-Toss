using Game.KnifeThrower.Interfaces;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Specialized interface for power-up projectile throwing that does not affect the main magazine.
    /// </summary>
    public interface IPowerUpThrowerView : IProjectileThrower
    {
        /// <summary>
        /// Clears all currently loaded knives without throwing them, returning them to the pool.
        /// </summary>
        void ClearLoadedKnives();
    }
}
