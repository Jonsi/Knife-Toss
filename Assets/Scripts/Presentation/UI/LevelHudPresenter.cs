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
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private Button muteButton;
        [SerializeField] private GameObject muteIcon;
        [SerializeField] private GameObject unmuteIcon;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;
        [SerializeField] private KnifeMagazineDisplay magazineDisplay;
        [SerializeField] private FailButtonsPresenter failButtonsPresenter;
        [SerializeField] private FruitCountDisplay fruitCountDisplay;

        private ILevelCommandInvoker commandInvoker;
        private ISettingsService _settingsService;
        private IPlayerDataService _playerDataService;

        private void Awake()
        {
            EnsureCommandInvoker();

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(ToggleMute);
            }
        }

        private void OnDestroy()
        {
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
            magazineDisplay?.Render(viewModel.TotalKnives, viewModel.KnivesThrown);
            fruitCountDisplay?.Render(viewModel.TotalFruitsHit);
        }

        /// <summary>
        /// Performs the ShowFail operation.
        /// </summary>
        public void ShowFail(bool show)
        {
            if (failButtonsPresenter != null)
            {
                failButtonsPresenter.ShowFailButtons(show);
            }
            else
            {
                Debug.LogWarning("[LevelHudPresenter] failButtonsPresenter is null, cannot show fail buttons");
            }
        }

        /// <summary>
        /// Performs the ResetState operation.
        /// </summary>
        public void ResetState()
        {
            ShowFail(false);
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

            if (int.TryParse(viewModel.LevelLabel, out int levelNum))
            {
                levelIconView.SetLevelNumber(levelNum);
            }
            else
            {
                levelIconView.SetLevelNumber(viewModel.LevelIndex);
            }
        }

    }
}
