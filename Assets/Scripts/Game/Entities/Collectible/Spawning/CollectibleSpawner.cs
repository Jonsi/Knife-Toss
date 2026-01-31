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
        /// Spawns collectibles attached to the target. Prefab and sprite come from each config's CollectibleCurrencyConfig.
        /// </summary>
        /// <param name="target">The target view to attach collectibles to.</param>
        /// <param name="configs">List of collectible configurations (type + placement).</param>
        public void SpawnCollectibles(TargetView target, List<CollectibleConfig> configs)
        {
            if (target == null || configs == null)
                return;

            foreach (var config in configs)
            {
                CollectibleView prefab = config.CollectibleCurrency?.Prefab;
                if (prefab == null)
                    continue;

                CollectibleView view = Object.Instantiate(prefab);
                view.Initialize(config);
                _ObjectResolver.Inject(view);
                _CollectibleRegistry?.Register(view);

                target.PlaceStickable(view, config.AngleOffset);
            }
        }
    }
}

