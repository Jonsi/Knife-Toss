using Game.PowerUps.Configs;

namespace Game.PowerUps.Data
{
    public struct PowerUpCancelledEventData
    {
        public PowerUpConfigBase Config;

        public PowerUpCancelledEventData(PowerUpConfigBase config)
        {
            Config = config;
        }
    }
}
