using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Leveling;
using VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{
    public class LevelHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [Header("UI References")]
        [SerializeField] private GameObject failText;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;

        private KnifeToss.Core.Abstractions.ILevelCommandInvoker commandInvoker;

        private void Awake()
        {
            EnsureCommandInvoker();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RaiseRetry);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(RaiseNext);
            }
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(RaiseRetry);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(RaiseNext);
            }
        }

        public void Render(LevelHudViewModel viewModel)
        {
            if (levelText != null)
            {
                levelText.text = viewModel.LevelLabel;
            }
        }

        private void RaiseRetry()
        {
            commandInvoker?.Retry();
        }

        private void RaiseNext()
        {
            commandInvoker?.Next();
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
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(show);
            }
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
    }
}
