using UnityEngine;

namespace Game.UI.Popups
{
    /// <summary>
    /// Scene-owned references for the reusable Settings, Privacy, and Report flow.
    /// The host contains presentation only; settings, store, and navigation behavior
    /// remains in their existing services/controllers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsModalHost : MonoBehaviour
    {
        [SerializeField] private SettingsPopup _SettingsPopup;
        [SerializeField] private PrivacyPolicyPopup _PrivacyPopup;
        [SerializeField] private ReportErrorPopup _ReportErrorPopup;

        public SettingsPopup SettingsPopup => _SettingsPopup;
        public PrivacyPolicyPopup PrivacyPopup => _PrivacyPopup;
        public ReportErrorPopup ReportErrorPopup => _ReportErrorPopup;
        public bool IsConfigured =>
            _SettingsPopup != null &&
            _PrivacyPopup != null &&
            _ReportErrorPopup != null;

        private void Reset()
        {
            _SettingsPopup = GetComponentInChildren<SettingsPopup>(true);
            _PrivacyPopup = GetComponentInChildren<PrivacyPolicyPopup>(true);
            _ReportErrorPopup = GetComponentInChildren<ReportErrorPopup>(true);
        }
    }
}
