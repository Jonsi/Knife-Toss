using Game.PowerUps.Configs;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Payload for when a power-up is consumed.
    /// </summary>
    public class PowerUpUsedEventData
    {
        public PowerUpConfigBase Config { get; }
        public int RemainingCount { get; }

        public PowerUpUsedEventData(PowerUpConfigBase config, int remainingCount)
        {
            Config = config;
            RemainingCount = remainingCount;
        }
    }
}
