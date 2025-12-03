using KnifeToss.Core.Data;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelConfigGenerator : EditorWindow
{
    private const int TotalLevels = 10;

    [MenuItem("Knife Toss/Generate Level Config")]
    public static void ShowWindow()
    {
        GetWindow<LevelConfigGenerator>("Level Config Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Configuration Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button($"Create Level Config Asset with {TotalLevels} Levels"))
        {
            CreateLevelConfig();
        }
    }

    private static void CreateLevelConfig()
    {
        LevelConfig config = ScriptableObject.CreateInstance<LevelConfig>();
        config.levels = new List<LevelData>();

        for (int i = 0; i < TotalLevels; i++)
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
        float difficulty = Mathf.InverseLerp(1f, TotalLevels, levelNumber);

        // Core requirements scale steadily
        level.requirements.requiredKnives = Mathf.RoundToInt(Mathf.Lerp(3f, 10f, difficulty));
        level.requirements.maxKnives = 0; // unlimited throws by default

        // Base rotation defaults (used by fallback path or when only one target exists)
        float baseRotationSpeed = Mathf.Lerp(70f, 180f, difficulty);
        float directionChangeInterval = Mathf.Lerp(3.5f, 1.1f, difficulty);

        level.targetGroup.targets = BuildTargets(levelNumber, difficulty, baseRotationSpeed, directionChangeInterval);
        return level;
    }

    private static List<TargetDefinition> BuildTargets(int levelNumber, float difficulty, float baseRotationSpeed, float directionChangeInterval)
    {
        int targetCount = Mathf.Clamp(1 + Mathf.FloorToInt(difficulty * 3f + 0.2f), 1, 4);
        float orbitRadius = targetCount == 1 ? 0f : Mathf.Lerp(1.25f, 3.5f, difficulty);
        float orbitSpeed = targetCount == 1 ? 0f : Mathf.Lerp(6f, 42f, difficulty);
        float baseScale = Mathf.Lerp(1.1f, 0.9f, difficulty);

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
                rotation = CreateRotationConfig(levelNumber, i, difficulty, baseRotationSpeed, directionChangeInterval),
                preStuckKnives = difficulty > 0.65f && i == 0 ? 1 : 0,
                preStuckKnifeSpacing = 360f
            };

            targets.Add(def);
        }

        return targets;
    }

    private static TargetRotationConfig CreateRotationConfig(int levelNumber, int targetIndex, float difficulty, float baseRotationSpeed, float directionChangeInterval)
    {
        // Cycle behaviours as difficulty rises.
        RotationBehaviourType behaviourType;
        if (levelNumber <= 2)
        {
            behaviourType = RotationBehaviourType.Constant;
        }
        else if (levelNumber <= 4)
        {
            behaviourType = RotationBehaviourType.Flipping;
        }
        else if (levelNumber <= 7)
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
            pulsating = new PulsatingRotationSettings
            {
                amplitude = speed * 0.35f,
                frequency = Mathf.Lerp(0.8f, 1.6f, difficulty)
            },
            flipping = new FlippingRotationSettings
            {
                flipInterval = Mathf.Lerp(2.5f, 0.9f, difficulty),
                flipProbability = Mathf.Clamp01(0.25f + difficulty * 0.55f)
            },
            acceleration = new AccelerationRotationSettings
            {
                accelerationCurve = AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.4f),
                curveLoopDuration = Mathf.Lerp(2.4f, 1.0f, difficulty)
            }
        };
    }
}





