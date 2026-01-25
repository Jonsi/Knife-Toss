using System;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    public class SettingsPopup : BasePopup
    {
        [SerializeField] private Toggle _SoundToggle;
        [SerializeField] private Button _PrivacyPolicyButton;
        [SerializeField] private Button _ReportErrorButton;
        [SerializeField] private Button _CloseButton;
        
        public event Action<bool> OnSoundToggled;
        public event Action OnPrivacyPolicyRequested;
        public event Action OnReportErrorRequested;
        public event Action OnCloseRequested;

        private void Awake()
        {
            if (_SoundToggle != null)
                _SoundToggle.onValueChanged.AddListener(OnSoundValueChanged);
            
            if (_PrivacyPolicyButton != null)
                _PrivacyPolicyButton.onClick.AddListener(() => OnPrivacyPolicyRequested?.Invoke());
            
            if (_ReportErrorButton != null)
                _ReportErrorButton.onClick.AddListener(() => OnReportErrorRequested?.Invoke());
            
            if (_CloseButton != null)
                _CloseButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }

        public void Setup(bool isMuted)
        {
            if (_SoundToggle != null)
                _SoundToggle.SetIsOnWithoutNotify(!isMuted);
        }

        private void OnSoundValueChanged(bool isOn)
        {
            OnSoundToggled?.Invoke(!isOn);
        }
    }
}
