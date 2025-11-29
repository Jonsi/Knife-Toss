using KnifeToss.Core.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelConfigGenerator : EditorWindow
{
    [MenuItem("Knife Toss/Generate Level Config")]
    public static void ShowWindow()
    {
        GetWindow<LevelConfigGenerator>("Level Config Generator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Level Configuration Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Level Config Asset with 30 Levels"))
        {
            CreateLevelConfig();
        }
    }
    
    private static void CreateLevelConfig()
    {
        LevelConfig config = ScriptableObject.CreateInstance<LevelConfig>();
        config.levels = new List<LevelData>();
        
        // Generate 30 hand-crafted levels with gradual difficulty increase
        for (int i = 0; i < 30; i++)
        {
            LevelData level = new LevelData();
            int levelNumber = i + 1;
            
            // Base difficulty progression
            level.requiredKnives = Mathf.Max(3, 3 + (levelNumber / 3));
            level.rotationSpeed = 60f + (levelNumber * 3f);
            level.directionChangeInterval = Mathf.Max(2f, 4f - (levelNumber * 0.05f));
            level.randomizeDirection = true;
            level.maxKnives = 0; // Unlimited for first 30 levels
            level.preStuckKnives = 0;
            level.preStuckKnifeSpacing = 45f;
            
            // Special adjustments for specific levels
            if (levelNumber == 1)
            {
                level.requiredKnives = 3;
                level.rotationSpeed = 60f;
                level.directionChangeInterval = 5f;
            }
            else if (levelNumber == 5)
            {
                level.requiredKnives = 5;
                level.rotationSpeed = 75f;
            }
            else if (levelNumber == 10)
            {
                level.requiredKnives = 6;
                level.rotationSpeed = 90f;
                level.preStuckKnives = 1;
            }
            else if (levelNumber == 15)
            {
                level.requiredKnives = 7;
                level.rotationSpeed = 105f;
                level.directionChangeInterval = 3f;
            }
            else if (levelNumber == 20)
            {
                level.requiredKnives = 8;
                level.rotationSpeed = 120f;
                level.preStuckKnives = 2;
            }
            else if (levelNumber == 25)
            {
                level.requiredKnives = 10;
                level.rotationSpeed = 135f;
                level.directionChangeInterval = 2.5f;
            }
            else if (levelNumber == 30)
            {
                level.requiredKnives = 12;
                level.rotationSpeed = 150f;
                level.preStuckKnives = 3;
                level.directionChangeInterval = 2f;
            }
            
            config.levels.Add(level);
        }
        
        // Set procedural generation settings
        config.generateProceduralLevels = true;
        config.proceduralBaseKnives = 12;
        config.proceduralBaseRotationSpeed = 150f;
        config.proceduralKnifeIncrease = 1f;
        config.proceduralSpeedIncrease = 5f;
        
        // Save asset
        string path = "Assets/LevelConfig.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
        
        Debug.Log($"Created LevelConfig asset at {path} with {config.levels.Count} levels!");
    }
}


