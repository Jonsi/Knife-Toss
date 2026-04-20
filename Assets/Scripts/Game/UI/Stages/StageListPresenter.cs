using System;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
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

        /// <summary>
        /// Creates a presenter for stage list UI.
        /// </summary>
        /// <param name="playerDataService">Player progression source.</param>
        /// <param name="gameData">Runtime stage selection state.</param>
        public StageListPresenter(
            PlayerDataService playerDataService,
            GameData gameData,
            RewardClaimTracker rewardClaimTracker)
        {
            _PlayerDataService = playerDataService;
            _GameData = gameData;
            _RewardClaimTracker = rewardClaimTracker;
        }

        /// <summary>
        /// Gets the stage index that should be focused or selected in UI.
        /// </summary>
        /// <param name="stagesCollection">Catalog used for bounds.</param>
        /// <returns>Resolved stage index within unlocked bounds.</returns>
        public int GetFocusedStageIndex(StagesCollection stagesCollection)
        {
            int stageCount = GetStageCount(stagesCollection);
            if (_PlayerDataService == null || _GameData == null)
            {
                return 0;
            }

            return _PlayerDataService.ClampStageIndexToUnlockedRange(_GameData.CurrentStageIndex, stageCount);
        }

        /// <summary>
        /// Builds UI data for a single stage item.
        /// </summary>
        /// <param name="stagesCollection">Catalog used to resolve the stage.</param>
        /// <param name="stageIndex">Zero-based stage index.</param>
        public StageListItemData GetItemData(StagesCollection stagesCollection, int stageIndex)
        {
            var stages = stagesCollection?.Stages;
            if (stages == null || stageIndex < 0 || stageIndex >= stages.Count)
            {
                return null;
            }

            int stageCount = GetStageCount(stagesCollection);
            int focusedStageIndex = GetFocusedStageIndex(stagesCollection);
            StageConfig stageConfig = stages[stageIndex];
            return stageConfig == null
                ? null
                : BuildItemData(stageConfig, stageIndex, stageCount, focusedStageIndex);
        }

        /// <summary>
        /// Applies an unlocked stage selection to runtime state.
        /// </summary>
        /// <param name="stagesCollection">Catalog used for bounds and lock checks.</param>
        /// <param name="stageIndex">Requested stage index.</param>
        /// <returns>True when the selection was accepted.</returns>
        public bool TrySelectStage(StagesCollection stagesCollection, int stageIndex)
        {
            if (_PlayerDataService == null || _GameData == null)
            {
                return false;
            }

            int stageCount = GetStageCount(stagesCollection);
            if (!_PlayerDataService.IsStageUnlocked(stageIndex, stageCount))
            {
                return false;
            }

            _GameData.CurrentStageIndex = _PlayerDataService.ClampStageIndexToUnlockedRange(stageIndex, stageCount);
            return true;
        }

        private StageListItemData BuildItemData(
            StageConfig stageConfig,
            int stageIndex,
            int stageCount,
            int focusedStageIndex)
        {
            Sprite icon = stageConfig.LayoutSettings != null
                ? stageConfig.LayoutSettings.StageIcon
                : null;
            bool hasReward = HasConfiguredRewards(stageConfig);

            return new StageListItemData
            {
                Icon = icon,
                StageIndex = stageIndex,
                StageNumber = stageIndex + StageNumberOffset,
                StageName = stageConfig.StageName,
                IsLocked = _PlayerDataService == null || !_PlayerDataService.IsStageUnlocked(stageIndex, stageCount),
                IsCurrent = stageIndex == focusedStageIndex,
                HasReward = hasReward,
                CanClaimReward = hasReward && _RewardClaimTracker != null && _RewardClaimTracker.IsStageCompleteAvailable(stageIndex),
                IsRewardClaimed = hasReward && _RewardClaimTracker != null && _RewardClaimTracker.IsStageCompleteClaimed(stageIndex),
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
