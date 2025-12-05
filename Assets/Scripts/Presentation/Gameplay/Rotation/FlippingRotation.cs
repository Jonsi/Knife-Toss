using UnityEngine;

namespace Presentation.Gameplay.Rotation
{
    public sealed class FlippingRotation : IRotationBehaviour
    {
        private readonly float speed;
        private readonly float flipInterval;
        private readonly float flipProbability;
        private float timer;
        private int direction = 1;

        /// <summary>
        /// Performs the FlippingRotation operation.
        /// </summary>
        public FlippingRotation(float speed, float flipInterval, float flipProbability)
        {
            this.speed = speed;
            this.flipInterval = Mathf.Max(0.01f, flipInterval);
            this.flipProbability = Mathf.Clamp01(flipProbability);
            timer = this.flipInterval;
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

            timer -= deltaTime;
            if (timer <= 0f)
            {
                if (Random.value <= flipProbability)
                {
                    direction *= -1;
                }

                timer = flipInterval;
            }

            float rotationAmount = speed * direction * deltaTime;
            target.Rotate(0f, 0f, rotationAmount);
        }

        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset()
        {
            direction = 1;
            timer = flipInterval;
        }
    }
}
