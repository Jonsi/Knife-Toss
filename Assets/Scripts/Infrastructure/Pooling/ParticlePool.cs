using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using KnifeToss.Core.Abstractions.Pooling;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Pool entry that maps a prefab to its pool.
    /// </summary>
    [Serializable]
    public class ParticlePoolEntry
    {
        public ParticleSystem Prefab;
        public int PoolSize = 5;
    }

    /// <summary>
    /// Pool for ParticleSystem components. Creates separate pools per prefab.
    /// </summary>
    public sealed class ParticlePool : ComponentPool<ParticleSystem>
    {
        private readonly ParticleSystem _prefab;

        /// <summary>
        /// Creates a new particle pool for a specific prefab.
        /// </summary>
        public ParticlePool(
            ParticleSystem prefab,
            Transform parent,
            int initialSize = 5,
            Action<ParticleSystem> onGet = null,
            Action<ParticleSystem> onRelease = null)
            : base(
                factory: () => CreateParticleSystem(prefab, parent),
                parent: parent,
                onGet: onGet ?? ResetParticleSystem,
                onRelease: onRelease ?? ResetParticleSystem)
        {
            _prefab = prefab;
            Warmup(initialSize);
        }

        private static ParticleSystem CreateParticleSystem(ParticleSystem prefab, Transform parent)
        {
            ParticleSystem instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static void ResetParticleSystem(ParticleSystem system)
        {
            if (system == null) return;

            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Clear();

            // Reset position and rotation
            system.transform.localPosition = Vector3.zero;
            system.transform.localRotation = Quaternion.identity;
            system.transform.localScale = Vector3.one;
        }
    }
}

