using System.Collections.Generic;
using Game.Entities.Target.Interfaces;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Runtime behavior container holding instantiated behaviors.
    /// </summary>
    public class TargetBehaviorCollection
    {
        private readonly List<ILocalTargetBehavior> _LocalBehaviors = new();
        private readonly List<IWorldTargetBehavior> _WorldBehaviors = new();

        /// <summary>
        /// Gets the local behaviors.
        /// </summary>
        public IReadOnlyList<ILocalTargetBehavior> LocalBehaviors => _LocalBehaviors;

        /// <summary>
        /// Gets the world behaviors.
        /// </summary>
        public IReadOnlyList<IWorldTargetBehavior> WorldBehaviors => _WorldBehaviors;

        /// <summary>
        /// Adds a local behavior instance.
        /// </summary>
        /// <param name="behavior">Behavior instance.</param>
        public void AddLocalBehavior(ILocalTargetBehavior behavior)
        {
            if (behavior == null)
                return;
            _LocalBehaviors.Add(behavior);
        }

        /// <summary>
        /// Adds a world behavior instance.
        /// </summary>
        /// <param name="behavior">Behavior instance.</param>
        public void AddWorldBehavior(IWorldTargetBehavior behavior)
        {
            if (behavior == null)
                return;
            _WorldBehaviors.Add(behavior);
        }
    }
}

