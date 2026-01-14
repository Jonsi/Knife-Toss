using Game.PowerUps.Data;

namespace Game.PowerUps.Interfaces
{
    /// <summary>
    /// Context provided to power-up configs for execution.
    /// </summary>
    public interface IPowerUpExecutionContext
    {
        /// <summary>
        /// Gets the throw context for throw-based power-ups.
        /// </summary>
        PowerUpThrowContext ThrowContext { get; }
    }
}
