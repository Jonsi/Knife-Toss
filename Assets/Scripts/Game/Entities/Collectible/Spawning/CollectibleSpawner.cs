using System.Collections.Generic;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Views;
using Game.Entities.Target.Views;
using Game.PowerUps.Services;
using UnityEngine;
using VContainer;

namespace Game.Entities.Collectible.Spawning
{
    /// <summary>
    /// Responsible for spawning and positioning collectibles on targets.
    /// </summary>
    public class CollectibleSpawner
    {
        private readonly IObjectResolver _ObjectResolver;
        private readonly CollectibleRegistry _CollectibleRegistry;

        /// <summary>
        /// Initializes a new instance of the CollectibleSpawner.
        /// </summary>
        /// <param name="objectResolver">Dependency injection resolver.</param>
        public CollectibleSpawner(
            IObjectResolver objectResolver,
            Game.PowerUps.Services.CollectibleRegistry collectibleRegistry)
        {
            _ObjectResolver = objectResolver;
            _CollectibleRegistry = collectibleRegistry;
        }

        /// <summary>
        /// Spawns collectibles attached to the target based on the provided configurations.
        /// </summary>
        /// <param name="target">The target view to attach collectibles to.</param>
        /// <param name="configs">List of collectible configurations.</param>
        /// <param name="defaultPrefab">The prefab to instantiate for each collectible.</param>
        public void SpawnCollectibles(TargetView target, List<CollectibleConfig> configs, CollectibleView defaultPrefab)
        {
            if (target == null || configs == null || defaultPrefab == null)
                return;

            foreach (var config in configs)
            {
                SpawnSingleCollectible(target, config, defaultPrefab);
            }
        }

        private void SpawnSingleCollectible(TargetView target, CollectibleConfig config, CollectibleView defaultPrefab)
        {
            CollectibleView view = Object.Instantiate(defaultPrefab);
            view.Initialize(config);
            _ObjectResolver.Inject(view);
            _CollectibleRegistry?.Register(view);
            
            target.PlaceStickable(view, config.AngleOffset);
        }
    }
}

