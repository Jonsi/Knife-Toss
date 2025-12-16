using System.Collections.Generic;
using Game.Entities.Target.Factories;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Views;
using Game.Level.Data;
using Game.Level.Interfaces;
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
                Object.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Phase 1: Spawns targets for the specified level configuration (without attachments).
        /// </summary>
        public List<ITargetView> SpawnTargets(LevelConfig config)
        {
            ClearLevel();

            var spawnedTargets = new List<ITargetView>();
            foreach (var targetConfig in config.Targets)
            {
                var target = _TargetFactory.SpawnTarget(targetConfig, _LevelRoot);
                spawnedTargets.Add(target);
            }

            return spawnedTargets;
        }

        /// <summary>
        /// Phase 2: Spawns and attaches elements to the spawned targets.
        /// </summary>
        public void AttachElements(List<ITargetView> targets, LevelConfig config, StageConfig stageConfig)
        {
            if (targets == null || config == null || targets.Count != config.Targets.Count)
            {
                Debug.LogError("Target count mismatch or null arguments in AttachElements");
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] is TargetView targetView)
                {
                    _TargetFactory.AttachElements(targetView, config.Targets[i], stageConfig);
                }
            }
        }

        /// <summary>
        /// Phase 3: Finalizes the spawned targets (animations, etc.).
        /// </summary>
        public void FinalizeSpawning(List<ITargetView> targets)
        {
            if (targets == null)
                return;

            foreach (var target in targets)
            {
                if (target is TargetView targetView)
                {
                    _TargetFactory.FinalizeTarget(targetView);
                }
            }
        }
    }
}
