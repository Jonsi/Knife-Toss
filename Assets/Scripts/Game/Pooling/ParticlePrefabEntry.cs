using System;
using UnityEngine;

namespace Game.Pooling
{
    public enum ParticlePrefabType
    {
        KnifeHitTarget,
        CollectibleHit
    }

    /// <summary>
    /// Prefab entry for pooled particle systems.
    /// </summary>
    [Serializable]
    public class ParticlePrefabEntry : PrefabEntry
    {
        [SerializeField] private ParticlePrefabType _Type;

        public ParticlePrefabType Type => _Type;
        public ParticleSystem GetParticlePrefab() => Prefab != null ? Prefab.GetComponent<ParticleSystem>() : null;
    }
}
