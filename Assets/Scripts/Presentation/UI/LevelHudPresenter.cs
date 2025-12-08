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
        [SerializeField] private LevelIconView levelIconView;
        [SerializeField] private Button muteButton;
        [SerializeField] private GameObject muteIcon;
        [SerializeField] private GameObject unmuteIcon;
        [SerializeField] private KnifeMagazineDisplay magazineDisplay;
        [SerializeField] private StageFailPopup stageFailPopup;
        [SerializeField] private FruitCountDisplay fruitCountDisplay;

        private ISettingsService _settingsService;
        private IPlayerDataService _playerDataService;

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
            if (show)
                stageFailPopup?.Show();
            else
                stageFailPopup?.Hide();
        }

        /// <summary>
        /// Performs the ResetState operation.
        /// </summary>
        public void ResetState()
        {
            stageFailPopup?.Hide();
            magazineDisplay?.ResetDisplay();
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [VContainer.Inject]
        public void Construct(ISettingsService settingsService, IPlayerDataService playerDataService)
        {
            _settingsService = settingsService;
            _playerDataService = playerDataService;
            UpdateMuteButtonIcon();
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
