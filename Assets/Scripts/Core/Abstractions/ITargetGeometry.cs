using Core.Data;
using UnityEngine;

namespace Core.Abstractions
{
    /// <summary>
    /// Interface for target geometry calculations.
    /// </summary>
    public interface ITargetGeometry
    {
        /// <summary>
        /// Calculates the position and rotation for a knife to stick to the target surface.
        /// </summary>
        /// <param name="knifeWorldPosition">The world position of the knife at collision.</param>
        /// <param name="targetCenter">The world position of the target center.</param>
        /// <param name="targetRadius">The radius of the target.</param>
        /// <returns>A StickPositionData containing the calculated position and rotation.</returns>
        StickPositionData CalculateStickPosition(Vector2 knifeWorldPosition, Vector2 targetCenter, float targetRadius);
    }
}

