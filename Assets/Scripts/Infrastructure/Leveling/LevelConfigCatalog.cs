using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;

namespace KnifeToss.Infrastructure.Leveling
{
    public class LevelConfigCatalog : ILevelCatalog
    {
        private readonly LevelConfig levelConfig;

        public LevelConfigCatalog(LevelConfig config)
        {
            levelConfig = config;
        }

        public LevelRules GetRules(int levelIndex)
        {
            LevelData data = levelConfig != null ? levelConfig.GetLevelData(levelIndex) : null;
            return LevelRules.FromLevelData(data);
        }

        public LevelData GetLevelData(int levelIndex)
        {
            return levelConfig != null ? levelConfig.GetLevelData(levelIndex) : null;
        }
    }
}
