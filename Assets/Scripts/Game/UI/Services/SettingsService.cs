using System;
using Game.Persistence.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.UI.Services
{
    /// <summary>
    /// Manages runtime UI settings such as audio mute state.
    /// Handles persistence and synchronization with underlying services.
    /// </summary>
    public class SettingsService : IInitializable
    {
        private readonly PlayerDataService _PlayerDataService;

        /// <summary>
        /// Event raised when mute state changes.
        /// </summary>
        public event Action<bool> OnMuteChanged;

        /// <summary>
        /// Gets a value indicating whether audio is muted.
        /// </summary>
        public bool IsMuted { get; private set; }

        [Inject]
        public SettingsService(PlayerDataService playerDataService)
        {
            _PlayerDataService = playerDataService;
        }

        public void Initialize()
        {
            LoadSettings();
        }

        /// <summary>
        /// Sets the mute state and applies it to the audio listener.
        /// </summary>
        /// <param name="isMuted">Requested mute state.</param>
        public void SetMuted(bool isMuted)
        {
            if (IsMuted == isMuted)
                return;

            IsMuted = isMuted;
            AudioListener.pause = IsMuted;
            OnMuteChanged?.Invoke(IsMuted);
            
            SaveSettings();
        }

        private void LoadSettings()
        {
            var settings = _PlayerDataService.Settings;
            
            SetMuted(settings.IsMuted);
        }

        private void SaveSettings()
        {
            var settings = _PlayerDataService.Settings;
            settings.IsMuted = IsMuted;
            
            _PlayerDataService.RequestSave();
        }
    }
}
