using Game.PowerUps.Services;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Interface for powerups that affect the game world without changing throw behavior.
    /// These are applied immediately and consumed, allowing normal throwing to continue.
    /// </summary>
    public interface IWorldEffectPowerUp
    {
        /// <summary>
        /// Duration of the world effect in seconds.
        /// </summary>
        float EffectDuration { get; }
        
        /// <summary>
        /// Applies the world effect using the provided service.
        /// </summary>
        void ApplyWorldEffect(PowerUpEffectService effectService);
    }
}
