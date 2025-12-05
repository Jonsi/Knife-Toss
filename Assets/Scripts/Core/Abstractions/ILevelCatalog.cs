using Core.Data;
using Core.Domain;

namespace Core.Abstractions
{
    /// <summary>
    /// Defines the ILevelCatalog interface.
    /// </summary>
    public interface ILevelCatalog
    {
        LevelRules GetRules(int levelIndex);
        LevelData GetLevelData(int levelIndex);
    }
}
