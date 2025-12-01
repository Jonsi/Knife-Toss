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
        [SerializeField] private GameObject failText;
        [SerializeField] private LevelCompletePopup levelCompletePopup;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private Button retryButton;

        private KnifeToss.Core.Abstractions.ILevelCommandInvoker commandInvoker;

        private void Awake()
        {
            EnsureCommandInvoker();
            PropagateInvokerToPopup();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RaiseRetry);
            }
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(RaiseRetry);
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
            if (failText != null)
            {
                failText.SetActive(show);
            }
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
        public void Construct(KnifeToss.Core.Abstractions.ILevelCommandInvoker invoker)
        {
            commandInvoker = invoker;
            PropagateInvokerToPopup();
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
    }
}
