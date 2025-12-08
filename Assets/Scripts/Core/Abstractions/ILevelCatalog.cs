using Core.Data;
using Core.Domain;

namespace Core.Abstractions
{
    /// <summary>
    /// Defines the ILevelCatalog interface for accessing level data by stage and level indices.
    /// </summary>
    public interface ILevelCatalog
    {
        LevelRules GetRules(int stageIndex, int levelIndex);
        LevelData GetLevelData(int stageIndex, int levelIndex);
        int GetLevelCountInStage(int stageIndex);
        int GetStageCount();
        bool IsValidStageAndLevel(int stageIndex, int levelIndex);
        StageLayoutSettings GetStageLayoutSettings(int stageIndex);
    }
}
