using Core.Abstractions;
using Core.Data;
using UnityEngine;
using VContainer;

namespace Infrastructure.Audio
{
    /// <summary>
    /// Represents the MusicPlayer class.
    /// </summary>
    public class MusicPlayer : IMusicPlayer
    {
        private readonly AudioSource _audioSource;
        private readonly SoundSettings _soundSettings;

        /// <summary>
        /// Performs the MusicPlayer operation.
        /// </summary>
        [Inject]
        public MusicPlayer(SoundSettings soundSettings)
        {
            _soundSettings = soundSettings;
            var musicPlayerObject = new GameObject("MusicPlayer");
            Object.DontDestroyOnLoad(musicPlayerObject);
            _audioSource = musicPlayerObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.clip = _soundSettings.Music;
        }

        /// <summary>
        /// Performs the PlayMusic operation.
        /// </summary>
        public void PlayMusic()
        {
            if (_audioSource.isPlaying)
            {
                return;
            }
            _audioSource.Play();
        }

        /// <summary>
        /// Performs the StopMusic operation.
        /// </summary>
        public void StopMusic()
        {
            _audioSource.Stop();
        }

        /// <summary>
        /// Performs the SetMuted operation.
        /// </summary>
        public void SetMuted(bool isMuted)
        {
            _audioSource.mute = isMuted;
        }
    }
}
