using Core.Data;

namespace Core.Abstractions
{
    /// <summary>
    /// High-level API for loading, mutating, and saving player data.
    /// </summary>
    public interface IPlayerDataService
    {
        PlayerData Data { get; }
        PlayerSettingsData Settings { get; }
        PlayerProgressData Progress { get; }
        LevelProgressData LevelProgress { get; }

        void Save();
        void Reload();
        void Reset();
        void Replace(PlayerData newData);

        (int stageIndex, int levelIndex) GetCurrentStageAndLevel(int fallbackStage, int fallbackLevel);
        void SetCurrentStageAndLevel(int stageIndex, int levelIndex);
        void ResetToStageLevel(int stageIndex, int levelIndex);
        void AddFruitsHit(int count);
        int GetTotalFruitsHit();
    }
}
