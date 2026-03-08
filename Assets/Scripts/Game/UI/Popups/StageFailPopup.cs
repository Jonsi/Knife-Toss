using Common.Events;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Popup shown when the player fails a level.
    /// Exposes three outcomes: Retry (same level), Restart Stage, and Continue (watch a rewarded ad).
    /// The popup itself is ad-agnostic — it raises event channels and lets controllers decide what happens.
    /// Call <see cref="SetContinueAvailable"/> to show/hide the Continue button based on ad availability.
    /// </summary>
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;
        [SerializeField] private Button _ContinueButton;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;
        [SerializeField] private VoidEventChannel _ContinueRequestedChannel;

        private void OnEnable()
        {
            _RetryButton.onClick.AddListener(OnRetryClicked);
            _RestartStageButton.onClick.AddListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDisable()
        {
            _RetryButton.onClick.RemoveListener(OnRetryClicked);
            _RestartStageButton.onClick.RemoveListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.RemoveListener(OnContinueClicked);
        }

        /// <summary>
        /// Shows or hides the Continue button based on whether a rewarded ad is available.
        /// Called by <see cref="Game.Ads.Controllers.AdRewardController"/> each time the popup appears.
        /// </summary>
        public void SetContinueAvailable(bool available)
        {
            if (_ContinueButton != null)
                _ContinueButton.gameObject.SetActive(available);
        }

        private void OnRetryClicked()
        {
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Raise();
        }

        private void OnRestartStageClicked()
        {
            if (_RestartStageChannel != null)
                _RestartStageChannel.Raise();
        }

        private void OnContinueClicked()
        {
            if (_ContinueRequestedChannel != null)
                _ContinueRequestedChannel.Raise();
        }
    }
}
