using Core.Abstractions;
using Core.Data;
using Core.Domain;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Represents the StagesConfigCatalog class.
    /// </summary>
    public class StagesConfigCatalog : ILevelCatalog
    {
        private readonly StagesConfig stagesConfig;

        /// <summary>
        /// Performs the StagesConfigCatalog operation.
        /// </summary>
        public StagesConfigCatalog(StagesConfig config)
        {
            stagesConfig = config;
        }

        /// <summary>
        /// Performs the GetRules operation.
        /// </summary>
        public LevelRules GetRules(int stageIndex, int levelIndex)
        {
            LevelData data = stagesConfig != null ? stagesConfig.GetLevelData(stageIndex, levelIndex) : null;
            return LevelRules.FromLevelData(data);
        }

        /// <summary>
        /// Performs the GetLevelData operation.
        /// </summary>
        public LevelData GetLevelData(int stageIndex, int levelIndex)
        {
            return stagesConfig != null ? stagesConfig.GetLevelData(stageIndex, levelIndex) : null;
        }

        /// <summary>
        /// Gets the number of levels in a specific stage.
        /// </summary>
        public int GetLevelCountInStage(int stageIndex)
        {
            return stagesConfig != null ? stagesConfig.GetLevelCountInStage(stageIndex) : 0;
        }

        /// <summary>
        /// Gets the total number of stages.
        /// </summary>
        public int GetStageCount()
        {
            return stagesConfig != null ? stagesConfig.GetStageCount() : 0;
        }

        /// <summary>
        /// Checks if a stage and level combination is valid.
        /// </summary>
        public bool IsValidStageAndLevel(int stageIndex, int levelIndex)
        {
            return stagesConfig != null && stagesConfig.IsValidStageAndLevel(stageIndex, levelIndex);
        }
    }
}
