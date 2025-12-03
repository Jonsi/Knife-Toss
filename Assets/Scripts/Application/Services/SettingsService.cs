using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using VContainer.Unity;

namespace KnifeToss.Application.Services
{
    /// <summary>
    /// Represents the SettingsService class.
    /// </summary>
    public class SettingsService : ISettingsService, IStartable
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IMusicPlayer _musicPlayer;
        private readonly IAudioPlayer _audioPlayer;
        private AudioSettingsData _audioSettings;

        /// <summary>
        /// Performs the SettingsService operation.
        /// </summary>
        public SettingsService(ISettingsRepository settingsRepository, IMusicPlayer musicPlayer, IAudioPlayer audioPlayer)
        {
            _settingsRepository = settingsRepository;
            _musicPlayer = musicPlayer;
            _audioPlayer = audioPlayer;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            _audioSettings = _settingsRepository.Load();
            ApplyMuteState();
        }

        /// <summary>
        /// Performs the ToggleMute operation.
        /// </summary>
        public void ToggleMute()
        {
            _audioSettings.isMuted = !_audioSettings.isMuted;
            _settingsRepository.Save(_audioSettings);
            ApplyMuteState();
        }
        
        /// <summary>
        /// Performs the IsMuted operation.
        /// </summary>
        public bool IsMuted()
        {
            return _audioSettings.isMuted;
        }

        /// <summary>
        /// Performs the ApplyMuteState operation.
        /// </summary>
        private void ApplyMuteState()
        {
            _musicPlayer.SetMuted(_audioSettings.isMuted);
            _audioPlayer.SetMuted(_audioSettings.isMuted);
        }
    }
}
