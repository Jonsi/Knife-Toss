using Game.PowerUps.Configs;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Manages the state transitions between normal and powerup throwing.
    /// </summary>
    public interface IThrowerStateManager
    {
        /// <summary>
        /// Whether a projectile powerup is currently active.
        /// </summary>
        bool IsProjectilePowerUpActive { get; }
        
        /// <summary>
        /// Switches to powerup throwing mode.
        /// </summary>
        void ActivateProjectilePowerUp(PowerUpConfigBase config);
        
        /// <summary>
        /// Returns to normal throwing mode after powerup use.
        /// </summary>
        void DeactivateProjectilePowerUp();
        
        /// <summary>
        /// Cancels active powerup and returns to normal throwing.
        /// </summary>
        void CancelProjectilePowerUp();
    }
}
