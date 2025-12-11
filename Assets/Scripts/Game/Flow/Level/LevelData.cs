using System.Collections.Generic;
using Game.Entities.Target.Interfaces;
using Game.Level.Data;
using Game.Level.Interfaces;

namespace Game.Flow.Level
{
    /// <summary>
    /// Runtime data for the currently active level.
    /// </summary>
    public class LevelData
    {
        public ILevelSpawner LevelSpawner { get; }

        public LevelConfig Config { get; internal set; }

        public int KnifeCount { get; internal set; }
        public int KnivesThrown { get; internal set; }
        public int KnivesStuck { get; internal set; }
        public bool IsThrowingActive { get; internal set; }
        
        /// <summary>
        /// Total currency items collected in this level.
        /// </summary>
        public int CollectedCount { get; internal set; }
        
        public List<ITargetView> ActiveTargets { get; } = new List<ITargetView>();

        public bool HasKnivesRemaining => KnivesThrown < KnifeCount;
        public bool AreAllTargetsDestroyed => ActiveTargets.Count == 0;

        public LevelData(ILevelSpawner levelSpawner)
        {
            LevelSpawner = levelSpawner;
        }

        public void Reset()
        {
            KnivesThrown = 0;
            KnivesStuck = 0;
            CollectedCount = 0;
            IsThrowingActive = false;
            ActiveTargets.Clear();
        }
    }
}
