using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
using Game.Level.Interfaces;
using Game.Persistence.Services;
using Game.Rewards.Services;
using UnityEngine;

namespace Game.UI.Stages
{
    /// <summary>
    /// Builds and binds stage list UI state from progression data.
    /// </summary>
    public class StageListPresenter
    {
        private const int StageNumberOffset = 1;

        private readonly PlayerDataService _PlayerDataService;
        private readonly GameData _GameData;
        private readonly RewardClaimTracker _RewardClaimTracker;
        private readonly IStagesCollectionCatalog _StagesCatalog;

        /// <summary>
        /// Creates a presenter for stage list UI.
        /// </summary>
        /// <param name="playerDataService">Player progression source.</param>
        /// <param name="gameData">Runtime stage selection state.</param>
        /// <param name="rewardClaimTracker">Reward claim UI state.</param>
        /// <param name="stagesCatalog">All world collections in order.</param>
        public StageListPresenter(
            PlayerDataService playerDataService,
            GameData gameData,
            RewardClaimTracker rewardClaimTracker,
            IStagesCollectionCatalog stagesCatalog)
        {
            _PlayerDataService = playerDataService;
            _GameData = gameData;
            _RewardClaimTracker = rewardClaimTracker;
            _StagesCatalog = stagesCatalog;
        }

        /// <summary>
        /// Gets the focused local stage index for a world row, or -1 when another world is active.
        /// </summary>
        public int GetFocusedLocalStageIndex(StagesCollection stagesCollection, int collectionIndex)
        {
            int stageCount = GetStageCount(stagesCollection);
            if (_PlayerDataService == null || _GameData == null || _StagesCatalog == null)
            {
                return 0;
            }

            if (_GameData.CurrentCollectionIndex != collectionIndex)
            {
                return -1;
            }

            return _PlayerDataService.ClampLocalStageToUnlockedRange(
                collectionIndex,
                _GameData.CurrentStageIndex,
                stageCount,
                _StagesCatalog);
        }

        /// <summary>
        /// Builds UI data for a single stage item within a world.
        /// </summary>
        public StageListItemData GetItemData(StagesCollection stagesCollection, int collectionIndex, int localStageIndex)
        {
            var stages = stagesCollection?.Stages;
            if (stages == null || localStageIndex < 0 || localStageIndex >= stages.Count || _StagesCatalog == null)
            {
                return null;
            }

            int stageCount = GetStageCount(stagesCollection);
            int focusedLocal = GetFocusedLocalStageIndex(stagesCollection, collectionIndex);
            StageConfig stageConfig = stages[localStageIndex];
            return stageConfig == null
                ? null
                : BuildItemData(stageConfig, collectionIndex, localStageIndex, stageCount, focusedLocal);
        }

        /// <summary>
        /// Applies an unlocked stage selection to runtime state for a world and local index.
        /// </summary>
        public bool TrySelectStage(StagesCollection stagesCollection, int collectionIndex, int localStageIndex)
        {
            if (_PlayerDataService == null || _GameData == null || _StagesCatalog == null)
            {
                return false;
            }

            int stageCount = GetStageCount(stagesCollection);
            if (!_PlayerDataService.IsStageUnlocked(collectionIndex, localStageIndex, _StagesCatalog))
            {
                return false;
            }

            _GameData.CurrentCollectionIndex = collectionIndex;
            _GameData.CurrentStageIndex = _PlayerDataService.ClampLocalStageToUnlockedRange(
                collectionIndex,
                localStageIndex,
                stageCount,
                _StagesCatalog);
            return true;
        }

        private StageListItemData BuildItemData(
            StageConfig stageConfig,
            int collectionIndex,
            int localStageIndex,
            int stageCount,
            int focusedLocalStageIndex)
        {
            Sprite icon = stageConfig.LayoutSettings != null
                ? stageConfig.LayoutSettings.TargetSprite
                : null;
            bool hasReward = HasConfiguredRewards(stageConfig);
            int displayNumber = _StagesCatalog.GetCumulativeStageNumberDisplay(collectionIndex, localStageIndex);

            return new StageListItemData
            {
                Icon = icon,
                StageIndex = localStageIndex,
                StageNumber = displayNumber,
                StageName = stageConfig.StageName,
                IsLocked = _PlayerDataService == null ||
                           !_PlayerDataService.IsStageUnlocked(collectionIndex, localStageIndex, _StagesCatalog),
                IsCurrent = focusedLocalStageIndex >= 0 && localStageIndex == focusedLocalStageIndex,
                HasReward = hasReward,
                CanClaimReward = hasReward && _RewardClaimTracker != null &&
                                 _RewardClaimTracker.IsStageCompleteAvailable(collectionIndex, localStageIndex),
                IsRewardClaimed = hasReward && _RewardClaimTracker != null &&
                                  _RewardClaimTracker.IsStageCompleteClaimed(collectionIndex, localStageIndex),
                IsStageComplete = _PlayerDataService != null &&
                                   _PlayerDataService.IsStageCleared(collectionIndex, localStageIndex, _StagesCatalog),
            };
        }

        private static bool HasConfiguredRewards(StageConfig stageConfig)
        {
            var rewards = stageConfig?.Rewards;
            if (rewards == null)
            {
                return false;
            }

            for (var i = 0; i < rewards.Count; i++)
            {
                if (rewards[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetStageCount(StagesCollection stagesCollection)
        {
            if (stagesCollection?.Stages == null || stagesCollection.Stages.Count == 0)
            {
                return StageNumberOffset;
            }

            return stagesCollection.Stages.Count;
        }
    }
}
