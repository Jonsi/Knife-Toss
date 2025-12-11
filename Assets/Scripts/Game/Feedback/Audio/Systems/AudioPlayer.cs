using Game.Feedback.Audio.Interfaces;
using UnityEngine;

namespace Game.Feedback.Audio.Systems
{
    /// <summary>
    /// Plays one-shot sound effects through a configured audio source.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioSource _AudioSource;

        /// <summary>
        /// Plays the provided audio clip once using the configured output settings.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="volume">Volume multiplier.</param>
        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (_AudioSource == null || clip == null)
            {
                return;
            }

            _AudioSource.PlayOneShot(clip, volume);
        }
    }
}

