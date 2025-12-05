using UnityEngine;

namespace Presentation.Gameplay.Rotation
{
    public sealed class AccelerationCurveRotation : IRotationBehaviour
    {
        private readonly float baseSpeed;
        private readonly AnimationCurve curve;
        private readonly float loopDuration;
        private float elapsed;

        /// <summary>
        /// Performs the AccelerationCurveRotation operation.
        /// </summary>
        public AccelerationCurveRotation(float baseSpeed, AnimationCurve curve, float loopDuration)
        {
            this.baseSpeed = baseSpeed;
            this.curve = curve != null ? new AnimationCurve(curve.keys) : AnimationCurve.Linear(0f, 1f, 1f, 1f);
            this.loopDuration = Mathf.Max(0.01f, loopDuration);
        }

        /// <summary>
        /// Performs the ApplyRotation operation.
        /// </summary>
        public void ApplyRotation(Transform target, float deltaTime)
        {
            if (target == null)
            {
                return;
            }

            elapsed += deltaTime;
            if (elapsed > loopDuration)
            {
                elapsed -= loopDuration;
            }

            float t = Mathf.Clamp01(loopDuration > 0f ? elapsed / loopDuration : 0f);
            float multiplier = curve != null ? curve.Evaluate(t) : 1f;
            float rotationAmount = baseSpeed * multiplier * deltaTime;
            target.Rotate(0f, 0f, rotationAmount);
        }

        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset()
        {
            elapsed = 0f;
        }
    }
}
