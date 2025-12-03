using KnifeToss.Core.Data;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Defines the ILevelProgressStore interface.
    /// </summary>
    public interface ILevelProgressStore
    {
        /// <summary>
        /// Performs the LoadHighestLevel operation.
        /// </summary>
        int LoadHighestLevel(int fallbackLevel);
        /// <summary>
        /// Performs the SaveHighestLevel operation.
        /// </summary>
        void SaveHighestLevel(int levelIndex);
        LevelProgressData LoadProgress();
        /// <summary>
        /// Performs the SaveProgress operation.
        /// </summary>
        void SaveProgress(LevelProgressData progress);
    }
}
