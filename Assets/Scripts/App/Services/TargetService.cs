using Core.Abstractions;
using Core.Data;
using UnityEngine;

namespace App.Services
{
    /// <summary>
    /// Service that coordinates target state and provides geometry calculations.
    /// </summary>
    public class TargetService : ITargetService
    {
        private readonly ITargetGeometry _targetGeometry;

        /// <summary>
        /// Initializes a new instance of TargetService.
        /// </summary>
        /// <param name="targetGeometry">The geometry calculator for target surface positioning.</param>
        public TargetService(ITargetGeometry targetGeometry)
        {
            _targetGeometry = targetGeometry;
        }

        /// <summary>
        /// Calculates the position and rotation for a knife to stick to the target surface.
        /// </summary>
        /// <param name="knifeWorldPosition">The world position of the knife at collision.</param>
        /// <param name="targetCenter">The world position of the target center.</param>
        /// <param name="targetRadius">The radius of the target.</param>
        /// <returns>A StickPositionData containing the calculated position and rotation.</returns>
        public StickPositionData GetStickPosition(Vector2 knifeWorldPosition, Vector2 targetCenter, float targetRadius)
        {
            return _targetGeometry.CalculateStickPosition(knifeWorldPosition, targetCenter, targetRadius);
        }
    }
}
