using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Represents the LevelConfig class.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Knife Toss/Configurations/Gameplay/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Definitions")]
        [Tooltip("List of level configurations. Index 0 = Level 1, Index 1 = Level 2, etc.")]
        public List<LevelData> levels = new List<LevelData>();

        /// <summary>
        /// Performs the GetLevelData operation.
        /// </summary>
        public LevelData GetLevelData(int levelIndex)
        {
            int arrayIndex = levelIndex - 1;

            if (arrayIndex >= 0 && arrayIndex < levels.Count)
            {
                return levels[arrayIndex];
            }

            if (levels.Count > 0)
            {
                return levels[levels.Count - 1];
            }

            return new LevelData();
        }
    }
}
