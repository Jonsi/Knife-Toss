using System;

namespace KnifeToss.Game.Knife.Data
{
    /// <summary>
    /// Contains configuration data for a knife instance.
    /// </summary>
    [Serializable]
    public struct KnifeData
    {
        /// <summary>
        /// The speed at which the knife travels.
        /// </summary>
        public float speed;
        
        /// <summary>
        /// The offset distance for the sticking point when the knife embeds into a surface.
        /// </summary>
        public float stickingOffset;
    }
}

