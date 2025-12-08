using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Displays a horizontal chain showing current stage, level progress, and next stage.
    /// Layout: [Current Stage] - [Connecting Line] - [Level Container with levels] - [Connecting Line] - [Last Stage]
    /// </summary>
    public sealed class StageProgressBarView : MonoBehaviour
    {
        [Header("Stage Circles")]
        [SerializeField] private StageCircleView currentStageCircle;
        [SerializeField] private StageCircleView lastStageCircle;

        [Header("Level Container")]
        [SerializeField] private RectTransform levelContainer;

        [Header("Prefabs")]
        [SerializeField] private LevelCircleView levelCirclePrefab;

        [Header("Connecting Line")]
        [SerializeField] private Image connectingLine;

        [Header("Visual Settings")]
        [SerializeField] private float disabledStageAlpha = 0.3f;

        private readonly List<LevelCircleView> instantiatedLevels = new List<LevelCircleView>();

        /// <summary>
        /// Initializes the progress bar with stage and level data.
        /// </summary>
        public void Initialize(int currentStage, int? nextStage, int currentLevel, int levelsInStage)
        {
            ClearLevels();
            UpdateStageCircles(currentStage, nextStage);
            CreateLevelCircles(currentLevel, levelsInStage);
        }

        private void UpdateStageCircles(int currentStage, int? nextStage)
        {
            if (currentStageCircle != null)
            {
                currentStageCircle.SetStage(currentStage);
            }

            if (lastStageCircle != null)
            {
                if (nextStage.HasValue)
                {
                    lastStageCircle.SetStage(nextStage.Value);
                }
                else
                {
                    lastStageCircle.SetDisabled(disabledStageAlpha);
                }
            }
        }

        private void CreateLevelCircles(int currentLevel, int levelsInStage)
        {
            if (levelContainer == null || levelCirclePrefab == null)
            {
                return;
            }

            for (int i = 1; i <= levelsInStage; i++)
            {
                LevelCircleView levelCircle = Instantiate(levelCirclePrefab, levelContainer);
                instantiatedLevels.Add(levelCircle);
                levelCircle.SetFilled(i <= currentLevel);
            }
        }

        private void ClearLevels()
        {
            foreach (LevelCircleView level in instantiatedLevels)
            {
                if (level != null)
                {
                    Destroy(level.gameObject);
                }
            }
            instantiatedLevels.Clear();
        }
    }
}

