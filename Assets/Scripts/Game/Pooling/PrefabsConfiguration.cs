using System;
using UnityEngine;
using Game.PowerUps.Configs;
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
        [SerializeField] private KnifePrefabEntry _DefaultKnife;
        [SerializeField] private KnifePrefabEntry _SuperKnife;

        [Header("Targets")]
        [SerializeField] private PrefabEntry _Target;

        [Header("Collectibles")]
        [SerializeField] private PrefabEntry _Collectible;

        [Header("Particles")]
        [SerializeField] private ParticlePrefabEntry _KnifeHitParticles;
        [SerializeField] private ParticlePrefabEntry _CollectibleHitParticles;

        public KnifePrefabEntry GetDefaultKnifeEntry() => _DefaultKnife;
        public KnifePrefabEntry GetSuperKnifeEntry() => _SuperKnife != null ? _SuperKnife : _DefaultKnife;
        public PrefabEntry GetTargetEntry() => _Target;
        public PrefabEntry GetCollectibleEntry() => _Collectible;

        public KnifePrefabEntry GetKnifeForPowerUp(PowerUpConfigBase config)
        {
            if (config is SuperKnifeConfig)
                return GetSuperKnifeEntry();

            return GetDefaultKnifeEntry();
        }

        public ParticlePrefabEntry GetKnifeHitParticles() => _KnifeHitParticles;
        public ParticlePrefabEntry GetCollectibleHitParticles() => _CollectibleHitParticles;

        public TargetView GetTargetPrefab() => _Target?.Prefab != null ? _Target.Prefab.GetComponent<TargetView>() : null;
        public CollectibleView GetCollectiblePrefab() => _Collectible?.Prefab != null ? _Collectible.Prefab.GetComponent<CollectibleView>() : null;
        public KnifeView GetDefaultKnifePrefab() => _DefaultKnife?.GetKnifePrefab();
        public KnifeView GetSuperKnifePrefab() => _SuperKnife?.GetKnifePrefab() ?? GetDefaultKnifePrefab();
    }
}
