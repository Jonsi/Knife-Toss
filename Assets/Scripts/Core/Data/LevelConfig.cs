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
        public float proceduralBaseRotationSpeed = 90f;
        public float proceduralKnifeIncrease = 1f;
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


            level.requiredKnives = Mathf.RoundToInt(proceduralBaseKnives + proceduralLevelNumber * proceduralKnifeIncrease);
            level.rotationSpeed = proceduralBaseRotationSpeed + proceduralLevelNumber * proceduralSpeedIncrease;


            level.maxKnives = 0;
            level.directionChangeInterval = 3f;
            level.randomizeDirection = true;
            level.preStuckKnives = 0;


            if (proceduralLevelNumber % 5 == 0 && proceduralLevelNumber > 0)
            {
                level.preStuckKnives = Mathf.Min(proceduralLevelNumber / 5, 3);
            }

            return level;
        }
    }
}

