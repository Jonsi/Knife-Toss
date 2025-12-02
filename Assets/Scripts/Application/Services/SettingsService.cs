using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using VContainer.Unity;

namespace KnifeToss.Application.Services
{
    public class SettingsService : ISettingsService, IStartable
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IMusicPlayer _musicPlayer;
        private readonly IAudioPlayer _audioPlayer;
        private AudioSettingsData _audioSettings;

        public SettingsService(ISettingsRepository settingsRepository, IMusicPlayer musicPlayer, IAudioPlayer audioPlayer)
        {
            _settingsRepository = settingsRepository;
            _musicPlayer = musicPlayer;
            _audioPlayer = audioPlayer;
        }

        public void Start()
        {
            _audioSettings = _settingsRepository.Load();
            ApplyMuteState();
        }

        public void ToggleMute()
        {
            _audioSettings.isMuted = !_audioSettings.isMuted;
            _settingsRepository.Save(_audioSettings);
            ApplyMuteState();
        }
        
        public bool IsMuted()
        {
            return _audioSettings.isMuted;
        }

        private void ApplyMuteState()
        {
            _musicPlayer.SetMuted(_audioSettings.isMuted);
            _audioPlayer.SetMuted(_audioSettings.isMuted);
        }
    }
}
