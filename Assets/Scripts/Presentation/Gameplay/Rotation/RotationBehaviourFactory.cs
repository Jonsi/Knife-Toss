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
                    return new PulsatingRotation(config.speed, config.pulsateAmplitude, config.pulsateFrequency);
                case RotationBehaviourType.Flipping:
                    return new FlippingRotation(config.speed, config.flipInterval, config.flipProbability);
                case RotationBehaviourType.AccelerationCurve:
                    return new AccelerationCurveRotation(config.speed, config.accelerationCurve, config.curveLoopDuration);
                default:
                    return new ConstantRotation(config.speed);
            }
        }
    }
}
