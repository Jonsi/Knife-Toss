#if TEST_ENABLED
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
using Game.Persistence.Data;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using Game.Rewards.Services;
using UnityEngine;

namespace Game.Debugging
{
    /// <summary>
    /// Test-only cheats for progression and persistence.
    /// </summary>
    public class GeneralCheats
    {
        private readonly GameData _gameData;
        private readonly PlayerDataService _playerDataService;
        private readonly RewardClaimTracker _rewardClaimTracker;

        /// <summary>
        /// Creates general cheats with optional reward tracker.
        /// </summary>
        public GeneralCheats(
            GameData gameData,
            PlayerDataService playerDataService,
            RewardClaimTracker rewardClaimTracker = null)
        {
            _gameData = gameData;
            _playerDataService = playerDataService;
            _rewardClaimTracker = rewardClaimTracker;
        }

        /// <summary>
        /// Clears persisted and session progression.
        /// </summary>
        public void ClearProgress()
        {
            if (Application.isPlaying)
            {
                if (_gameData == null)
                {
                    return;
                }

                _gameData.Reset();
                _playerDataService?.ResetAllData();
            }
            else
            {
                var store = new FilePlayerDataStore();
                store.Save(new PlayerData());
            }
        }

        /// <summary>
        /// Sets progress to the last stage of the last world and flags rewards.
        /// </summary>
        public void UnlockAllStagesAsWon()
        {
            if (!Application.isPlaying || _playerDataService == null)
            {
                return;
            }

            StagesCollectionComposite composite = FindLoadedComposite();
            if (composite == null || composite.CollectionsCount <= 0)
            {
                return;
            }

            int lastCollectionIndex = composite.CollectionsCount - 1;
            StagesCollection lastCollection = composite.GetCollection(lastCollectionIndex);
            int lastLocal = (lastCollection?.Stages?.Count ?? 0) - 1;
            if (lastLocal < 0)
            {
                return;
            }

            PlayerData playerData = _playerDataService.GetData() ?? new PlayerData();
            playerData.Progress ??= new PlayerProgressData();
            playerData.Progress.CurrentCollectionIndex = lastCollectionIndex;
            playerData.Progress.CurrentStageIndex = lastLocal;
            playerData.Progress.HighestCollectionIndex = lastCollectionIndex;
            playerData.Progress.HighestStageIndex = lastLocal;

            if (_gameData != null)
            {
                _gameData.CurrentCollectionIndex = lastCollectionIndex;
                _gameData.CurrentStageIndex = lastLocal;
            }

            MarkStageRewardsAvailable(playerData, composite);
            _playerDataService.ForceSave();
        }

        private void MarkStageRewardsAvailable(PlayerData playerData, StagesCollectionComposite composite)
        {
            if (playerData == null || composite == null)
            {
                return;
            }

            playerData.RewardClaims ??= new RewardClaimData();
            playerData.RewardClaims.AvailableStageRewards ??= new System.Collections.Generic.List<StageRewardKey>();
            playerData.RewardClaims.ClaimedStageRewards ??= new System.Collections.Generic.List<StageRewardKey>();

            for (int c = 0; c < composite.CollectionsCount; c++)
            {
                StagesCollection collection = composite.GetCollection(c);
                if (collection?.Stages == null)
                {
                    continue;
                }

                for (int s = 0; s < collection.Stages.Count; s++)
                {
                    StageConfig stageConfig = collection.Stages[s];
                    if (!HasConfiguredRewards(stageConfig))
                    {
                        continue;
                    }

                    if (_rewardClaimTracker != null)
                    {
                        _rewardClaimTracker.MarkStageCompleteAvailable(c, s);
                        continue;
                    }

                    if (ListContains(playerData.RewardClaims.ClaimedStageRewards, c, s) ||
                        ListContains(playerData.RewardClaims.AvailableStageRewards, c, s))
                    {
                        continue;
                    }

                    playerData.RewardClaims.AvailableStageRewards.Add(new StageRewardKey { CollectionIndex = c, StageIndex = s });
                }
            }
        }

        private static bool ListContains(System.Collections.Generic.List<StageRewardKey> list, int c, int s)
        {
            if (list == null)
            {
                return false;
            }

            for (var i = 0; i < list.Count; i++)
            {
                StageRewardKey k = list[i];
                if (k != null && k.CollectionIndex == c && k.StageIndex == s)
                {
                    return true;
                }
            }

            return false;
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

        private static StagesCollectionComposite FindLoadedComposite()
        {
            StagesCollectionComposite[] loaded = Resources.FindObjectsOfTypeAll<StagesCollectionComposite>();
            if (loaded == null || loaded.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] != null && loaded[i].CollectionsCount > 0)
                {
                    return loaded[i];
                }
            }

            return null;
        }
    }
}
#endif
