using System;
using UnityEngine;

namespace Game.UI.Services
{
    /// <summary>
    /// Manages runtime UI settings such as audio mute state.
    /// </summary>
    public class SettingsService
    {
        /// <summary>
        /// Event raised when mute state changes.
        /// </summary>
        public event Action<bool> OnMuteChanged;

        /// <summary>
        /// Gets a value indicating whether audio is muted.
        /// </summary>
        public bool IsMuted { get; private set; }

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
        }
    }
}
