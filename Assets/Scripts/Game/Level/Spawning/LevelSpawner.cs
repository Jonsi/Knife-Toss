using System.Collections.Generic;
using Game.Level.Data;
using Game.Level.Interfaces;
using Game.Target.Factories;
using Game.Target.Interfaces;
using UnityEngine;

namespace Game.Level.Spawning
{
    /// <summary>
    /// Manages spawning and clearing of level objects.
    /// </summary>
    public class LevelSpawner : ILevelSpawner
    {
        private readonly TargetFactory _TargetFactory;
        private readonly Transform _LevelRoot;

        public LevelSpawner(TargetFactory targetFactory, Transform levelRoot)
        {
            _TargetFactory = targetFactory;
            _LevelRoot = levelRoot;
        }

        /// <summary>
        /// Destroys all objects under the level root.
        /// </summary>
        public void ClearLevel()
        {
            foreach (Transform child in _LevelRoot)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Spawns all targets defined in the level configuration.
        /// </summary>
        /// <param name="config">The level configuration.</param>
        /// <returns>A list of spawned TargetView instances.</returns>
        public List<ITargetView> SpawnLevel(LevelConfig config)
        {
            ClearLevel();

            var spawnedTargets = new List<ITargetView>();
            foreach (var targetConfig in config.Targets)
            {
                var target = _TargetFactory.Create(targetConfig, _LevelRoot);
                spawnedTargets.Add(target);
            }

            return spawnedTargets;
        }
    }
}

