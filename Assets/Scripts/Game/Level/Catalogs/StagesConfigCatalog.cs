using Game.Level.Configurations.Stage;
using Game.Level.Data;
using Game.Level.Interfaces;
using Game.Level.Services;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Implementation of ILevelCatalog for the active stage collection in the current session.
    /// </summary>
    public class StagesConfigCatalog : ILevelCatalog
    {
        private readonly WorldAssetService _WorldAssets;

        /// <summary>
        /// Creates a catalog bound to the loaded world asset service.
        /// </summary>
        public StagesConfigCatalog(WorldAssetService worldAssets)
        {
            _WorldAssets = worldAssets;
        }

        private StagesCollection ActiveCollection => _WorldAssets?.ActiveCollection;

        /// <inheritdoc />
        public int StageCount
        {
            get
            {
                var stages = ActiveCollection?.Stages;
                return stages != null ? stages.Count : 0;
            }
        }

        /// <inheritdoc />
        public int GetLevelCount(int stageIndex)
        {
            var stages = ActiveCollection?.Stages;
            if (stages == null || stageIndex < 0 || stageIndex >= stages.Count)
            {
                return 0;
            }

            var stage = stages[stageIndex];
            return stage == null ? 0 : stage.Levels.Count;
        }

        /// <inheritdoc />
        public LevelConfig GetLevelConfig(int stageIndex, int levelIndex)
        {
            var stages = ActiveCollection?.Stages;
            if (stages == null || stageIndex < 0 || stageIndex >= stages.Count)
            {
                return null;
            }

            var stage = stages[stageIndex];
            if (stage == null)
            {
                return null;
            }

            var levels = stage.Levels;
            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                return null;
            }

            return levels[levelIndex];
        }

        /// <inheritdoc />
        public StageConfig GetStageConfig(int stageIndex)
        {
            var stages = ActiveCollection?.Stages;
            if (stages == null || stageIndex < 0 || stageIndex >= stages.Count)
            {
                return null;
            }

            return stages[stageIndex];
        }

        /// <summary>
        /// Finds the stage index of the asset in the active collection, or -1 if not found.
        /// </summary>
        public int GetStageIndexOrNegative(StageConfig stageConfig)
        {
            var stages = ActiveCollection?.Stages;
            if (stageConfig == null || stages == null)
            {
                return -1;
            }

            for (var i = 0; i < stages.Count; i++)
            {
                if (stages[i] == stageConfig)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the level index for the given level asset within the stage, or -1 if not found.
        /// </summary>
        public int GetLevelIndexInStageOrNegative(int stageIndex, LevelConfig levelConfig)
        {
            var stage = GetStageConfig(stageIndex);
            if (stage?.Levels == null || levelConfig == null)
            {
                return -1;
            }

            for (var i = 0; i < stage.Levels.Count; i++)
            {
                if (stage.Levels[i] == levelConfig)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
