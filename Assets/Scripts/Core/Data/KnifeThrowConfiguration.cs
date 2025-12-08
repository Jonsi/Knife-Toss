using System;

namespace Core.Data
{
    /// <summary>
    /// Contains configuration data for knife throwing behavior.
    /// </summary>
    [Serializable]
    public class KnifeThrowConfiguration
    {
        /// <summary>
        /// Gets or sets the speed at which knives are thrown.
        /// </summary>
        public float ThrowSpeed;
        
        /// <summary>
        /// Gets or sets the X component of the spawn position offset relative to the thrower's position.
        /// </summary>
        public float SpawnPositionOffsetX;
        
        /// <summary>
        /// Gets or sets the Y component of the spawn position offset relative to the thrower's position.
        /// </summary>
        public float SpawnPositionOffsetY;
    }
}

