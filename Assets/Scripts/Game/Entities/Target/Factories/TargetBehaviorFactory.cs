using Game.Entities.Target.Data;
using Game.Entities.Target.Data.Behaviors;

namespace Game.Entities.Target.Factories
{
    /// <summary>
    /// Factory responsible for instantiating runtime target behaviors.
    /// </summary>
    public class TargetBehaviorFactory
    {
        /// <summary>
        /// Creates runtime behaviors for the provided configuration.
        /// </summary>
        /// <param name="config">Target configuration.</param>
        /// <returns>Behavior collection.</returns>
        public TargetBehaviorCollection Create(TargetConfig config)
        {
            if (config?.Behaviors == null)
                return new TargetBehaviorCollection();

            return config.Behaviors.CreateRuntimeCollection();
        }
    }
}

