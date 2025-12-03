using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    public sealed class PulsatingRotation : IRotationBehaviour
    {
        private readonly float baseSpeed;
        private readonly float amplitude;
        private readonly float frequency;
        private float elapsed;

        public PulsatingRotation(float baseSpeed, float amplitude, float frequency)
        {
            this.baseSpeed = baseSpeed;
            this.amplitude = amplitude;
            this.frequency = Mathf.Max(0f, frequency);
        }

        public void ApplyRotation(Transform target, float deltaTime)
        {
            if (target == null)
            {
                return;
            }

            elapsed += deltaTime;
            float pulsate = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * amplitude;
            float rotationAmount = (baseSpeed + pulsate) * deltaTime;
            target.Rotate(0f, 0f, rotationAmount);
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}
