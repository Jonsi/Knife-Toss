using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KnifeToss.Core.Abstractions.Services
{
    /// <summary>
    /// Handle for a playing particle system. Used to track and control individual VFX instances.
    /// </summary>
    public readonly struct VFXHandle
    {
        public readonly ParticleSystem System;
        public readonly bool IsValid;

        public VFXHandle(ParticleSystem system)
        {
            System = system;
            IsValid = system != null;
        }

        public void Stop()
        {
            if (IsValid && System != null)
            {
                System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        public bool IsPlaying => IsValid && System != null && System.isPlaying;
    }

    /// <summary>
    /// Service for spawning particle effects using pooled ParticleSystems.
    /// Reduces GC allocations by reusing particle systems instead of Instantiate/Destroy.
    /// </summary>
    public interface IVFXService
    {
        /// <summary>
        /// Spawns a particle effect at the specified position and rotation using a pooled ParticleSystem.
        /// </summary>
        VFXHandle Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Spawns a particle effect asynchronously. Returns when the particle system finishes playing.
        /// </summary>
        UniTask<VFXHandle> SpawnAsync(ParticleSystem prefab, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Releases a particle effect back to the pool.
        /// </summary>
        void Release(VFXHandle handle);

        /// <summary>
        /// Stops all active particle effects and returns them to the pool.
        /// </summary>
        void StopAll();
    }
}

