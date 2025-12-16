using Game.Level.Data;
using Game.Level.Interfaces;

namespace Game.Level.Catalogs
{
    /// <summary>
    /// Implementation of ILevelCatalog using the scriptable object StagesConfigAsset.
    /// </summary>
    public class StagesConfigCatalog : ILevelCatalog
    {
        private readonly StagesConfigAsset _ConfigAsset;

        /// <summary>
        /// Initializes a new instance of the StagesConfigCatalog class.
        /// </summary>
        /// <param name="configAsset">The StagesConfigAsset containing the game stages data.</param>
        public StagesConfigCatalog(StagesConfigAsset configAsset)
        {
            _ConfigAsset = configAsset;
        }

        /// <summary>
        /// Gets the total number of stages available in the configuration.
        /// </summary>
        public int StageCount => _ConfigAsset.Stages.Count;

        /// <summary>
        /// Gets the number of levels for a specific stage.
        /// </summary>
        /// <param name="stageIndex">The index of the stage to query.</param>
        /// <returns>The number of levels in the specified stage, or 0 if the stage index is invalid.</returns>
        public int GetLevelCount(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= _ConfigAsset.Stages.Count)
                return 0;
            return _ConfigAsset.Stages[stageIndex].Levels.Count;
        }

        /// <summary>
        /// Retrieves the configuration for a specific level.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <param name="levelIndex">The index of the level within the stage.</param>
        /// <returns>The level configuration, or null if not found.</returns>
        public LevelConfig GetLevelConfig(int stageIndex, int levelIndex)
        {
            if (stageIndex < 0 || stageIndex >= _ConfigAsset.Stages.Count)
                return null;
            
            var levels = _ConfigAsset.Stages[stageIndex].Levels;
            if (levelIndex < 0 || levelIndex >= levels.Count)
                return null;

            return levels[levelIndex];
        }

        /// <summary>
        /// Retrieves the configuration for the specified stage index.
        /// </summary>
        /// <param name="stageIndex">The stage index to look up.</param>
        /// <returns>The stage configuration, or null if the index is invalid.</returns>
        public StageConfig GetStageConfig(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= _ConfigAsset.Stages.Count)
                return null;

            return _ConfigAsset.Stages[stageIndex];
        }
    }
}
