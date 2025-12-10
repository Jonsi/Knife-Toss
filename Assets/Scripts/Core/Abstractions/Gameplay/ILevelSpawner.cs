using System.Collections.Generic;
using KnifeToss.Core.Abstractions.Gameplay.Target;
using KnifeToss.Core.Data.Level;

namespace KnifeToss.Core.Abstractions.Gameplay
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
        /// Spawns objects for the specified level configuration.
        /// </summary>
        /// <param name="config">The level configuration to spawn.</param>
        /// <returns>A list of spawned target views.</returns>
        List<ITargetView> SpawnLevel(LevelConfig config);
    }
}

