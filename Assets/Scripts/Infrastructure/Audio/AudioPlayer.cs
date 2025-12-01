using KnifeToss.Core.Abstractions;
using UnityEngine;

namespace KnifeToss.Infrastructure.Audio
{
    public sealed class AudioPlayer : IAudioPlayer
    {
        public void Play(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
