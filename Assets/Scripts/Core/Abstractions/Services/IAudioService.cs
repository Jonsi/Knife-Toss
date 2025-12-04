using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KnifeToss.Core.Abstractions.Services
{
    /// <summary>
    /// Handle for a playing audio source. Used to track and control individual audio instances.
    /// </summary>
    public readonly struct AudioHandle
    {
        public readonly AudioSource Source;
        public readonly bool IsValid;

        public AudioHandle(AudioSource source)
        {
            Source = source;
            IsValid = source != null;
        }

        public void Stop()
        {
            if (IsValid && Source != null)
            {
                Source.Stop();
            }
        }

        public bool IsPlaying => IsValid && Source != null && Source.isPlaying;
    }

    /// <summary>
    /// Service for playing audio using pooled AudioSources.
    /// Replaces the old AudioPlayer with a pooled system to reduce GC allocations.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Plays a 2D audio clip using a pooled AudioSource.
        /// </summary>
        AudioHandle Play2D(AudioClip clip, float volume = 1f);

        /// <summary>
        /// Plays a 2D audio clip asynchronously using a pooled AudioSource.
        /// Returns when the clip finishes playing.
        /// </summary>
        UniTask<AudioHandle> Play2DAsync(AudioClip clip, float volume = 1f);

        /// <summary>
        /// Plays a 3D audio clip at a specific position using a pooled AudioSource.
        /// </summary>
        AudioHandle PlayAt(AudioClip clip, Vector3 position, float volume = 1f);

        /// <summary>
        /// Plays a 3D audio clip asynchronously at a specific position.
        /// Returns when the clip finishes playing.
        /// </summary>
        UniTask<AudioHandle> PlayAtAsync(AudioClip clip, Vector3 position, float volume = 1f);

        /// <summary>
        /// Stops all currently playing audio sources and returns them to the pool.
        /// </summary>
        void StopAll();

        /// <summary>
        /// Sets the global mute state for all audio.
        /// </summary>
        void SetMuted(bool isMuted);
    }
}

