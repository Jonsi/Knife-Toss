using UnityEngine;

namespace KnifeToss.Core.Abstractions
{
    public interface IAudioPlayer
    {
        void Play(AudioClip clip, Vector3 position, float volume);
        
        /// <summary>
        /// Plays a 2D sound at the default position (Vector3.zero) with full volume.
        /// Convenience method for UI sounds and non-spatial audio.
        /// </summary>
        void Play2D(AudioClip clip);
        
        /// <summary>
        /// Plays a 2D sound at the default position with specified volume.
        /// </summary>
        void Play2D(AudioClip clip, float volume);
    }
}
