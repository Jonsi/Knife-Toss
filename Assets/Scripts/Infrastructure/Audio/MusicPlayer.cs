using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using UnityEngine;
using VContainer;

namespace KnifeToss.Infrastructure.Audio
{
    public class MusicPlayer : IMusicPlayer
    {
        private readonly AudioSource _audioSource;
        private readonly SoundSettings _soundSettings;

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

        public void PlayMusic()
        {
            if (_audioSource.isPlaying)
            {
                return;
            }
            _audioSource.Play();
        }

        public void StopMusic()
        {
            _audioSource.Stop();
        }

        public void SetMuted(bool isMuted)
        {
            _audioSource.mute = isMuted;
        }
    }
}
