using Game.PowerUps.Configs;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Tracks runtime state for the currently active power-up.
    /// </summary>
    public class PowerUpRuntimeState
    {
        /// <summary>
        /// Gets or sets whether a power-up is active.
        /// </summary>
        public bool HasActivePowerUp { get; set; }
        /// <summary>
        /// Gets or sets the active configuration.
        /// </summary>
        public PowerUpConfigBase ActiveConfig { get; set; }

        public void Clear()
        {
            HasActivePowerUp = false;
            ActiveConfig = null;
        }
    }
}
