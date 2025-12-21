using Game.Entities.Knife.Views;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Provides power-up activation and execution services.
    /// </summary>
    public interface IPowerUpService
    {
        /// <summary>
        /// Gets whether a power-up is currently active.
        /// </summary>
        bool HasActivePowerUp { get; }
        /// <summary>
        /// Gets the currently active power-up configuration.
        /// </summary>
        PowerUpConfigBase ActiveConfig { get; }

        /// <summary>
        /// Attempts to activate the specified power-up.
        /// </summary>
        bool TryActivate(PowerUpConfigBase config);
        /// <summary>
        /// Clears any active power-up state.
        /// </summary>
        void ClearActive();
        /// <summary>
        /// Attempts to handle a throw using the active power-up.
        /// </summary>
        bool TryHandleThrow(PowerUpThrowContext context);
        /// <summary>
        /// Resets runtime state when a level starts.
        /// </summary>
        void OnLevelStarted();
        /// <summary>
        /// Prepares the loaded knife based on the active power-up.
        /// </summary>
        void PrepareLoadedKnives(System.Collections.Generic.IReadOnlyList<KnifeView> knives);
    }
}
