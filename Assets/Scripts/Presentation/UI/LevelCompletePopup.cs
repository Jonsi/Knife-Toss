using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Leveling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{



    public sealed class LevelCompletePopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI levelCompleteText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject[] stars;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;

        private ILevelCommandInvoker commandInvoker;

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

        public void SetLabel(string label)
        {
            if (levelCompleteText != null)
            {
                levelCompleteText.text = label;
            }
        }

        public void SetStars(int count)
        {
            for (var i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(i < count);
            }
        }

        public void Show()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        public void SetCommandInvoker(ILevelCommandInvoker invoker)
        {
            commandInvoker = invoker;
            EnsureCommandInvoker();
        }

        private void RaiseRetry()
        {

            commandInvoker?.Retry();
        }

        private void RaiseNext()
        {

            commandInvoker?.Next();
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
