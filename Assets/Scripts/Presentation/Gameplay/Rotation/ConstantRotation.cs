using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    public sealed class ConstantRotation : IRotationBehaviour
    {
        private readonly float speed;

        /// <summary>
        /// Performs the ConstantRotation operation.
        /// </summary>
        public ConstantRotation(float speed)
        {
            this.speed = speed;
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

            float rotationAmount = speed * deltaTime;
            target.Rotate(0f, 0f, rotationAmount);
        }

        /// <summary>
        /// Performs the Reset operation.
        /// </summary>
        public void Reset()
        {
        }
    }
}
