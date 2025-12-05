using Core.Abstractions;
using Core.Data;
using VContainer.Unity;

namespace App.Services
{
    /// <summary>
    /// Represents the SettingsService class.
    /// </summary>
    public class SettingsService : ISettingsService, IStartable
    {
        private readonly IPlayerDataService playerDataService;
        private readonly IMusicPlayer _musicPlayer;
        private readonly IAudioPlayer _audioPlayer;
        private AudioSettingsData _audioSettings;

        /// <summary>
        /// Performs the SettingsService operation.
        /// </summary>
        public SettingsService(IPlayerDataService playerDataService, IMusicPlayer musicPlayer, IAudioPlayer audioPlayer)
        {
            this.playerDataService = playerDataService;
            _musicPlayer = musicPlayer;
            _audioPlayer = audioPlayer;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            _audioSettings = playerDataService.Settings.audio;
            ApplyMuteState();
        }

        /// <summary>
        /// Performs the ToggleMute operation.
        /// </summary>
        public void ToggleMute()
        {
            _audioSettings.isMuted = !_audioSettings.isMuted;
            playerDataService.Save();
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
