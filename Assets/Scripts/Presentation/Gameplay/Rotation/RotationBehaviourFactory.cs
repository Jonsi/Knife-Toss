using KnifeToss.Core.Data;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    public sealed class RotationBehaviourFactory
    {
        public IRotationBehaviour Create(TargetRotationConfig config)
        {
            if (config == null)
            {
                return new ConstantRotation(0f);
            }

            switch (config.behaviourType)
            {
                case RotationBehaviourType.Pulsating:
                    return new PulsatingRotation(
                        config.speed,
                        config.pulsating != null ? config.pulsating.amplitude : 0f,
                        config.pulsating != null ? config.pulsating.frequency : 1f);
                case RotationBehaviourType.Flipping:
                    return new FlippingRotation(
                        config.speed,
                        config.flipping != null ? config.flipping.flipInterval : 1f,
                        config.flipping != null ? config.flipping.flipProbability : 0.5f);
                case RotationBehaviourType.AccelerationCurve:
                    return new AccelerationCurveRotation(
                        config.speed,
                        config.acceleration != null ? config.acceleration.accelerationCurve : null,
                        config.acceleration != null ? config.acceleration.curveLoopDuration : 2f);
                default:
                    return new ConstantRotation(config.speed);
            }
        }
    }
}
