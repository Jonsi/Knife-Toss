using Core.Domain;
using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Presentation.UI
{
    /// <summary>
    /// Displays the stage fail popup with stage/level information and action buttons.
    /// </summary>
    public sealed class StageFailPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI stageNumberText;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button restartStageButton;

        [Header("Text Prefixes")]
        [SerializeField] private string stagePrefix = "Stage";
        [SerializeField] private string levelPrefix = "Level";

        [Header("Events")]
        [SerializeField] private LevelCommandEventChannel commandChannel;

        private LevelSession session;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        private void Start()
        {
            continueButton?.onClick.AddListener(HandleContinueClicked);
            restartStageButton?.onClick.AddListener(HandleRestartStageClicked);
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            continueButton?.onClick.RemoveListener(HandleContinueClicked);
            restartStageButton?.onClick.RemoveListener(HandleRestartStageClicked);
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [Inject]
        public void Construct(LevelSession levelSession)
        {
            session = levelSession;
        }

        /// <summary>
        /// Shows the popup and updates stage/level information.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (session != null)
            {
                UpdateStageLevelDisplay();
            }
        }

        /// <summary>
        /// Updates the stage and level number displays.
        /// </summary>
        private void UpdateStageLevelDisplay()
        {
            if (stageNumberText != null)
                stageNumberText.text = $"{stagePrefix} {session.StageIndex}";

            if (levelNumberText != null)
                levelNumberText.text = $"{levelPrefix} {session.LevelIndex}";
        }

        /// <summary>
        /// Handles continue button click - retries the current level.
        /// </summary>
        private void HandleContinueClicked()
        {
            commandChannel?.RaiseRetry();
            Hide();
        }

        /// <summary>
        /// Handles restart stage button click - resets to level 1 of the current stage.
        /// </summary>
        private void HandleRestartStageClicked()
        {
            commandChannel?.RaiseResetToStageStart();
            Hide();
        }
    }
}

