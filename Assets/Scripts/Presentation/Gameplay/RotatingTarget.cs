using Presentation.Gameplay.Rotation;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Represents the RotatingTarget class.
    /// </summary>
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

        /// <summary>
        /// Performs the Update operation.
        /// </summary>
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

        /// <summary>
        /// Performs the Rotate operation.
        /// </summary>
        private void Rotate()
        {
            float rotationAmount = rotationSpeed * currentDirection * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationAmount);
        }

        /// <summary>
        /// Performs the UpdateDirectionChange operation.
        /// </summary>
        private void UpdateDirectionChange()
        {
            directionChangeTimer -= Time.deltaTime;

            if (directionChangeTimer <= 0f)
            {
                currentDirection = Random.value > 0.5f ? 1 : -1;
                directionChangeTimer = directionChangeInterval;
            }
        }

        /// <summary>
        /// Performs the SetRotationSpeed operation.
        /// </summary>
        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;

            if (rotationBehaviour is ConstantRotation)
            {
                rotationBehaviour = new ConstantRotation(rotationSpeed * currentDirection);
            }
        }

        /// <summary>
        /// Performs the SetDirectionChangeInterval operation.
        /// </summary>
        public void SetDirectionChangeInterval(float interval)
        {
            directionChangeInterval = interval;
            directionChangeTimer = interval;
        }

        /// <summary>
        /// Performs the SetRandomizeDirection operation.
        /// </summary>
        public void SetRandomizeDirection(bool randomize)
        {
            randomizeDirection = randomize;
        }

        /// <summary>
        /// Performs the SetRotationBehaviour operation.
        /// </summary>
        public void SetRotationBehaviour(IRotationBehaviour behaviour)
        {
            rotationBehaviour = behaviour;
            rotationBehaviour?.Reset();
        }

        /// <summary>
        /// Performs the Freeze operation.
        /// </summary>
        public void Freeze()
        {
            isFrozen = true;
        }

        /// <summary>
        /// Performs the Resume operation.
        /// </summary>
        public void Resume()
        {
            isFrozen = false;
        }

        /// <summary>
        /// Performs the ResetRotation operation.
        /// </summary>
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
