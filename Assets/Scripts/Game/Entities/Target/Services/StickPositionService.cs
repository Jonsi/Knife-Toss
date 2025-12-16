using UnityEngine;

namespace Game.Entities.Target.Services
{
    public class StickPositionService
    {
        private const float ROTATION_FACING_CENTER_OFFSET = 90f;

        /// <summary>
        /// Calculates the local position for a stickable on a target.
        /// </summary>
        public Vector2 CalculatePosition(float targetRadius, float parentScale, float angleDegrees, float radiusOffset)
        {
            // Calculate world radius of the target
            float worldRadius = targetRadius * parentScale;
            
            // Calculate desired distance from center in world units
            float worldDistance = worldRadius + radiusOffset;
            
            // Convert back to local distance (relative to parent scale)
            // We use a small epsilon to avoid division by zero
            float localDistance = parentScale > 0.001f ? worldDistance / parentScale : worldDistance;

            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(angleRadians) * localDistance,
                Mathf.Sin(angleRadians) * localDistance
            );
        }

        /// <summary>
        /// Calculates the local rotation for a stickable on a target.
        /// </summary>
        public Quaternion CalculateRotation(float angleDegrees, float rotationOffset)
        {
            float finalRotation = angleDegrees + ROTATION_FACING_CENTER_OFFSET + rotationOffset;
            return Quaternion.Euler(0f, 0f, finalRotation);
        }
    }
}

