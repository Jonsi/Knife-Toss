using UnityEngine;

namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Defines the IAudioPlayer interface.
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// Performs the Play operation.
        /// </summary>
        void Play(AudioClip clip, Vector3 position, float volume);





        /// <summary>
        /// Performs the Play2D operation.
        /// </summary>
        void Play2D(AudioClip clip);




        /// <summary>
        /// Performs the Play2D operation.
        /// </summary>
        void Play2D(AudioClip clip, float volume);

        /// <summary>
        /// Performs the SetMuted operation.
        /// </summary>
        void SetMuted(bool isMuted);
    }
}
