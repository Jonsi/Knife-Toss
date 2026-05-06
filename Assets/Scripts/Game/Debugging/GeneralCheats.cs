#if TEST_ENABLED
using System;
using System.Collections.Generic;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Level.Configurations.Stage;
using Game.Persistence.Data;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using Game.Rewards.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Debugging
{
    /// <summary>
    /// Test-only cheats for progression and persistence.
    /// </summary>
    public class GeneralCheats
    {
        private const int LastSlotIndexFromCountOffset = 1;

        private readonly GameData _gameData;
        private readonly PlayerDataService _playerDataService;
        private readonly IStagesCollectionCatalog _stagesCollectionCatalog;
        private readonly RewardClaimTracker _rewardClaimTracker;

        /// <summary>
        /// Creates general cheats with optional reward tracker.
        /// </summary>
        public GeneralCheats(
            GameData gameData,
            PlayerDataService playerDataService,
            IStagesCollectionCatalog stagesCollectionCatalog,
            RewardClaimTracker rewardClaimTracker = null)
        {
            _gameData = gameData;
            _playerDataService = playerDataService;
            _stagesCollectionCatalog = stagesCollectionCatalog;
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

            StagesCollectionComposite composite = _stagesCollectionCatalog?.Composite;
            if (composite == null || composite.CollectionsCount <= 0)
            {
                return;
            }

            int lastCollectionIndex = composite.CollectionsCount - LastSlotIndexFromCountOffset;
            int lastLocal = composite.GetStageSlotCountForWorld(lastCollectionIndex) - LastSlotIndexFromCountOffset;
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
            playerData.RewardClaims.AvailableStageRewards ??= new List<StageRewardKey>();
            playerData.RewardClaims.ClaimedStageRewards ??= new List<StageRewardKey>();

            for (int c = 0; c < composite.CollectionsCount; c++)
            {
                int collectionIndex = c;
                WithCollectionLoaded(composite, collectionIndex, collection =>
                {
                    if (collection?.Stages == null)
                    {
                        return;
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
                            _rewardClaimTracker.MarkStageCompleteAvailable(collectionIndex, s);
                            continue;
                        }

                        if (ListContains(playerData.RewardClaims.ClaimedStageRewards, collectionIndex, s) ||
                            ListContains(playerData.RewardClaims.AvailableStageRewards, collectionIndex, s))
                        {
                            continue;
                        }

                        playerData.RewardClaims.AvailableStageRewards.Add(
                            new StageRewardKey { CollectionIndex = collectionIndex, StageIndex = s });
                    }
                });
            }
        }

        private static void WithCollectionLoaded(
            StagesCollectionComposite composite,
            int collectionIndex,
            Action<StagesCollection> use)
        {
            if (composite == null || use == null)
            {
                return;
            }

            string locationKey = composite.GetCollectionLocationKey(collectionIndex);
            if (string.IsNullOrEmpty(locationKey))
            {
                return;
            }

            AsyncOperationHandle<StagesCollection> handle = Addressables.LoadAssetAsync<StagesCollection>(locationKey);
            try
            {
                StagesCollection collection = handle.WaitForCompletion();
                use(collection);
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        private static bool ListContains(List<StageRewardKey> list, int c, int s)
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

        private static bool HasConfiguredRewards(StageConfig stageConfig)
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

    }
}
#endif
