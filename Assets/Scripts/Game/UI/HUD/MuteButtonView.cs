using Game.UI.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Toggles audio mute state through the settings service.
    /// </summary>
    public class MuteButtonView : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        [SerializeField] private TMP_Text _Label;
        [SerializeField] private SettingsService _SettingsService;

        private void OnEnable()
        {
            if (_Button != null)
                _Button.onClick.AddListener(OnClicked);

            if (_SettingsService != null)
                _SettingsService.OnMuteChanged += OnMuteChanged;

            Refresh();
        }

        private void OnDisable()
        {
            if (_Button != null)
                _Button.onClick.RemoveListener(OnClicked);

            if (_SettingsService != null)
                _SettingsService.OnMuteChanged -= OnMuteChanged;
        }

        private void OnClicked()
        {
            if (_SettingsService == null)
                return;

            _SettingsService.SetMuted(!_SettingsService.IsMuted);
        }

        private void OnMuteChanged(bool isMuted)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_Label == null || _SettingsService == null)
                return;

            _Label.text = _SettingsService.IsMuted ? "UNMUTE" : "MUTE";
        }
    }
}
