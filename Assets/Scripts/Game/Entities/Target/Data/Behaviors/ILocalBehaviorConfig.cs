using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Defines a configuration capable of producing a local-space behavior.
    /// </summary>
    public interface ILocalBehaviorConfig
    {
        /// <summary>
        /// Gets the kind identifier for this local behavior.
        /// </summary>
        TargetLocalBehaviorKind Kind { get; }

        /// <summary>
        /// Creates the runtime local behavior.
        /// </summary>
        /// <returns>Behavior instance.</returns>
        ILocalTargetBehavior CreateLocalBehavior();
    }
}
