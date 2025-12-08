namespace Core.Data
{
    /// <summary>
    /// Contains configuration data for knife throwing behavior.
    /// </summary>
    public struct KnifeThrowConfiguration
    {
        /// <summary>
        /// The speed at which knives are thrown.
        /// </summary>
        public float ThrowSpeed;
        
        /// <summary>
        /// X component of the spawn position offset relative to the thrower.
        /// </summary>
        public float SpawnPositionOffsetX;
        
        /// <summary>
        /// Y component of the spawn position offset relative to the thrower.
        /// </summary>
        public float SpawnPositionOffsetY;
    }
}

