using Game.PowerUps.Configs;

namespace Game.PowerUps.Data
{
    /// <summary>
    /// Payload for when a power-up is activated.
    /// </summary>
    public class PowerUpActivatedEventData
    {
        public PowerUpConfigBase Config { get; }
        public int RemainingCount { get; }

        public PowerUpActivatedEventData(PowerUpConfigBase config, int remainingCount)
        {
            Config = config;
            RemainingCount = remainingCount;
        }
    }
}
