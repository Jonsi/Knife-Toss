using Game.Target.Data;
using Game.Target.Interfaces;
using Game.Target.Logic;

namespace Game.Target.Factories
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
