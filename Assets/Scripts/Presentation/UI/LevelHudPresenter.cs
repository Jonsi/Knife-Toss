using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Leveling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace KnifeToss.Presentation.UI
{
    public class LevelHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [Header("UI References")]
        public LevelCompletePopup levelCompletePopup;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        public Button retryButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private GameObject muteIcon;
        [SerializeField] private GameObject unmuteIcon;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;

        private ILevelCommandInvoker commandInvoker;
        private ISettingsService _settingsService;

        private void Awake()
        {
            EnsureCommandInvoker();
            PropagateInvokerToPopup();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RaiseRetry);
            }
            if (muteButton != null)
            {
                muteButton.onClick.AddListener(ToggleMute);
            }
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(RaiseRetry);
            }
            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(ToggleMute);
            }
        }

        public void Render(LevelHudViewModel viewModel)
        {
            if (levelText != null)
            {
                levelText.text = viewModel.LevelLabel;
            }
            levelCompletePopup?.SetLabel(viewModel.LevelCompleteLabel);
        }

        public void ShowFail(bool show)
        {
            // Fail text functionality is removed.
            // The retry button is now shown via LevelFlowCoordinator.
        }

        public void ShowLevelComplete(bool show)
        {
            if (levelCompletePopup == null)
                return;

            if (show)
                levelCompletePopup.Show();
            else
                levelCompletePopup.Hide();
        }

        public void ResetState()
        {
            ShowFail(false);
            ShowLevelComplete(false);
        }

        [VContainer.Inject]
        public void Construct(ILevelCommandInvoker invoker, ISettingsService settingsService)
        {
            commandInvoker = invoker;
            _settingsService = settingsService;
            PropagateInvokerToPopup();
            UpdateMuteButtonIcon();
        }

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

        private void PropagateInvokerToPopup()
        {
            if (levelCompletePopup != null)
            {
                levelCompletePopup.SetCommandInvoker(commandInvoker);
            }
        }

        private void RaiseRetry()
        {
            commandInvoker?.Retry();
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
            
            var isMuted = _settingsService.IsMuted();
            muteIcon.SetActive(isMuted);
            unmuteIcon.SetActive(!isMuted);
        }
    }
}
