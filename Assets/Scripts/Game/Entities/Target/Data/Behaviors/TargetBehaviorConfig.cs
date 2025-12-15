using System;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Base configuration shared by all target behaviors.
    /// </summary>
    [Serializable]
    public abstract class TargetBehaviorConfig
    {
        /// <summary>
        /// Gets the behavior kind represented by this configuration.
        /// </summary>
        public abstract TargetBehaviorKind Kind { get; }
    }
}
