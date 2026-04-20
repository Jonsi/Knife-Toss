#if TEST_ENABLED
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Persistence.Data;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using Game.Rewards.Services;
using UnityEngine;

namespace Game.Debugging
{
    public class GeneralCheats
    {
        private readonly GameData _gameData;
        private readonly PlayerDataService _playerDataService;
        private readonly RewardClaimTracker _rewardClaimTracker;

        public GeneralCheats(
            GameData gameData,
            PlayerDataService playerDataService,
            RewardClaimTracker rewardClaimTracker = null)
        {
            _gameData = gameData;
            _playerDataService = playerDataService;
            _rewardClaimTracker = rewardClaimTracker;
        }

        public void ClearProgress()
        {
            if (Application.isPlaying)
            {
                if (_gameData == null) return;
                _gameData.Reset();
                _playerDataService?.ResetAllData();
            }
            else
            {
                var store = new FilePlayerDataStore();
                store.Save(new PlayerData());
            }
        }

        public void UnlockAllStagesAsWon()
        {
            if (!Application.isPlaying || _playerDataService == null)
            {
                return;
            }

            var stagesCollection = FindLoadedStagesCollection();
            int stageCount = stagesCollection?.Stages?.Count ?? 0;
            if (stageCount <= 0)
            {
                Debug.LogWarning("GeneralCheats: Could not resolve a loaded stages collection.");
                return;
            }

            int lastStageIndex = stageCount - 1;
            var playerData = _playerDataService.GetData() ?? new PlayerData();

            playerData.Progress ??= new PlayerProgressData();
            playerData.Progress.CurrentStageIndex = lastStageIndex;
            playerData.Progress.HighestStageIndex = lastStageIndex;

            if (_gameData != null)
            {
                _gameData.CurrentStageIndex = lastStageIndex;
            }

            MarkStageRewardsAvailable(playerData, stagesCollection);
            _playerDataService.ForceSave();
        }

        private void MarkStageRewardsAvailable(PlayerData playerData, StagesCollection stagesCollection)
        {
            if (playerData == null || stagesCollection?.Stages == null)
            {
                return;
            }

            playerData.RewardClaims ??= new RewardClaimData();
            playerData.RewardClaims.AvailableStageRewards ??= new System.Collections.Generic.List<int>();
            playerData.RewardClaims.ClaimedStageRewards ??= new System.Collections.Generic.List<int>();

            for (int stageIndex = 0; stageIndex < stagesCollection.Stages.Count; stageIndex++)
            {
                var stageConfig = stagesCollection.Stages[stageIndex];
                if (!HasConfiguredRewards(stageConfig))
                {
                    continue;
                }

                if (_rewardClaimTracker != null)
                {
                    _rewardClaimTracker.MarkStageCompleteAvailable(stageIndex);
                    continue;
                }

                if (playerData.RewardClaims.ClaimedStageRewards.Contains(stageIndex) ||
                    playerData.RewardClaims.AvailableStageRewards.Contains(stageIndex))
                {
                    continue;
                }

                playerData.RewardClaims.AvailableStageRewards.Add(stageIndex);
            }
        }

        private static bool HasConfiguredRewards(Game.Level.Configurations.Stage.StageConfig stageConfig)
        {
            var rewards = stageConfig?.Rewards;
            if (rewards == null)
            {
                return false;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static StagesCollection FindLoadedStagesCollection()
        {
            var loadedCollections = Resources.FindObjectsOfTypeAll<StagesCollection>();
            if (loadedCollections == null || loadedCollections.Length == 0)
            {
                return null;
            }

            foreach (var collection in loadedCollections)
            {
                if (collection?.Stages != null && collection.Stages.Count > 0)
                {
                    return collection;
                }
            }

            return null;
        }
    }
}
#endif
