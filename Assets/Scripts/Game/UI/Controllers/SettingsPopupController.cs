using System;
using System.IO;
using Game.UI.Data;
using Game.UI.Popups;
using Game.UI.Services;
using UnityEngine;
using VContainer;

namespace Game.UI.Controllers
{
    public class SettingsPopupController : IDisposable
    {
        private readonly SettingsPopup _SettingsPopup;
        private readonly PrivacyPolicyPopup _PrivacyPopup;
        private readonly ReportErrorPopup _ReportErrorPopup;
        private readonly SettingsService _SettingsService;
        private readonly PrivacyPolicyContent _PrivacyContent;

        [Inject]
        public SettingsPopupController(
            SettingsPopup settingsPopup,
            PrivacyPolicyPopup privacyPopup,
            ReportErrorPopup reportErrorPopup,
            SettingsService settingsService,
            PrivacyPolicyContent privacyContent)
        {
            _SettingsPopup = settingsPopup;
            _PrivacyPopup = privacyPopup;
            _ReportErrorPopup = reportErrorPopup;
            _SettingsService = settingsService;
            _PrivacyContent = privacyContent;

            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void ShowSettings()
        {
            _SettingsPopup.Setup(_SettingsService.IsMuted);
            _SettingsPopup.Show();
        }

        public void ToggleSettings()
        {
            if (_SettingsPopup.gameObject.activeSelf)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
        }

        public void HideSettings()
        {
            _SettingsPopup.Hide();
        }

        private void Subscribe()
        {
            if (_SettingsPopup != null)
            {
                _SettingsPopup.OnSoundToggled += OnSoundToggled;
                _SettingsPopup.OnPrivacyPolicyRequested += OnPrivacyPolicyRequested;
                _SettingsPopup.OnReportErrorRequested += OnReportErrorRequested;
                _SettingsPopup.OnCloseRequested += OnSettingsCloseRequested;
            }

            if (_PrivacyPopup != null)
            {
                _PrivacyPopup.OnCloseRequested += OnPrivacyCloseRequested;
            }

            if (_ReportErrorPopup != null)
            {
                _ReportErrorPopup.OnCancelRequested += OnReportCancelRequested;
            }
        }

        private void Unsubscribe()
        {
            if (_SettingsPopup != null)
            {
                _SettingsPopup.OnSoundToggled -= OnSoundToggled;
                _SettingsPopup.OnPrivacyPolicyRequested -= OnPrivacyPolicyRequested;
                _SettingsPopup.OnReportErrorRequested -= OnReportErrorRequested;
                _SettingsPopup.OnCloseRequested -= OnSettingsCloseRequested;
            }

            if (_PrivacyPopup != null)
            {
                _PrivacyPopup.OnCloseRequested -= OnPrivacyCloseRequested;
            }

            if (_ReportErrorPopup != null)
            {
                _ReportErrorPopup.OnCancelRequested -= OnReportCancelRequested;
            }
        }

        private void OnSoundToggled(bool isMuted)
        {
            _SettingsService.SetMuted(isMuted);
        }

        private void OnPrivacyPolicyRequested()
        {
            _SettingsPopup.Hide();
            
            if (_PrivacyContent != null)
                _PrivacyPopup.SetContent(_PrivacyContent.PolicyText);
                
            _PrivacyPopup.Show();
        }

        private void OnReportErrorRequested()
        {
            _SettingsPopup.Hide();
            _ReportErrorPopup.Clear();
            _ReportErrorPopup.Show();
        }

        private void OnSettingsCloseRequested()
        {
            _SettingsPopup.Hide();
        }

        private void OnPrivacyCloseRequested()
        {
            _PrivacyPopup.Hide();
            _SettingsPopup.Show();
        }

        private void OnReportCancelRequested()
        {
            _ReportErrorPopup.Hide();
            _SettingsPopup.Show();
        }
    }
}
