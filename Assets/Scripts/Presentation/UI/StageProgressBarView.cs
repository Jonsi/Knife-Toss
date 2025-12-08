using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Presentation.UI
{
    /// <summary>
    /// Displays a progress bar showing level progress within a stage.
    /// </summary>
    public sealed class StageProgressBarView : MonoBehaviour
    {
        [Header("Content Container")]
        [SerializeField] private Transform contentContainer;

        [Header("Prefabs")]
        [SerializeField] private ProgressBarSlot slotPrefab;

        [Header("Stage Text")]
        [SerializeField] private TextMeshProUGUI currentStageText;
        [SerializeField] private TextMeshProUGUI nextStageText;

        [Header("Colors")]
        [SerializeField] private Color completeColor = Color.green;
        [SerializeField] private Color incompleteColor = Color.gray;

        private readonly List<ProgressBarSlot> instantiatedSlots = new List<ProgressBarSlot>();

        /// <summary>
        /// Initializes the progress bar with stage and level data.
        /// </summary>
        public void Initialize(int currentStage, int? nextStage, int currentLevel, int levelsInStage)
        {
            SetStageTexts(currentStage, nextStage);
            ClearSlots();
            CreateSlots(currentLevel, levelsInStage);
        }

        private void SetStageTexts(int currentStage, int? nextStage)
        {
            if (currentStageText != null)
            {
                currentStageText.text = currentStage.ToString();
            }

            if (nextStageText != null)
            {
                nextStageText.text = nextStage?.ToString() ?? string.Empty;
            }
        }

        private void CreateSlots(int currentLevel, int levelsInStage)
        {
            if (contentContainer == null || slotPrefab == null)
            {
                return;
            }

            for (int i = 1; i <= levelsInStage; i++)
            {
                ProgressBarSlot slot = Instantiate(slotPrefab, contentContainer);
                instantiatedSlots.Add(slot);
                bool isCompleted = i <= currentLevel;
                slot.SetCompleted(isCompleted, completeColor, incompleteColor);
            }
        }

        private void ClearSlots()
        {
            foreach (ProgressBarSlot slot in instantiatedSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            instantiatedSlots.Clear();
        }
    }
}

