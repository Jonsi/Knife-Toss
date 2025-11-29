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

        private void Update()
        {
            Rotate();

            if (randomizeDirection)
            {
                UpdateDirectionChange();
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

        public void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
            currentDirection = 1;
            directionChangeTimer = directionChangeInterval;
        }
    }
}
