using Cysharp.Threading.Tasks;
using Common.Events;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Flow.Game;
using Game.UI.Common;
using Game.UI.Controllers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    /// <summary>
    /// Popup shown when the player fails a level.
    /// Exposes one free retry action and one quiet return-to-menu action.
    /// </summary>
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button _RetryButton;
        [SerializeField] private Button _MenuButton;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelChannel;

        [Inject] private FeedbackEventChannel _FeedbackEventChannel;
        [Inject] private SceneLoaderService _SceneLoaderService;

        private ActionButtonPresenter _RetryPresenter;
        private ActionButtonPresenter _MenuPresenter;
        private UIController _UIController;
        private bool _RecoveryCommitted;

        private void Awake()
        {
            _UIController = GetComponentInParent<UIController>();
        }

        private void OnEnable()
        {
            _RecoveryCommitted = false;
            _RetryPresenter = ActionButtonPresenter.GetOrAdd(_RetryButton, _FeedbackEventChannel);
            _MenuPresenter = ActionButtonPresenter.GetOrAdd(_MenuButton, _FeedbackEventChannel);

            if (_RetryButton != null)
                _RetryButton.onClick.AddListener(OnRetryClicked);

            if (_MenuButton != null)
                _MenuButton.onClick.AddListener(OnMenuClicked);
        }

        protected override void OnDisable()
        {
            if (_RetryButton != null)
                _RetryButton.onClick.RemoveListener(OnRetryClicked);

            if (_MenuButton != null)
                _MenuButton.onClick.RemoveListener(OnMenuClicked);

            _RecoveryCommitted = false;
            base.OnDisable();
        }

        private void OnRetryClicked()
        {
            if (_RecoveryCommitted || (_RetryPresenter != null && !_RetryPresenter.TryBeginSubmission()))
                return;

            _RecoveryCommitted = true;
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Raise();
        }

        private void OnMenuClicked()
        {
            LoadMainMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMainMenuAsync()
        {
            if (_RecoveryCommitted || (_MenuPresenter != null && !_MenuPresenter.TryBeginSubmission()))
                return;

            _RecoveryCommitted = true;
            try
            {
                if (_UIController != null)
                    await _UIController.PlayStageOutAsync();

                if (_SceneLoaderService != null)
                    await _SceneLoaderService.LoadMainMenu();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                _RecoveryCommitted = false;
                _MenuPresenter?.CancelSubmission();
                _MenuPresenter?.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "navigation.failed",
                    gameObject.name));
            }
        }
    }
}
