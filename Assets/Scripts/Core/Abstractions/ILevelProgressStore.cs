using KnifeToss.Core.Data;

namespace KnifeToss.Core.Abstractions
{
    public interface ILevelProgressStore
    {
        int LoadHighestLevel(int fallbackLevel);
        void SaveHighestLevel(int levelIndex);
        LevelProgressData LoadProgress();
        void SaveProgress(LevelProgressData progress);
    }
}
