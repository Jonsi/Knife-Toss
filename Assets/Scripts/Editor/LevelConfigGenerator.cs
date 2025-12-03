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

        for (int i = 0; i < 30; i++)
        {
            config.levels.Add(CreateLevel(i + 1));
        }

        config.generateProceduralLevels = true;
        config.proceduralBaseKnives = 12;
        config.proceduralBaseRotationSpeed = 150f;
        config.proceduralKnifeIncrease = 1f;
        config.proceduralSpeedIncrease = 5f;


        string path = "Assets/LevelConfig.asset";
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;

        Debug.Log($"Created LevelConfig asset at {path} with {config.levels.Count} levels!");
    }

    private static LevelData CreateLevel(int levelNumber)
    {
        LevelData level = new LevelData();

        // Core requirements scale steadily
        level.requirements.requiredKnives = Mathf.Max(3, 3 + levelNumber / 2);
        level.requirements.maxKnives = 0; // unlimited throws by default

        // Base rotation defaults (used by fallback path or when only one target exists)
        float baseRotationSpeed = 60f + levelNumber * 3f;
        float directionChangeInterval = Mathf.Max(1.75f, 3.5f - levelNumber * 0.04f);

        level.targetGroup.targets = BuildTargets(levelNumber, baseRotationSpeed, directionChangeInterval);
        return level;
    }

    private static List<TargetDefinition> BuildTargets(int levelNumber, float baseRotationSpeed, float directionChangeInterval)
    {
        int targetCount = ResolveTargetCount(levelNumber);
        float orbitRadius = targetCount == 1 ? 0f : Mathf.Lerp(1.5f, 3.5f, Mathf.Clamp01(levelNumber / 30f));
        float orbitSpeed = targetCount == 1 ? 0f : Mathf.Lerp(10f, 35f, Mathf.Clamp01(levelNumber / 30f));
        float baseScale = 1f;

        var targets = new List<TargetDefinition>();
        for (int i = 0; i < targetCount; i++)
        {
            float angle = (360f / targetCount) * i;
            TargetDefinition def = new TargetDefinition
            {
                orbitRadius = orbitRadius,
                startingAngle = angle,
                orbitDegreesPerSecond = orbitSpeed * (i % 2 == 0 ? 1f : -1f),
                baseScale = baseScale,
                rotation = CreateRotationConfig(levelNumber, i, baseRotationSpeed, directionChangeInterval)
            };

            targets.Add(def);
        }

        return targets;
    }

    private static int ResolveTargetCount(int levelNumber)
    {
        if (levelNumber < 6) return 1;
        if (levelNumber < 12) return 2;
        if (levelNumber < 20) return 3;
        return 4;
    }

    private static TargetRotationConfig CreateRotationConfig(int levelNumber, int targetIndex, float baseRotationSpeed, float directionChangeInterval)
    {
        // Cycle behaviours as difficulty rises.
        RotationBehaviourType behaviourType;
        if (levelNumber < 6)
        {
            behaviourType = RotationBehaviourType.Constant;
        }
        else if (levelNumber < 12)
        {
            behaviourType = RotationBehaviourType.Flipping;
        }
        else if (levelNumber < 20)
        {
            behaviourType = RotationBehaviourType.Pulsating;
        }
        else
        {
            behaviourType = targetIndex % 2 == 0 ? RotationBehaviourType.AccelerationCurve : RotationBehaviourType.Flipping;
        }

        float speed = baseRotationSpeed * Mathf.Lerp(1.0f, 1.5f, Mathf.Clamp01(levelNumber / 30f));

        return new TargetRotationConfig
        {
            behaviourType = behaviourType,
            speed = speed * (targetIndex % 2 == 0 ? 1f : -1f),
            pulsateAmplitude = speed * 0.35f,
            pulsateFrequency = Mathf.Lerp(0.8f, 1.6f, Mathf.Clamp01(levelNumber / 30f)),
            flipInterval = Mathf.Max(0.8f, directionChangeInterval - targetIndex * 0.15f),
            flipProbability = Mathf.Clamp01(0.35f + levelNumber * 0.015f),
            accelerationCurve = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.4f),
            curveLoopDuration = Mathf.Max(1.2f, 3f - levelNumber * 0.04f)
        };
    }
}





