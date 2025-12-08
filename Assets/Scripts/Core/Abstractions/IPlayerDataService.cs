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

        int GetCurrentStage(int fallbackStage);
        void SetCurrentStage(int stageIndex);
        void ResetToStage(int stageIndex);
        void AddFruitsHit(int count);
        int GetTotalFruitsHit();
    }
}
