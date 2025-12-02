using UnityEngine;

namespace KnifeToss.Core.Abstractions
{
    public interface IAudioPlayer
    {
        void Play(AudioClip clip, Vector3 position, float volume);
        void Play(SoundEffect sound, Vector3 position, float volume);
    }
}
