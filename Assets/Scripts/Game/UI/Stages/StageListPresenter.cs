using System;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
using Game.Persistence.Services;
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

        /// <summary>
        /// Creates a presenter for stage list UI.
        /// </summary>
        /// <param name="playerDataService">Player progression source.</param>
        /// <param name="gameData">Runtime stage selection state.</param>
        public StageListPresenter(
            PlayerDataService playerDataService,
            GameData gameData)
        {
            _PlayerDataService = playerDataService;
            _GameData = gameData;
        }

        /// <summary>
        /// Gets the stage index that should be focused or selected in UI.
        /// </summary>
        /// <param name="stagesCollection">Catalog used for bounds.</param>
        /// <returns>Resolved stage index within unlocked bounds.</returns>
        public int GetFocusedStageIndex(StagesCollection stagesCollection)
        {
            int stageCount = GetStageCount(stagesCollection);
            return _PlayerDataService.ClampStageIndexToUnlockedRange(_GameData.CurrentStageIndex, stageCount);
        }

        /// <summary>
        /// Rebuilds the stage list under the provided content root.
        /// </summary>
        /// <param name="stagesCollection">Catalog used to build items.</param>
        /// <param name="stageItemPrefab">Prefab used for each item.</param>
        /// <param name="contentRoot">Parent transform for instantiated items.</param>
        /// <param name="onStageSelected">Callback invoked for unlocked selections.</param>
        public void Rebuild(
            StagesCollection stagesCollection,
            UiStageItem stageItemPrefab,
            Transform contentRoot,
            Action<int> onStageSelected)
        {
            if (stagesCollection == null || stageItemPrefab == null || contentRoot == null)
            {
                return;
            }

            Clear(contentRoot);

            int stageCount = GetStageCount(stagesCollection);
            int focusedStageIndex = GetFocusedStageIndex(stagesCollection);
            var stages = stagesCollection.Stages;
            if (stages == null)
            {
                return;
            }

            for (int stageIndex = 0; stageIndex < stageCount; stageIndex++)
            {
                StageConfig stageConfig = stages[stageIndex];
                if (stageConfig == null)
                {
                    continue;
                }

                var item = UnityEngine.Object.Instantiate(stageItemPrefab, contentRoot);
                item.Initialize(BuildItemData(stageConfig, stageIndex, stageCount, focusedStageIndex), onStageSelected);
            }
        }

        /// <summary>
        /// Applies an unlocked stage selection to runtime state.
        /// </summary>
        /// <param name="stagesCollection">Catalog used for bounds and lock checks.</param>
        /// <param name="stageIndex">Requested stage index.</param>
        /// <returns>True when the selection was accepted.</returns>
        public bool TrySelectStage(StagesCollection stagesCollection, int stageIndex)
        {
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

            return new StageListItemData
            {
                Icon = icon,
                StageIndex = stageIndex,
                StageNumber = stageIndex + StageNumberOffset,
                StageName = stageConfig.StageName,
                IsLocked = !_PlayerDataService.IsStageUnlocked(stageIndex, stageCount),
                IsCurrent = stageIndex == focusedStageIndex,
            };
        }

        private void Clear(Transform contentRoot)
        {
            foreach (Transform child in contentRoot)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
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
