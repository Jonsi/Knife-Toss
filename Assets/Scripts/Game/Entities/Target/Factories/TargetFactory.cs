using System;
using System.Collections.Generic;
using Game.Entities.Collectible.Spawning;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Data;
using Game.Entities.Target.Views;
using Game.Level.Data;
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
        private readonly TargetBehaviorFactory _TargetBehaviorFactory;
        private readonly CollectibleSpawner _CollectibleSpawner;
        private readonly TargetView _DefaultPrefab;

        /// <summary>
        /// Initializes a new instance of the TargetFactory.
        /// </summary>
        /// <param name="objectResolver">DI container.</param>
        /// <param name="targetBehaviorFactory">Factory for behavior collections.</param>
        /// <param name="collectibleSpawner">Spawner for collectibles.</param>
        /// <param name="defaultPrefab">Default target prefab.</param>
        public TargetFactory(
            IObjectResolver objectResolver,
            TargetBehaviorFactory targetBehaviorFactory,
            CollectibleSpawner collectibleSpawner,
            TargetView defaultPrefab)
        {
            _ObjectResolver = objectResolver;
            _TargetBehaviorFactory = targetBehaviorFactory;
            _CollectibleSpawner = collectibleSpawner;
            _DefaultPrefab = defaultPrefab;
        }

        /// <summary>
        /// Phase 1: Instantiates and initializes the target view.
        /// </summary>
        public TargetView SpawnTarget(TargetConfig config, Transform parent)
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

            TargetView instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.transform.localPosition = config.InitialOffset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * config.BaseScale;
            instance.SetInitialWorldPosition(instance.transform.position);

            var behaviors = _TargetBehaviorFactory.Create(config);
            instance.ConfigureBehaviors(behaviors);

            instance.Initialize(config);

            _ObjectResolver.Inject(instance);

            return instance;
        }

        /// <summary>
        /// Phase 2: Spawns and attaches elements (knives, collectibles) to the target.
        /// </summary>
        public void AttachElements(TargetView instance, TargetConfig config, StageConfig stageConfig)
        {
            var stageKnifePrefab = stageConfig?.DefaultKnifePrefab;
            var stageCollectiblePrefab = stageConfig?.DefaultCollectiblePrefab;

            if (stageCollectiblePrefab != null && config.Collectibles != null && config.Collectibles.Count > 0)
            {
                _CollectibleSpawner.SpawnCollectibles(instance, config.Collectibles, stageCollectiblePrefab);
            }

            if (stageKnifePrefab != null && config.PreStuckKnives != null && config.PreStuckKnives.Count > 0)
            {
                SpawnPreStuckKnives(config.PreStuckKnives, instance, stageKnifePrefab);
            }
        }

        /// <summary>
        /// Phase 3: Finalizes the target (animations, etc).
        /// </summary>
        public void FinalizeTarget(TargetView instance)
        {
            instance.PlaySpawnAnimation();
        }

        private void SpawnPreStuckKnives(
            IReadOnlyList<PreStuckKnifeConfig> preStuckConfigs,
            TargetView target,
            KnifeView knifePrefab)
        {
            if (knifePrefab == null)
                return;

            for (int i = 0; i < preStuckConfigs.Count; i++)
            {
                var knifeConfig = preStuckConfigs[i];
                var knife = CreatePreStuckKnife(knifePrefab);
                if (knife == null)
                    continue;

                knife.StopMovement();
                target.PlaceStickable(knife, knifeConfig.Angle);
            }
        }

        private KnifeView CreatePreStuckKnife(KnifeView knifePrefab)
        {
            if (knifePrefab == null)
                return null;

            var knife = UnityEngine.Object.Instantiate(knifePrefab);
            _ObjectResolver.Inject(knife);
            return knife;
        }
    }
}
