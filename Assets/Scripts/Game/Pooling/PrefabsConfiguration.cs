using System;
using UnityEngine;
using Game.PowerUps.Configs;
using Game.Configs.Prefabs;
using Game.Entities.Target.Views;
using Game.Entities.Collectible.Views;
using Game.Entities.Knife.Views;

namespace Game.Pooling
{
    /// <summary>
    /// Centralized registry of prefabs and their pooling settings.
    /// Uses explicit lists per entry type to keep inspector workflows simple.
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabsConfiguration", menuName = "Knife Toss/Prefabs/Prefabs Configuration")]
    public class PrefabsConfiguration : ScriptableObject
    {
        [Header("Knives")]
        [SerializeField] private FactoryPrefabEntry<KnifeView> _DefaultKnife;
        [SerializeField] private FactoryPrefabEntry<KnifeView> _SuperKnife;

        [Header("Targets")]
        [SerializeField] private FactoryPrefabEntry<TargetView> _Target;

        [Header("Collectibles")]
        [SerializeField] private FactoryPrefabEntry<CollectibleView> _Collectible;

        [Header("Particles")]
        [SerializeField] private FactoryPrefabEntry<ParticleSystem> _KnifeHitParticles;
        [SerializeField] private FactoryPrefabEntry<ParticleSystem> _CollectibleHitParticles;

        public FactoryPrefabEntry<KnifeView> GetDefaultKnifeEntry() => _DefaultKnife;
        public FactoryPrefabEntry<KnifeView> GetSuperKnifeEntry() => _SuperKnife != null ? _SuperKnife : _DefaultKnife;
        public FactoryPrefabEntry<TargetView> GetTargetEntry() => _Target;
        public FactoryPrefabEntry<CollectibleView> GetCollectibleEntry() => _Collectible;

        public FactoryPrefabEntry<KnifeView> GetKnifeForPowerUp(PowerUpConfigBase config)
        {
            if (config is SuperKnifeConfig)
                return GetSuperKnifeEntry();

            return GetDefaultKnifeEntry();
        }

        public FactoryPrefabEntry<ParticleSystem> GetKnifeHitParticles() => _KnifeHitParticles;
        public FactoryPrefabEntry<ParticleSystem> GetCollectibleHitParticles() => _CollectibleHitParticles;

        public TargetView GetTargetPrefab() => _Target?.Prefab;
        public CollectibleView GetCollectiblePrefab() => _Collectible?.Prefab;
        public KnifeView GetDefaultKnifePrefab() => _DefaultKnife?.Prefab;
        public KnifeView GetSuperKnifePrefab() => _SuperKnife?.Prefab ?? GetDefaultKnifePrefab();
    }
}
