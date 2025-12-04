using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using KnifeToss.Core.Abstractions.Pooling;

namespace KnifeToss.Infrastructure.Pooling
{
    /// <summary>
    /// Pool for AudioSource components. Optimized for one-shot audio playback.
    /// </summary>
    public sealed class AudioSourcePool : ComponentPool<AudioSource>
    {
        private readonly Transform _poolRoot;
        private readonly AudioSource _prefab;

        /// <summary>
        /// Creates a new AudioSource pool.
        /// </summary>
        public AudioSourcePool(
            Transform poolRoot,
            int initialSize = 8,
            Action<AudioSource> onGet = null,
            Action<AudioSource> onRelease = null)
            : base(
                factory: () => CreateAudioSource(poolRoot),
                parent: poolRoot,
                onGet: onGet ?? ConfigureForPlayback,
                onRelease: onRelease ?? ResetAudioSource)
        {
            _poolRoot = poolRoot;
            Warmup(initialSize);
        }

        private static AudioSource CreateAudioSource(Transform parent)
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.SetParent(parent, false);
            AudioSource source = go.AddComponent<AudioSource>();

            // Configure for 2D one-shot playback
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // 2D
            source.dopplerLevel = 0f;
            source.priority = 0;
            source.spatialize = false;
            source.volume = 1f;

            go.SetActive(false);
            return source;
        }

        private static void ConfigureForPlayback(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
        }

        private static void ResetAudioSource(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
            source.volume = 1f;
        }
    }
}

