using System.Collections.Generic;
using UnityEngine;
using KnifeToss.Presentation.Gameplay.Target;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Abstractions.Gameplay.Target;
using KnifeToss.Core.Data.Level;
using System.Linq;

namespace KnifeToss.Presentation.Gameplay.Leveling
{
    /// <summary>
    /// Manages spawning and clearing of level objects.
    /// </summary>
    public class LevelSpawner : ILevelSpawner
    {
        private readonly TargetFactory _targetFactory;
        private readonly Transform _levelRoot;

        public LevelSpawner(TargetFactory targetFactory, Transform levelRoot)
        {
            _targetFactory = targetFactory;
            _levelRoot = levelRoot;
        }

        /// <summary>
        /// Destroys all objects under the level root.
        /// </summary>
        public void ClearLevel()
        {
            foreach (Transform child in _levelRoot)
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
                var target = _targetFactory.Create(targetConfig, _levelRoot);
                spawnedTargets.Add(target);
            }

            return spawnedTargets;
        }
    }
}

