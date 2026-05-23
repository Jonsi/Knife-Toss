using System;
using Game.Missions.Daily.Data;
using Game.PowerUps.Data;
using Newtonsoft.Json;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Root container for all persisted player data.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public PlayerProgressData Progress = new PlayerProgressData();
        public PlayerSettingsData Settings = new PlayerSettingsData();
        public PlayerResourcesData Resources = new PlayerResourcesData();
        public RewardClaimData RewardClaims = new RewardClaimData();
        public DailyMissionData DailyMissions = new DailyMissionData();
        public PlayerOnboardingData Onboarding = new PlayerOnboardingData();

        [JsonProperty("PowerUpInventory")]
        private PowerUpInventoryData LegacyPowerUpInventory
        {
            set
            {
                if (Resources == null)
                    Resources = new PlayerResourcesData();
                Resources.PowerUps = value ?? new PowerUpInventoryData();
            }
        }
    }
}
