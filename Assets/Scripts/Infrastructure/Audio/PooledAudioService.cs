using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using KnifeToss.Core.Abstractions.Services;
using KnifeToss.Infrastructure.Pooling;

namespace KnifeToss.Infrastructure.Audio
{
    /// <summary>
    /// Pooled audio service that uses AudioSourcePool to reduce GC allocations.
    /// Replaces the old AudioPlayer implementation.
    /// </summary>
    public sealed class PooledAudioService : IAudioService, System.IDisposable
    {
        private readonly AudioSourcePool _pool;
        private readonly Transform _poolRoot;
        private readonly List<AudioSource> _activeSources = new List<AudioSource>();
        private bool _isMuted;

        /// <summary>
        /// Creates a new pooled audio service.
        /// </summary>
        public PooledAudioService(Transform poolRoot, int poolSize = 8)
        {
            if (poolRoot == null)
            {
                GameObject rootObject = new GameObject("AudioPoolRoot");
                Object.DontDestroyOnLoad(rootObject);
                _poolRoot = rootObject.transform;
            }
            else
            {
                _poolRoot = poolRoot;
            }

            _pool = new AudioSourcePool(_poolRoot, poolSize);
        }

        public AudioHandle Play2D(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return new AudioHandle(null);
            }

            AudioSource source = _pool.Get();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f; // 2D
            source.mute = _isMuted;
            source.Play();

            _activeSources.Add(source);
            return new AudioHandle(source);
        }

        public async UniTask<AudioHandle> Play2DAsync(AudioClip clip, float volume = 1f)
        {
            var handle = Play2D(clip, volume);
            
            if (!handle.IsValid)
            {
                return handle;
            }

            // Wait for clip to finish playing
            await UniTask.WaitWhile(() => handle.IsPlaying);

            // Return to pool
            ReturnToPool(handle.Source);
            return handle;
        }

        public AudioHandle PlayAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null)
            {
                return new AudioHandle(null);
            }

            AudioSource source = _pool.Get();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 1f; // 3D
            source.mute = _isMuted;
            source.Play();

            _activeSources.Add(source);
            return new AudioHandle(source);
        }

        public async UniTask<AudioHandle> PlayAtAsync(AudioClip clip, Vector3 position, float volume = 1f)
        {
            var handle = PlayAt(clip, position, volume);

            if (!handle.IsValid)
            {
                return handle;
            }

            // Wait for clip to finish playing
            await UniTask.WaitWhile(() => handle.IsPlaying);

            // Return to pool
            ReturnToPool(handle.Source);
            return handle;
        }

        public void StopAll()
        {
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = _activeSources[i];
                if (source != null)
                {
                    source.Stop();
                    ReturnToPool(source);
                }
            }

            _activeSources.Clear();
        }

        public void SetMuted(bool isMuted)
        {
            _isMuted = isMuted;

            foreach (AudioSource source in _activeSources)
            {
                if (source != null)
                {
                    source.mute = isMuted;
                }
            }
        }

        private void ReturnToPool(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            _activeSources.Remove(source);
            _pool.Release(source);
        }

        public void Dispose()
        {
            StopAll();
        }
    }
}

