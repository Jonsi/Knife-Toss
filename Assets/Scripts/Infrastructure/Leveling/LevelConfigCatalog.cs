using Core.Abstractions;
using Core.Data;
using Core.Domain;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Represents the LevelConfigCatalog class.
    /// </summary>
    public class LevelConfigCatalog : ILevelCatalog
    {
        private readonly LevelConfig levelConfig;

        /// <summary>
        /// Performs the LevelConfigCatalog operation.
        /// </summary>
        public LevelConfigCatalog(LevelConfig config)
        {
            levelConfig = config;
        }

        /// <summary>
        /// Performs the GetRules operation.
        /// </summary>
        public LevelRules GetRules(int levelIndex)
        {
            LevelData data = levelConfig != null ? levelConfig.GetLevelData(levelIndex) : null;
            return LevelRules.FromLevelData(data);
        }

        /// <summary>
        /// Performs the GetLevelData operation.
        /// </summary>
        public LevelData GetLevelData(int levelIndex)
        {
            return levelConfig != null ? levelConfig.GetLevelData(levelIndex) : null;
        }
    }
}
