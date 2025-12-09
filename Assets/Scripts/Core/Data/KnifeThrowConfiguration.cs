using System;
using UnityEngine;

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
        /// Gets or sets the spawn position offset relative to the thrower's position.
        /// </summary>
        public Vector2 SpawnPositionOffset;
    }
}
