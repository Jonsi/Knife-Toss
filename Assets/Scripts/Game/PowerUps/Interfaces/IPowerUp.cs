using Game.PowerUps.Configs;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Represents a power-up definition.
    /// </summary>
    public interface IPowerUp
    {
        /// <summary>
        /// Gets the configuration asset backing the power-up.
        /// </summary>
        PowerUpConfigBase Config { get; }
    }
}
