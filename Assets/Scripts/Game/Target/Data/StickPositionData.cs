using System;

namespace KnifeToss.Game.Target.Data
{
    /// <summary>
    /// Contains position and rotation configuration for a knife sticking to a target.
    /// </summary>
    [Serializable]
    public struct StickPositionData
    {
        /// <summary>
        /// The radial distance offset from the surface (e.g. penetration depth).
        /// </summary>
        public float offset;

        /// <summary>
        /// The rotation offset in degrees applied to the base rotation (which points toward target center).
        /// </summary>
        public float rotationOffset;

        public StickPositionData(float offset, float rotationOffset)
        {
            this.offset = offset;
            this.rotationOffset = rotationOffset;
        }
    }
}
