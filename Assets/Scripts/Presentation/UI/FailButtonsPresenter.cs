using Core.Events;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Presentation.UI
{
    /// <summary>
    /// Manages the display and interaction of fail buttons (Retry and Give Up).
    /// </summary>
    public sealed class FailButtonsPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button giveUpButton;

        [Header("Events")]
        [SerializeField] private LevelCommandEventChannel commandChannel;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
            }
            if (giveUpButton != null)
            {
                giveUpButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        private void Start()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryClicked);
            }

            if (giveUpButton != null)
            {
                giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            }
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryClicked);
            }

            if (giveUpButton != null)
            {
                giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            }
        }

        /// <summary>
        /// Shows or hides the fail buttons.
        /// </summary>
        public void ShowFailButtons(bool show)
        {
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(show);
            }
            if (giveUpButton != null)
            {
                giveUpButton.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// Handles retry button click - restarts the same level.
        /// </summary>
        private void HandleRetryClicked()
        {
            commandChannel?.RaiseRetry();
            ShowFailButtons(false);
        }

        /// <summary>
        /// Handles give up button click - resets to level 1 of the current stage.
        /// </summary>
        private void HandleGiveUpClicked()
        {
            commandChannel?.RaiseResetToStageStart();
            ShowFailButtons(false);
        }
    }
}

