using KnifeToss.Presentation.Gameplay.Rotation;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    public class RotatingTarget : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Difficulty Settings")]
        [SerializeField] private bool randomizeDirection = true;
        [SerializeField] private float directionChangeInterval = 3f;

        private int currentDirection = 1;
        private float directionChangeTimer;
        private bool isFrozen;
        private IRotationBehaviour rotationBehaviour;

        private void Update()
        {
            if (isFrozen)
            {
                return;
            }

            if (rotationBehaviour != null)
            {
                rotationBehaviour.ApplyRotation(transform, Time.deltaTime);
            }
            else
            {
                Rotate();

                if (randomizeDirection)
                {
                    UpdateDirectionChange();
                }
            }
        }

        private void Rotate()
        {
            float rotationAmount = rotationSpeed * currentDirection * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationAmount);
        }

        private void UpdateDirectionChange()
        {
            directionChangeTimer -= Time.deltaTime;

            if (directionChangeTimer <= 0f)
            {
                currentDirection = Random.value > 0.5f ? 1 : -1;
                directionChangeTimer = directionChangeInterval;
            }
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;

            if (rotationBehaviour is ConstantRotation)
            {
                rotationBehaviour = new ConstantRotation(rotationSpeed * currentDirection);
            }
        }

        public void SetDirectionChangeInterval(float interval)
        {
            directionChangeInterval = interval;
            directionChangeTimer = interval;
        }

        public void SetRandomizeDirection(bool randomize)
        {
            randomizeDirection = randomize;
        }

        public void SetRotationBehaviour(IRotationBehaviour behaviour)
        {
            rotationBehaviour = behaviour;
            rotationBehaviour?.Reset();
        }

        public void Freeze()
        {
            isFrozen = true;
        }

        public void Resume()
        {
            isFrozen = false;
        }

        public void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
            currentDirection = 1;
            directionChangeTimer = directionChangeInterval;
            isFrozen = false;
            rotationBehaviour?.Reset();
        }
    }
}
