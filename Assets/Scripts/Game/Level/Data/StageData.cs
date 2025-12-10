using System.Collections.Generic;
using Game.Entities.Target.Interfaces;
using Game.Level.Interfaces;

namespace Game.Level.Data
{
    /// <summary>
    /// Pure data container for the current stage/level session state.
    /// Writable only within Core assembly (States), Readable by Services.
    /// </summary>
    public class StageData
    {
        public ILevelCatalog LevelCatalog { get; }
        public ILevelSpawner LevelSpawner { get; }

        public int CurrentStageIndex { get; internal set; } = 0;
        public int CurrentLevelIndex { get; internal set; } = 0;

        public int KnifeCount { get; internal set; }
        public int KnivesThrown { get; internal set; }
        public int KnivesStuck { get; internal set; }
        public bool IsThrowingActive { get; internal set; }
        
        public List<ITargetView> ActiveTargets { get; } = new List<ITargetView>();

        public bool HasKnivesRemaining => KnivesThrown < KnifeCount;
        public bool AreAllTargetsDestroyed => ActiveTargets.Count == 0;

        public StageData(ILevelCatalog levelCatalog, ILevelSpawner levelSpawner)
        {
            LevelCatalog = levelCatalog;
            LevelSpawner = levelSpawner;
        }

        internal void ResetLevelState()
        {
            KnivesThrown = 0;
            KnivesStuck = 0;
            IsThrowingActive = false;
            ActiveTargets.Clear();
        }
    }
}

