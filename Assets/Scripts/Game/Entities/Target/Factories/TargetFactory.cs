using System;
using System.Collections.Generic;
using Game.Entities.Collectible.Spawning;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Data;
using Game.Entities.Target.Views;
using Game.Pooling;
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
        private readonly KnifePoolProvider _KnifePoolProvider;

        /// <summary>
        /// Initializes a new instance of the TargetFactory.
        /// </summary>
        /// <param name="objectResolver">DI container.</param>
        /// <param name="rotationBehaviorFactory">Factory for rotation behaviors.</param>
        /// <param name="collectibleSpawner">Spawner for collectibles.</param>
        /// <param name="defaultPrefab">Default target prefab.</param>
        /// <param name="knifePoolProvider">Pool provider for knives.</param>
        public TargetFactory(
            IObjectResolver objectResolver,
            RotationBehaviorFactory rotationBehaviorFactory,
            CollectibleSpawner collectibleSpawner,
            TargetView defaultPrefab,
            KnifePoolProvider knifePoolProvider)
        {
            _ObjectResolver = objectResolver;
            _RotationBehaviorFactory = rotationBehaviorFactory;
            _CollectibleSpawner = collectibleSpawner;
            _DefaultPrefab = defaultPrefab;
            _KnifePoolProvider = knifePoolProvider;
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
                ? config.PrefabOverride 
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

            if (config.PreStuckKnives != null && config.PreStuckKnives.Count > 0)
            {
                SpawnPreStuckKnives(config.PreStuckKnives, instance);
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

        private void SpawnPreStuckKnives(IReadOnlyList<PreStuckKnifeConfig> preStuckConfigs, TargetView target)
        {
            var pooledKnives = new List<KnifeView>();
            var overrideKnives = new List<KnifeView>();

            for (int i = 0; i < preStuckConfigs.Count; i++)
            {
                var knifeConfig = preStuckConfigs[i];
                var knife = CreatePreStuckKnife(knifeConfig, pooledKnives, overrideKnives);
                if (knife == null)
                    continue;

                knife.StopMovement();
                target.PlacePreStuckKnife(knife, knifeConfig.Angle);
            }

            if (pooledKnives.Count == 0 && overrideKnives.Count == 0)
                return;

            target.OnDestroyed += _ =>
            {
                ReleasePooledKnives(pooledKnives);
                DestroyOverrideKnives(overrideKnives);
            };
        }

        private KnifeView CreatePreStuckKnife(PreStuckKnifeConfig knifeConfig, IList<KnifeView> pooledKnives, IList<KnifeView> overrideKnives)
        {
            if (knifeConfig == null)
                return null;

            if (knifeConfig.PrefabOverride != null)
            {
                var knife = UnityEngine.Object.Instantiate(knifeConfig.PrefabOverride);
                _ObjectResolver.Inject(knife);
                overrideKnives.Add(knife);
                return knife;
            }

            var pooledKnife = _KnifePoolProvider.Get();
            pooledKnives.Add(pooledKnife);
            return pooledKnife;
        }

        private void ReleasePooledKnives(IEnumerable<KnifeView> knives)
        {
            foreach (var knife in knives)
            {
                _KnifePoolProvider.Release(knife);
            }
        }

        private void DestroyOverrideKnives(IEnumerable<KnifeView> knives)
        {
            foreach (var knife in knives)
            {
                if (knife == null)
                    continue;

                UnityEngine.Object.Destroy(knife.gameObject);
            }
        }
    }
}
