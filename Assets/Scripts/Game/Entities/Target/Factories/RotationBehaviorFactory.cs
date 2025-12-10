using Game.Entities.Target.Data;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;

namespace Game.Entities.Target.Factories
{
    /// <summary>
    /// Factory for creating rotation behaviors based on target configuration.
    /// </summary>
    public class RotationBehaviorFactory
    {
        /// <summary>
        /// Creates an IRotationBehavior instance.
        /// </summary>
        /// <param name="config">The target configuration containing rotation parameters.</param>
        /// <returns>A configured IRotationBehavior instance.</returns>
        public IRotationBehavior Create(TargetConfig config)
        {
            float speed = config.RotationSpeed * config.RotationDirection;
            return new ConstantRotationBehavior(speed);
        }
    }
}
