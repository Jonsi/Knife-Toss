using UnityEngine;
using KnifeToss.Core.Abstractions;

namespace KnifeToss.Infrastructure.Audio
{
    /// <summary>
    /// Default implementation of IAudioPlayer using UnityEngine.AudioSource.
    /// </summary>
    public class UnityAudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioSource oneShotSource;

        public void Play(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        public void Play2D(AudioClip clip)
        {
            if (clip == null) return;
            oneShotSource?.PlayOneShot(clip);
        }

        public void Play2D(AudioClip clip, float volume)
        {
            if (clip == null) return;
            oneShotSource?.PlayOneShot(clip, volume);
        }

        public void SetMuted(bool isMuted)
        {
            AudioListener.pause = isMuted;
        }
    }
}

