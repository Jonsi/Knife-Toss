using System;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Stores counts for each available power-up type.
    /// </summary>
    [Serializable]
    public class PowerUpInventoryData
    {
        public int SuperKnifeCount;
        public int MultipleKnivesCount;
        public int CollectiblesBladeCount;
    }
}
