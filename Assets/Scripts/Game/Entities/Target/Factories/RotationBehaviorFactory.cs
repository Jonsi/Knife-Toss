using System;
using Game.Entities.Target.Data;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using Game.Entities.Target.RotationBehaviors;

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
            string behaviorType = config.RotationBehaviorType ?? "Constant";

            if (behaviorType.Equals("Pulsating", StringComparison.OrdinalIgnoreCase))
                return new PulsatingRotationBehavior(speed, config.PulsateAmplitude, config.PulsateFrequency);

            if (behaviorType.Equals("Flipping", StringComparison.OrdinalIgnoreCase))
                return new FlippingRotationBehavior(speed, config.FlipMinInterval, config.FlipMaxInterval);

            if (behaviorType.Equals("Curve", StringComparison.OrdinalIgnoreCase))
                return new CurveRotationBehavior(config.RotationCurve);

            return new ConstantRotationBehavior(speed);
        }
    }
}
