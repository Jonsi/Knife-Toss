using Core.Abstractions;
using Core.Data;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Implementation of target geometry calculations.
    /// Handles surface point calculation and stick positioning.
    /// </summary>
    public class TargetGeometry : ITargetGeometry
    {
        /// <summary>
        /// Calculates the position and rotation for a knife to stick to the target surface.
        /// </summary>
        /// <param name="knifeWorldPosition">The world position of the knife at collision.</param>
        /// <param name="targetCenter">The world position of the target center.</param>
        /// <param name="targetRadius">The radius of the target.</param>
        /// <returns>A StickPositionData containing the calculated position and rotation.</returns>
        public StickPositionData CalculateStickPosition(
            Vector2 knifeWorldPosition,
            Vector2 targetCenter,
            float targetRadius)
        {
            Vector2 delta = knifeWorldPosition - targetCenter;
            float distance = delta.magnitude;

            if (distance < 0.0001f)
            {
                return new StickPositionData
                {
                    Position = new Vector2(targetRadius, 0f),
                    RotationAngle = 0f
                };
            }

            Vector2 normalized = delta.normalized;
            Vector2 surfacePosition = normalized * targetRadius;
            float angleDegrees = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;

            return new StickPositionData
            {
                Position = surfacePosition,
                RotationAngle = angleDegrees
            };
        }
    }
}
