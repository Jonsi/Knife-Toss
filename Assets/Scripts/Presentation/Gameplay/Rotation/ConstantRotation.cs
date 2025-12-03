using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Rotation
{
    public sealed class ConstantRotation : IRotationBehaviour
    {
        private readonly float speed;

        public ConstantRotation(float speed)
        {
            this.speed = speed;
        }

        public void ApplyRotation(Transform target, float deltaTime)
        {
            if (target == null)
            {
                return;
            }

            float rotationAmount = speed * deltaTime;
            target.Rotate(0f, 0f, rotationAmount);
        }

        public void Reset()
        {
        }
    }
}
