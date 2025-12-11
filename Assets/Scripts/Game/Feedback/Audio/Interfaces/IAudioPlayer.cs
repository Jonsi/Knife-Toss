using UnityEngine;

namespace Game.Feedback.Audio.Interfaces
{
    /// <summary>
    /// Plays sound effects through a configured audio source.
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// Plays the provided audio clip once using the configured output settings.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="volume">Volume multiplier.</param>
        void PlayOneShot(AudioClip clip, float volume = 1f);
    }
}

