using Core.Abstractions;
using Core.Events;
using Infrastructure.Leveling;
using Presentation.UI.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Represents the LevelHudPresenter class.
    /// </summary>
    public class LevelHudPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private LevelIconView levelIconView;
        [SerializeField] private LevelsPopup levelsPopup;
        public LevelCompletePopup levelCompletePopup;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        public Button retryButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private GameObject muteIcon;
        [SerializeField] private GameObject unmuteIcon;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;
        [SerializeField] private KnifeMagazineDisplay magazineDisplay;

        private ILevelCommandInvoker commandInvoker;
        private ISettingsService _settingsService;
        private IPlayerDataService _playerDataService;
        private Button levelIconButton;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            EnsureCommandInvoker();
            PropagateInvokerToPopup();
            BindLevelIconButton();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RaiseRetry);
            }
            if (muteButton != null)
            {
                muteButton.onClick.AddListener(ToggleMute);
            }
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            if (levelIconButton != null)
            {
                levelIconButton.onClick.RemoveListener(HandleLevelIconClicked);
            }
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(RaiseRetry);
            }
            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(ToggleMute);
            }
        }

        /// <summary>
        /// Performs the Render operation.
        /// </summary>
        public void Render(LevelHudViewModel viewModel)
        {
            RenderLevelIcon(viewModel);
            levelCompletePopup?.SetLabel(viewModel.LevelCompleteLabel);
            magazineDisplay?.Render(viewModel.TotalKnives, viewModel.KnivesThrown);
        }

        /// <summary>
        /// Performs the ShowFail operation.
        /// </summary>
        public void ShowFail(bool show)
        {
        }

        /// <summary>
        /// Performs the ShowLevelComplete operation.
        /// </summary>
        public void ShowLevelComplete(bool show)
        {
            if (levelCompletePopup == null)
                return;

            if (show)
                levelCompletePopup.Show();
            else
                levelCompletePopup.Hide();
        }

        /// <summary>
        /// Performs the ResetState operation.
        /// </summary>
        public void ResetState()
        {
            ShowFail(false);
            ShowLevelComplete(false);
            magazineDisplay?.ResetDisplay();
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [VContainer.Inject]
        public void Construct(ILevelCommandInvoker invoker, ISettingsService settingsService, IPlayerDataService playerDataService)
        {
            commandInvoker = invoker;
            _settingsService = settingsService;
            _playerDataService = playerDataService;
            PropagateInvokerToPopup();
            UpdateMuteButtonIcon();
        }

        /// <summary>
        /// Performs the EnsureCommandInvoker operation.
        /// </summary>
        private void EnsureCommandInvoker()
        {
            if (commandInvoker != null)
            {
                return;
            }

            if (commandChannel != null)
            {
                commandInvoker = new LevelCommandChannelInvoker(commandChannel);
            }
        }

        /// <summary>
        /// Performs the PropagateInvokerToPopup operation.
        /// </summary>
        private void PropagateInvokerToPopup()
        {
            if (levelCompletePopup != null)
            {
                levelCompletePopup.SetCommandInvoker(commandInvoker);
            }
        }

        /// <summary>
        /// Subscribes to the HUD level icon button to open the popup.
        /// </summary>
        private void BindLevelIconButton()
        {
            if (levelIconButton == null && levelIconView != null)
            {
                levelIconButton = levelIconView.GetComponent<Button>() ?? levelIconView.GetComponentInChildren<Button>();
            }

            if (levelIconButton != null)
            {
                levelIconButton.onClick.AddListener(HandleLevelIconClicked);
            }
        }

        /// <summary>
        /// Performs the RaiseRetry operation.
        /// </summary>
        private void RaiseRetry()
        {
            commandInvoker?.Retry();
        }

        /// <summary>
        /// Performs the ToggleMute operation.
        /// </summary>
        private void ToggleMute()
        {
            _settingsService.ToggleMute();
            UpdateMuteButtonIcon();
        }

        /// <summary>
        /// Performs the UpdateMuteButtonIcon operation.
        /// </summary>
        private void UpdateMuteButtonIcon()
        {
            if (muteIcon == null || unmuteIcon == null)
            {
                return;
            }
            
            var isMuted = _settingsService.IsMuted();
            muteIcon.SetActive(isMuted);
            unmuteIcon.SetActive(!isMuted);
        }

        /// <summary>
        /// Updates the level icon display based on the current view model.
        /// </summary>
        private void RenderLevelIcon(LevelHudViewModel viewModel)
        {
            if (levelIconView == null)
            {
                return;
            }

            levelIconView.SetLevelNumber(viewModel.LevelIndex);

            if (_playerDataService != null)
            {
                int stars = _playerDataService.GetStarsForLevel(viewModel.LevelIndex);
                levelIconView.SetStars(stars);
            }
        }

        /// <summary>
        /// Handles clicks on the HUD level icon.
        /// </summary>
        private void HandleLevelIconClicked()
        {
            levelsPopup?.ShowPopup();
        }

    }
}
