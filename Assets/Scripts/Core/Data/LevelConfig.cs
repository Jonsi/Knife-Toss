using UnityEngine;
using System.Collections.Generic;

namespace KnifeToss.Core.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Knife Toss/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Definitions")]
        [Tooltip("List of level configurations. Index 0 = Level 1, Index 1 = Level 2, etc.")]
        public List<LevelData> levels = new List<LevelData>();

        [Header("Procedural Generation")]
        [Tooltip("If true, levels beyond the defined list will be procedurally generated")]
        public bool generateProceduralLevels = true;

        [Tooltip("Base values for procedural level generation")]
        public int proceduralBaseKnives = 5;
        [Tooltip("Base rotation speed used when generating procedural levels (degrees per second)")]
        public float proceduralBaseRotationSpeed = 90f;
        [Tooltip("Knives added per procedural level after the authored ones")]
        public float proceduralKnifeIncrease = 1f;
        [Tooltip("Rotation speed added per procedural level after the authored ones")]
        public float proceduralSpeedIncrease = 5f;

        public LevelData GetLevelData(int levelIndex)
        {


            int arrayIndex = levelIndex - 1;


            if (arrayIndex >= 0 && arrayIndex < levels.Count)
            {
                return levels[arrayIndex];
            }


            if (generateProceduralLevels)
            {
                return GenerateProceduralLevel(levelIndex);
            }


            if (levels.Count > 0)
            {
                return levels[levels.Count - 1];
            }


            return new LevelData();
        }

        private LevelData GenerateProceduralLevel(int levelIndex)
        {
            LevelData level = new LevelData();


            int proceduralLevelNumber = levelIndex - levels.Count;


            level.requirements.requiredKnives = Mathf.RoundToInt(proceduralBaseKnives + proceduralLevelNumber * proceduralKnifeIncrease);
            // Legacy rotation values removed; target behaviours drive rotation.
            level.requirements.maxKnives = 0;

            return level;
        }
    }
}

