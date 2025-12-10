using KnifeToss.Core.Abstractions.Flow;
using KnifeToss.Core.Abstractions.Gameplay;

namespace App.Services.Flow
{
    /// <summary>
    /// Tracks the player's current stage and level progress.
    /// </summary>
    public class StageProgressTracker : IStageProgressTracker, IStageNavigator
    {
        private readonly ILevelCatalog _LevelCatalog;

        public int CurrentStageIndex { get; private set; }
        public int CurrentLevelIndex { get; private set; }

        public StageProgressTracker(ILevelCatalog levelCatalog)
        {
            _LevelCatalog = levelCatalog;
        }

        public void AdvanceLevel()
        {
            int levelsInCurrentStage = _LevelCatalog.GetLevelCount(CurrentStageIndex);

            if (CurrentLevelIndex + 1 < levelsInCurrentStage)
            {
                CurrentLevelIndex++;
            }
            else
            {
                if (CurrentStageIndex + 1 < _LevelCatalog.StageCount)
                {
                    CurrentStageIndex++;
                    CurrentLevelIndex = 0;
                }
            }
        }

        public void RetryLevel()
        {
        }

        public void RestartStage()
        {
            CurrentLevelIndex = 0;
        }
    }
}
