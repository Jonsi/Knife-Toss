using System.Collections.Generic;
using Game.Domain.Level;
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
        public LevelStats Stats { get; } = new LevelStats();

        public ILevelSpawner LevelSpawner { get; }

        public StageConfig StageConfig { get; internal set; }
        public LevelConfig Config { get; internal set; }

        public int KnifeCount
        {
            get => Stats.KnifeCount;
            internal set => Stats.KnifeCount = value;
        }

        public int KnivesThrown
        {
            get => Stats.KnivesThrown;
            internal set => Stats.KnivesThrown = value;
        }

        public int KnivesStuck
        {
            get => Stats.KnivesStuck;
            internal set => Stats.KnivesStuck = value;
        }

        public bool IsThrowingActive
        {
            get => Stats.IsThrowingActive;
            internal set => Stats.IsThrowingActive = value;
        }
        
        /// <summary>
        /// Total currency items collected in this level.
        /// </summary>
        public int CollectedCount
        {
            get => Stats.CollectedCount;
            internal set => Stats.CollectedCount = value;
        }
        
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
