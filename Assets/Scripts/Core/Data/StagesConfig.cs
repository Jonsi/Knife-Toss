using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Represents a single stage containing an ordered list of levels.
    /// </summary>
    [System.Serializable]
    public class StageData
    {
        [Tooltip("Name of this stage (for display purposes).")]
        public string stageName = "Stage";

        [Tooltip("Ordered list of levels in this stage. Index 0 = Level 1, Index 1 = Level 2, etc.")]
        public List<LevelData> levels = new List<LevelData>();
    }

    /// <summary>
    /// Represents the StagesConfig class that organizes levels into stages.
    /// </summary>
    [CreateAssetMenu(fileName = "StagesConfig", menuName = "Knife Toss/Configurations/Gameplay/Stages Config")]
    public class StagesConfig : ScriptableObject
    {
        [Header("Stage Definitions")]
        [Tooltip("List of stages. Index 0 = Stage 1, Index 1 = Stage 2, etc.")]
        public List<StageData> stages = new List<StageData>();

        /// <summary>
        /// Gets the level data for a specific stage and level index (both 1-based).
        /// </summary>
        public LevelData GetLevelData(int stageIndex, int levelIndex)
        {
            int stageArrayIndex = stageIndex - 1;
            int levelArrayIndex = levelIndex - 1;

            if (stageArrayIndex >= 0 && stageArrayIndex < stages.Count)
            {
                StageData stage = stages[stageArrayIndex];
                if (stage?.levels != null && levelArrayIndex >= 0 && levelArrayIndex < stage.levels.Count)
                {
                    return stage.levels[levelArrayIndex];
                }

                if (stage?.levels != null && stage.levels.Count > 0)
                {
                    return stage.levels[stage.levels.Count - 1];
                }
            }

            if (stages.Count > 0)
            {
                StageData lastStage = stages[stages.Count - 1];
                if (lastStage?.levels != null && lastStage.levels.Count > 0)
                {
                    return lastStage.levels[lastStage.levels.Count - 1];
                }
            }

            return new LevelData();
        }

        /// <summary>
        /// Gets the number of levels in a specific stage (1-based stage index).
        /// </summary>
        public int GetLevelCountInStage(int stageIndex)
        {
            int stageArrayIndex = stageIndex - 1;
            if (stageArrayIndex >= 0 && stageArrayIndex < stages.Count)
            {
                StageData stage = stages[stageArrayIndex];
                return stage?.levels != null ? stage.levels.Count : 0;
            }
            return 0;
        }

        /// <summary>
        /// Gets the total number of stages.
        /// </summary>
        public int GetStageCount()
        {
            return stages != null ? stages.Count : 0;
        }

        /// <summary>
        /// Checks if a stage and level combination is valid (both 1-based).
        /// </summary>
        public bool IsValidStageAndLevel(int stageIndex, int levelIndex)
        {
            int stageArrayIndex = stageIndex - 1;
            int levelArrayIndex = levelIndex - 1;

            if (stageArrayIndex < 0 || stageArrayIndex >= stages.Count)
            {
                return false;
            }

            StageData stage = stages[stageArrayIndex];
            if (stage?.levels == null)
            {
                return false;
            }

            return levelArrayIndex >= 0 && levelArrayIndex < stage.levels.Count;
        }
    }
}
