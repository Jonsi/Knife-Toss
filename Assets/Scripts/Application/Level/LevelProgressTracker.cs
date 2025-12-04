using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;

namespace KnifeToss.Application.Level
{
    /// <summary>
    /// Tracks and persists level progress.
    /// </summary>
    public sealed class LevelProgressTracker
    {
        private readonly ILevelProgressStore _progressStore;

        public LevelProgressTracker(ILevelProgressStore progressStore)
        {
            _progressStore = progressStore;
        }

        public int GetHighestLevel(int defaultLevel)
        {
            return _progressStore.LoadHighestLevel(defaultLevel);
        }

        public void SaveProgress(LevelSession session)
        {
            if (_progressStore == null)
            {
                return;
            }

            _progressStore.SaveHighestLevel(session.LevelIndex);
        }

        public void SaveStars(LevelSession session, int stars)
        {
            if (_progressStore == null)
            {
                return;
            }

            var progress = _progressStore.LoadProgress();
            if (!progress.starsPerLevel.ContainsKey(session.LevelIndex) || progress.starsPerLevel[session.LevelIndex] < stars)
            {
                progress.starsPerLevel[session.LevelIndex] = stars;
                _progressStore.SaveProgress(progress);
            }
        }
    }
}

