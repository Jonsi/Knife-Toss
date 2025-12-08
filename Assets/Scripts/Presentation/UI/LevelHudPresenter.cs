using Core.Abstractions;
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
        [SerializeField] private StageProgressBarView stageProgressBar;
        [SerializeField] private Button muteButton;
        [SerializeField] private GameObject muteIcon;
        [SerializeField] private GameObject unmuteIcon;
        [SerializeField] private KnifeMagazineDisplay magazineDisplay;
        [SerializeField] private StageFailPopup stageFailPopup;
        [SerializeField] private FruitCountDisplay fruitCountDisplay;

        private ISettingsService _settingsService;
        private IPlayerDataService _playerDataService;
        private ILevelCatalog _levelCatalog;

        private void Awake()
        {
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
        /// Renders the HUD with the provided view model data.
        /// </summary>
        public void Render(LevelHudViewModel viewModel)
        {
            RenderStageProgress(viewModel);
            magazineDisplay?.Render(viewModel.TotalKnives, viewModel.KnivesThrown);
            fruitCountDisplay?.Render(viewModel.TotalFruitsHit);
        }

        /// <summary>
        /// Shows or hides the stage fail popup.
        /// </summary>
        public void ShowFail(bool show)
        {
            if (show)
                stageFailPopup?.Show();
            else
                stageFailPopup?.Hide();
        }

        /// <summary>
        /// Resets the HUD to its initial state.
        /// </summary>
        public void ResetState()
        {
            stageFailPopup?.Hide();
            magazineDisplay?.ResetDisplay();
        }

        /// <summary>
        /// Constructs the presenter with required services.
        /// </summary>
        [VContainer.Inject]
        public void Construct(ISettingsService settingsService, IPlayerDataService playerDataService, ILevelCatalog levelCatalog)
        {
            _settingsService = settingsService;
            _playerDataService = playerDataService;
            _levelCatalog = levelCatalog;
            UpdateMuteButtonIcon();
        }

        private void ToggleMute()
        {
            _settingsService.ToggleMute();
            UpdateMuteButtonIcon();
        }

        private void UpdateMuteButtonIcon()
        {
            if (muteIcon == null || unmuteIcon == null)
            {
                return;
            }
            
            bool isMuted = _settingsService.IsMuted();
            muteIcon.SetActive(isMuted);
            unmuteIcon.SetActive(!isMuted);
        }

        /// <summary>
        /// Updates the stage progress bar display based on the current view model.
        /// </summary>
        private void RenderStageProgress(LevelHudViewModel viewModel)
        {
            if (stageProgressBar == null || _levelCatalog == null)
            {
                return;
            }

            int currentStage = viewModel.StageIndex;
            int currentLevel = viewModel.LevelIndex;
            int levelsInStage = _levelCatalog.GetLevelCountInStage(currentStage);
            
            int? nextStage = null;
            int nextStageIndex = currentStage + 1;
            if (_levelCatalog.IsValidStageAndLevel(nextStageIndex, 1))
            {
                nextStage = nextStageIndex;
            }

            stageProgressBar.Initialize(currentStage, nextStage, currentLevel, levelsInStage);
        }
    }
}
