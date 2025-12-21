using System.Collections.Generic;
using Game.Entities.Target.Interfaces;
using Game.Level.Configurations.Stage;
using Game.Level.Data;

namespace Game.Level.Interfaces
{
    /// <summary>
    /// Interface for spawning level content.
    /// </summary>
    public interface ILevelSpawner
    {
        /// <summary>
        /// Clears the current level.
        /// </summary>
        void ClearLevel();

        /// <summary>
        /// Phase 1: Spawns targets for the specified level configuration (without attachments).
        /// </summary>
        List<ITargetView> SpawnTargets(LevelConfig config);

        /// <summary>
        /// Phase 2: Spawns and attaches elements to the spawned targets.
        /// </summary>
        void AttachElements(List<ITargetView> targets, LevelConfig config, StageConfig stageConfig);

        /// <summary>
        /// Phase 3: Finalizes the spawned targets (animations, etc.).
        /// </summary>
        void FinalizeSpawning(List<ITargetView> targets);
    }
}
