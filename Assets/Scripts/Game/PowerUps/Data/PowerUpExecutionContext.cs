using Game.PowerUps.Interfaces;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Concrete implementation of the power-up execution context.
    /// </summary>
    public class PowerUpExecutionContext : IPowerUpExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the execution context.
        /// </summary>
        public PowerUpExecutionContext(PowerUpThrowContext throwContext)
        {
            ThrowContext = throwContext;
        }

        /// <summary>
        /// Gets the throw context for throw-based power-ups.
        /// </summary>
        public PowerUpThrowContext ThrowContext { get; }
    }
}
