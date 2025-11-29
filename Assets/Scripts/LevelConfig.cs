using UnityEngine;
using System.Collections.Generic;

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
        // Level index is 1-based (Level 1, Level 2, etc.)
        // Convert to 0-based array index
        int arrayIndex = levelIndex - 1;
        
        // If we have a defined level, return it
        if (arrayIndex >= 0 && arrayIndex < levels.Count)
        {
            return levels[arrayIndex];
        }
        
        // Otherwise, generate procedurally if enabled
        if (generateProceduralLevels)
        {
            return GenerateProceduralLevel(levelIndex);
        }
        
        // Fallback: return the last defined level
        if (levels.Count > 0)
        {
            return levels[levels.Count - 1];
        }
        
        // Ultimate fallback: return default level
        return new LevelData();
    }
    
    private LevelData GenerateProceduralLevel(int levelIndex)
    {
        LevelData level = new LevelData();
        
        // Calculate how many levels beyond the defined ones
        int proceduralLevelNumber = levelIndex - levels.Count;
        
        // Gradually increase difficulty
        level.requiredKnives = Mathf.RoundToInt(proceduralBaseKnives + proceduralLevelNumber * proceduralKnifeIncrease);
        level.rotationSpeed = proceduralBaseRotationSpeed + proceduralLevelNumber * proceduralSpeedIncrease;
        
        // Keep some reasonable defaults
        level.maxKnives = 0; // Unlimited
        level.directionChangeInterval = 3f;
        level.randomizeDirection = true;
        level.preStuckKnives = 0;
        
        // Occasionally add pre-stuck knives for spikes in difficulty
        if (proceduralLevelNumber % 5 == 0 && proceduralLevelNumber > 0)
        {
            level.preStuckKnives = Mathf.Min(proceduralLevelNumber / 5, 3);
        }
        
        return level;
    }
}

