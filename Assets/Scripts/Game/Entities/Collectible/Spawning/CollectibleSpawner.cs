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
        public void SpawnCollectibles(TargetView target, List<CollectibleConfig> configs)
        {
            if (target == null || configs == null)
                return;

            foreach (var config in configs)
            {
                if (config.Prefab == null)
                    continue;

                SpawnSingleCollectible(target, config);
            }
        }

        private void SpawnSingleCollectible(TargetView target, CollectibleConfig config)
        {
            Vector3 position = CalculatePositionOnTarget(target, config);
            Quaternion rotation = CalculateRotationOnTarget(config);

            GameObject instanceObj = Object.Instantiate(config.Prefab, position, rotation, target.Transform);
            
            var view = instanceObj.GetComponent<CollectibleView>();
            if (view != null)
            {
                view.Initialize(config);
                _ObjectResolver.Inject(view);
            }
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

