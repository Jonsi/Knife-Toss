using UnityEngine;

namespace KnifeToss.Core.Abstractions
{
    public interface IAudioPlayer
    {
        void Play(AudioClip clip, Vector3 position, float volume);





        void Play2D(AudioClip clip);




        void Play2D(AudioClip clip, float volume);
    }
}
