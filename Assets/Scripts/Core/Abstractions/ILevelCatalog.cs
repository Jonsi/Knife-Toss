using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;

namespace KnifeToss.Core.Abstractions
{
    public interface ILevelCatalog
    {
        LevelRules GetRules(int levelIndex);
        LevelData GetLevelData(int levelIndex);
    }
}
