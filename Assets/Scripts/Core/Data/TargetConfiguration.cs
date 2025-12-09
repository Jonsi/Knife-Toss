using System;

namespace Core.Data
{
    /// <summary>
    /// Contains configuration data for target behavior.
    /// </summary>
    [Serializable]
    public class TargetConfiguration
    {
        /// <summary>
        /// Gets or sets the rotation speed in degrees per second.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Gets or sets the radius of the target.
        /// </summary>
        public float Radius;
    }
}

