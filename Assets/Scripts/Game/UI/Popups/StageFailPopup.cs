using Cysharp.Threading.Tasks;
using Common.Events;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Economy.Data;
using Game.Flow.Game;
using Game.Flow.Stage;
using Game.UI.Common;
using Game.UI.Controllers;
using Game.UI.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    /// <summary>
    /// Popup shown when the player fails a level.
    /// Exposes the configured paid retry transaction and one quiet return-to-menu action.
    /// </summary>
    public class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _StageLevelLabel;
        [SerializeField] private Button _RetryButton;
        [SerializeField] private StorePricePillView _RetryPricePill;
        [SerializeField] private Button _MenuButton;

        [Header("Channels")]
        [SerializeField] private VoidEventChannel _ContinueRequestedChannel;

        [Inject] private FeedbackEventChannel _FeedbackEventChannel;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private StageData _StageData;

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
            RefreshStageLevelLabel();
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

            if (_ContinueRequestedChannel != null)
                _ContinueRequestedChannel.Raise();
            else
                _RetryPresenter?.CancelSubmission();
        }

        public void SetRetryState(bool visible, bool interactable, int cost)
        {
            if (_RetryButton == null)
                return;

            _RetryButton.gameObject.SetActive(visible);
            _RetryButton.interactable = interactable;
            _RetryPricePill?.SetVisible(visible);
            _RetryPricePill?.SetPrice(cost, CurrencyType.Coin);
        }

        public void PresentRetryBlocked(FeedbackEventData feedback)
        {
            _RetryPresenter ??= ActionButtonPresenter.GetOrAdd(_RetryButton, _FeedbackEventChannel);
            _RetryPresenter?.PresentBlocked(feedback);
        }

        private void RefreshStageLevelLabel()
        {
            if (_StageLevelLabel == null)
                return;

            int stageNumber = (_StageData?.StageIndex ?? 0) + 1;
            int levelNumber = (_StageData?.CurrentLevelIndex ?? 0) + 1;
            _StageLevelLabel.text = $"STAGE {stageNumber}  •  LEVEL {levelNumber}";
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
