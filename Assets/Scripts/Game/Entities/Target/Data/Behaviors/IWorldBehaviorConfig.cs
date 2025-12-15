using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Defines a configuration capable of producing a world-space behavior.
    /// </summary>
    public interface IWorldBehaviorConfig
    {
        /// <summary>
        /// Gets the kind identifier for this world behavior.
        /// </summary>
        TargetWorldBehaviorKind Kind { get; }

        /// <summary>
        /// Creates the runtime world behavior.
        /// </summary>
        /// <returns>Behavior instance.</returns>
        IWorldTargetBehavior CreateWorldBehavior();
    }
}
