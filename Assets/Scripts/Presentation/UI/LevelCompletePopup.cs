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
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameObject[] stars;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;
        [SerializeField] private ScoreTextConfiguration scoreTextConfiguration;

        private ILevelCommandInvoker commandInvoker;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
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

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
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

        /// <summary>
        /// Performs the SetLabel operation.
        /// </summary>
        public void SetLabel(string label)
        {
            if (levelCompleteText != null)
            {
                levelCompleteText.text = label;
            }
        }

        /// <summary>
        /// Performs the SetStars operation.
        /// </summary>
        public void SetStars(int count)
        {
            for (var i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(i < count);
            }
        }

        /// <summary>
        /// Performs the SetScoreText operation.
        /// </summary>
        public void SetScoreText(int stars)
        {
            if (scoreText == null)
            {
                return;
            }

            ScoreTextEntry entry = scoreTextConfiguration != null
                ? scoreTextConfiguration.GetEntry(stars)
                : new ScoreTextEntry(scoreText.text, scoreText.color);

            if (!string.IsNullOrEmpty(entry.Text))
            {
                scoreText.text = entry.Text;
            }

            scoreText.color = entry.Color;
        }

        /// <summary>
        /// Performs the Show operation.
        /// </summary>
        public void Show()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }
        }

        /// <summary>
        /// Performs the Hide operation.
        /// </summary>
        public void Hide()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Performs the SetCommandInvoker operation.
        /// </summary>
        public void SetCommandInvoker(ILevelCommandInvoker invoker)
        {
            commandInvoker = invoker;
            EnsureCommandInvoker();
        }

        /// <summary>
        /// Performs the RaiseRetry operation.
        /// </summary>
        private void RaiseRetry()
        {

            commandInvoker?.Retry();
        }

        /// <summary>
        /// Performs the RaiseNext operation.
        /// </summary>
        private void RaiseNext()
        {

            commandInvoker?.Next();
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
    }
}
