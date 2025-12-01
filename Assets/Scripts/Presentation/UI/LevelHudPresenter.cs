using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{
    public class LevelHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI knivesLeftText;
        [Header("UI References")]
        [SerializeField] private GameObject failText;
        [SerializeField] private LevelCommandEventChannel commandChannel;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextButton;

        private void Awake()
        {
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

        public void Render(LevelSession session)
        {
            if (session == null)
                return;

            if (levelText != null)
            {
                levelText.text = $"{session.LevelIndex}";
            }

            if (knivesLeftText != null)
            {
                int remaining = Mathf.Max(0, session.Rules.RequiredKnives - session.KnivesStuck);
                knivesLeftText.text = $"{remaining}";
            }
        }

        private void RaiseRetry()
        {
            commandChannel?.RaiseRetry();
        }

        private void RaiseNext()
        {
            commandChannel?.RaiseNext();
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
    }
}
