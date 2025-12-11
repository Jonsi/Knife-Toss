using System;
using Game.Entities.Collectible.Spawning;
using Game.Entities.Target.Data;
using Game.Entities.Target.Views;
using UnityEngine;
using VContainer;

namespace Game.Entities.Target.Factories
{
    /// <summary>
    /// Factory responsible for instantiating and configuring target objects.
    /// </summary>
    public class TargetFactory
    {
        private readonly IObjectResolver _ObjectResolver;
        private readonly RotationBehaviorFactory _RotationBehaviorFactory;
        private readonly CollectibleSpawner _CollectibleSpawner;
        private readonly TargetView _DefaultPrefab;

        /// <summary>
        /// Initializes a new instance of the TargetFactory.
        /// </summary>
        /// <param name="objectResolver">DI container.</param>
        /// <param name="rotationBehaviorFactory">Factory for rotation behaviors.</param>
        /// <param name="collectibleSpawner">Spawner for collectibles.</param>
        /// <param name="defaultPrefab">Default target prefab.</param>
        public TargetFactory(
            IObjectResolver objectResolver,
            RotationBehaviorFactory rotationBehaviorFactory,
            CollectibleSpawner collectibleSpawner,
            TargetView defaultPrefab)
        {
            _ObjectResolver = objectResolver;
            _RotationBehaviorFactory = rotationBehaviorFactory;
            _CollectibleSpawner = collectibleSpawner;
            _DefaultPrefab = defaultPrefab;
        }

        /// <summary>
        /// Creates a target instance at the specified orbit position.
        /// </summary>
        /// <param name="config">Configuration for the target.</param>
        /// <param name="parent">Parent transform for the target.</param>
        /// <returns>The instantiated TargetView.</returns>
        public TargetView Create(TargetConfig config, Transform parent)
        {
            TargetView prefab = config.PrefabOverride != null 
                ? config.PrefabOverride.GetComponent<TargetView>() 
                : _DefaultPrefab;

            if (prefab == null)
            {
                if (_DefaultPrefab == null)
                    throw new InvalidOperationException("No target prefab available for creation.");
                prefab = _DefaultPrefab;
            }

            Vector3 position = CalculateOrbitPosition(config.OrbitRadius, config.StartingAngle);
            
            TargetView instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, parent);
            
            instance.transform.localScale = Vector3.one * config.BaseScale;

            var rotationBehavior = _RotationBehaviorFactory.Create(config);
            instance.SetRotationBehavior(rotationBehavior);

            instance.Initialize(config);
            
            _ObjectResolver.Inject(instance);

            if (config.Collectibles != null && config.Collectibles.Count > 0)
            {
                _CollectibleSpawner.SpawnCollectibles(instance, config.Collectibles);
            }

            return instance;
        }

        private Vector3 CalculateOrbitPosition(float radius, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Cos(angleRadians) * radius,
                Mathf.Sin(angleRadians) * radius,
                0f
            );
        }
    }
}
