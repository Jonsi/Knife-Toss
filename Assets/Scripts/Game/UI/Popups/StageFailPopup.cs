using Common.Events;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    /// <summary>
    /// Popup shown when the player fails a level.
    /// Exposes retry, restart, and paid-continue outcomes.
    /// The popup itself is flow-agnostic — it raises event channels and lets controllers decide what happens.
    /// </summary>
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _RestartStageButton;
        [SerializeField] private Button _ContinueButton;
        [SerializeField] private TMP_Text _ContinueAmountText;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;
        [SerializeField] private VoidEventChannel _ContinueRequestedChannel;

        [Inject] private FeedbackEventChannel _FeedbackEventChannel;

        private ActionButtonPresenter _ContinuePresenter;

        private void OnEnable()
        {
            ActionButtonPresenter.GetOrAdd(_RetryButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_RestartStageButton, _FeedbackEventChannel);
            _ContinuePresenter = ActionButtonPresenter.GetOrAdd(_ContinueButton, _FeedbackEventChannel);

            if (_RetryButton != null && _RetryButton != _ContinueButton)
                _RetryButton.onClick.AddListener(OnRetryClicked);

            if (_RestartStageButton != null)
                _RestartStageButton.onClick.AddListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.AddListener(OnContinueClicked);
        }

        protected override void OnDisable()
        {
            if (_RetryButton != null && _RetryButton != _ContinueButton)
                _RetryButton.onClick.RemoveListener(OnRetryClicked);

            if (_RestartStageButton != null)
                _RestartStageButton.onClick.RemoveListener(OnRestartStageClicked);

            if (_ContinueButton != null)
                _ContinueButton.onClick.RemoveListener(OnContinueClicked);

            base.OnDisable();
        }

        public void SetContinueAvailable(bool available)
        {
            SetContinueState(available, available, 0);
        }

        public void SetContinueState(bool visible, bool interactable, int cost)
        {
            if (_ContinueButton == null)
                return;

            _ContinueButton.gameObject.SetActive(visible);
            _ContinueButton.interactable = interactable;

            if (_ContinueAmountText != null)
            {
                _ContinueAmountText.text = cost.ToString();
            }
        }

        public void PresentContinueBlocked(FeedbackEventData feedback)
        {
            _ContinuePresenter ??= ActionButtonPresenter.GetOrAdd(_ContinueButton, _FeedbackEventChannel);
            _ContinuePresenter?.PresentBlocked(feedback);
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
