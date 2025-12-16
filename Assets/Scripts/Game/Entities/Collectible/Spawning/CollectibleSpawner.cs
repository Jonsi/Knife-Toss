using System.Collections.Generic;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Views;
using Game.Entities.Target.Views;
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

        /// <summary>
        /// Initializes a new instance of the CollectibleSpawner.
        /// </summary>
        /// <param name="objectResolver">Dependency injection resolver.</param>
        public CollectibleSpawner(IObjectResolver objectResolver)
        {
            _ObjectResolver = objectResolver;
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
            Vector3 position = CalculatePositionOnTarget(target, config);
            Quaternion rotation = CalculateRotationOnTarget(config);

            CollectibleView view = Object.Instantiate(defaultPrefab, position, rotation, target.Transform);
            view.Initialize(config);
            _ObjectResolver.Inject(view);
        }

        private Vector3 CalculatePositionOnTarget(TargetView target, CollectibleConfig config)
        {
            float angleRad = config.AngleOffset * Mathf.Deg2Rad;
            float parentScale = Mathf.Max(target.transform.lossyScale.x, target.transform.lossyScale.y);
            parentScale = parentScale > 0.001f ? parentScale : 1f;
            
            float globalDistance = target.Radius + config.RadiusOffset;
            
            float localDistance = globalDistance / parentScale;

            return new Vector3(
                Mathf.Cos(angleRad) * localDistance,
                Mathf.Sin(angleRad) * localDistance,
                0f
            );
        }

        private Quaternion CalculateRotationOnTarget(CollectibleConfig config)
        {
            return Quaternion.Euler(0f, 0f, config.AngleOffset - 90f);
        }
    }
}

