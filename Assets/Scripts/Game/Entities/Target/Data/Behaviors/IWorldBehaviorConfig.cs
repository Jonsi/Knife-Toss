using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Defines a configuration capable of producing a world-space behavior.
    /// </summary>
    public interface IWorldBehaviorConfig
    {
        /// <summary>
        /// Creates the runtime world behavior.
        /// </summary>
        /// <returns>Behavior instance.</returns>
        IWorldTargetBehavior CreateWorldBehavior();
    }
}

