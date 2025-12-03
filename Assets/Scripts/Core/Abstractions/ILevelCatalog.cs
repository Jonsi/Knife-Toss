using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;

namespace KnifeToss.Core.Abstractions
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
