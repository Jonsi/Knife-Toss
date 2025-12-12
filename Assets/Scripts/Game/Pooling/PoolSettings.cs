using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Configuration settings for object pooling systems.
    /// Contains prefabs and pool parameters for all pooled objects in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolSettings", menuName = "Knife Toss/Pooling/Pool Settings")]
    public class PoolSettings : ScriptableObject
    {
        [Header("Knife Pool")]
        [Tooltip("Prefab for knife objects (must have KnifeView component)")]
        public KnifeView KnifePrefab;

        [Tooltip("Initial number of knives to create in the pool")]
        public int KnifePoolWarmupCount = 5;

        [Tooltip("Maximum number of knives that can be pooled")]
        public int KnifePoolMaxSize = 20;

        [Header("Particle Pools")]
        [Tooltip("Prefab for knife hit target particles")]
        public ParticleSystem KnifeHitTargetParticlesPrefab;

        [Tooltip("Initial number of particle systems to create in the pool")]
        public int ParticlePoolWarmupCount = 3;

        [Tooltip("Maximum number of particle systems that can be pooled")]
        public int ParticlePoolMaxSize = 10;

        [Tooltip("Prefab for collectible hit particles")]
        public ParticleSystem CollectibleHitParticlesPrefab;
    }
}