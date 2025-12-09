using System;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Contains position and rotation data for a knife sticking to a target.
    /// </summary>
    [Serializable]
    public struct StickPositionData
    {
        /// <summary>
        /// The position relative to target center.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The rotation angle in degrees (knife pointing toward target center).
        /// </summary>
        public float RotationAngle;

        public StickPositionData(Vector2 position, float rotationAngle)
        {
            Position = position;
            RotationAngle = rotationAngle;
        }
    }
}
