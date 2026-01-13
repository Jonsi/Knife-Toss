using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Level.Configurations.Stage;
using Game.Level.Data;
using Game.Entities.Target.Data;
using Game.Entities.Target.Data.Behaviors.Local;

using Game.Level.Catalogs;

namespace Editor.Generators
{
    public static class StageGenerator
    {
        private const int TotalStages = 10;
        private const int LevelsPerStage = 10;
        private const string OutputPath = "Assets/Configurations/Stages";
        private const string CollectionPath = "Assets/Configurations/Gameplay/StagesCollection.asset";

        [MenuItem("Knife Toss/Generate Stages")]
        public static void GenerateStages()
        {
            if (!System.IO.Directory.Exists(OutputPath))
            {
                System.IO.Directory.CreateDirectory(OutputPath);
            }

            for (int i = 1; i <= TotalStages; i++)
            {
                GenerateStage(i);
            }

            UpdateStagesCollection();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Successfully generated {TotalStages} stages in {OutputPath} and updated collection.");
        }

        private static void UpdateStagesCollection()
        {
            StagesCollection collection = AssetDatabase.LoadAssetAtPath<StagesCollection>(CollectionPath);
            if (collection == null)
            {
                // Try to create the directory if it doesn't exist
                string dir = System.IO.Path.GetDirectoryName(CollectionPath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                
                collection = ScriptableObject.CreateInstance<StagesCollection>();
                AssetDatabase.CreateAsset(collection, CollectionPath);
            }

            // Since we made the setter public, we can modify the list directly.
            // But if it's null, initialize it.
            if (collection.Stages == null) collection.Stages = new List<StageConfig>();
            
            collection.Stages.Clear();
            
            for (int i = 1; i <= TotalStages; i++)
            {
                string fileName = $"Stage{i}.asset";
                string path = System.IO.Path.Combine(OutputPath, fileName);
                StageConfig stage = AssetDatabase.LoadAssetAtPath<StageConfig>(path);
                if (stage != null)
                {
                    collection.Stages.Add(stage);
                }
            }
            EditorUtility.SetDirty(collection);
        }

        private static void GenerateStage(int stageIndex)
        {
            string fileName = $"Stage{stageIndex}.asset";
            string path = System.IO.Path.Combine(OutputPath, fileName);
            
            StageConfig stageConfig = AssetDatabase.LoadAssetAtPath<StageConfig>(path);
            bool isNew = false;

            if (stageConfig == null)
            {
                stageConfig = ScriptableObject.CreateInstance<StageConfig>();
                isNew = true;
            }
            
            // Update Name
            SerializedObject so = new SerializedObject(stageConfig);
            SerializedProperty nameProp = so.FindProperty("<StageName>k__BackingField");
            if (nameProp != null)
            {
                nameProp.stringValue = $"Stage {stageIndex}";
                so.ApplyModifiedProperties();
            }

            // Populate Levels
            // We modify the existing list to preserve other fields like LayoutSettings
            if (stageConfig.Levels == null)
            {
                // Fallback if list is null (should not happen with default init)
                 SerializedProperty levelsProp = so.FindProperty("<Levels>k__BackingField");
                 // If backing field not found, try standard name (sometimes Unity changes this)
                 if (levelsProp == null) levelsProp = so.FindProperty("Levels");
                 
                 // If we still can't find it or it's just null in C#, we might be in trouble for a private setter.
                 // But typically it's initialized.
                 // We'll rely on the default constructor having run or serialization populating it.
                 // If it is strictly null, we can't easily fix it without reflection due to private set.
            }
            
            stageConfig.Levels.Clear();
            
            int levelsCount = Random.Range(4, 7); // 4 to 6 levels per stage
            for (int lvl = 1; lvl <= levelsCount; lvl++)
            {
                LevelConfig levelConfig = CreateLevel(stageIndex, lvl);
                stageConfig.Levels.Add(levelConfig);
            }

            if (isNew)
            {
                AssetDatabase.CreateAsset(stageConfig, path);
            }
            else
            {
                EditorUtility.SetDirty(stageConfig);
            }
        }

        private static LevelConfig CreateLevel(int stageIndex, int levelIndex)
        {
            var level = new LevelConfig();
            level.ExtraKnives = (stageIndex == 1 && levelIndex <= 3) ? 1 : 0;

            var target = new TargetConfig();
            target.KnivesToBreak = CalculateHP(stageIndex, levelIndex);
            target.BaseScale = CalculateScale(stageIndex, levelIndex);
            target.InitialOffset = new Vector3(0, 1.5f, 0); // Raised slightly above center

            // Behaviors
            ConfigureBehavior(target, stageIndex, levelIndex);

            // Obstacles
            ConfigureObstacles(target, stageIndex, levelIndex);

            level.Targets.Add(target);
            return level;
        }

        private static int CalculateHP(int stage, int level)
        {
            // Base HP calculation
            // Stage 1: 3-5
            // Stage 10: 15-20
            float baseHp = 3 + (stage * 1.5f) + (level * 0.2f);
            
            // Scale adjustment: Smaller targets should have less HP
            float scale = CalculateScale(stage, level);
            if (scale < 0.9f) baseHp *= 0.8f;

            return Mathf.Clamp(Mathf.RoundToInt(baseHp), 3, 20);
        }

        private static float CalculateScale(int stage, int level)
        {
            // Stage 4+ starts shrinking targets occasionally
            if (stage >= 8) return Random.Range(0.6f, 0.9f);
            if (stage >= 5) return Random.Range(0.8f, 1.0f);
            return 1.0f;
        }

        private static void ConfigureBehavior(TargetConfig target, int stage, int level)
        {
            float difficulty = stage + (level / 10f);
            bool isBoss = (level == 10);

            // Tutorial (Stage 1, Levels 1-3)
            if (stage == 1 && level <= 3)
            {
                target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(40f));
                return;
            }

            float roll = Random.value;
            
            if (difficulty < 3) // Easy (Stage 1-2)
            {
                float speed = Random.Range(50f, 90f) * (Random.value > 0.5f ? 1 : -1);
                target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(speed));
            }
            else if (difficulty < 6) // Normal (Stage 3-5)
            {
                if (roll < 0.4f) // Fast Constant
                {
                    target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(Random.Range(90f, 120f)));
                }
                else // Flipping
                {
                    target.Behaviors.AddBehavior(new FlippingSpinBehaviorConfig(80f, 2f, 4f));
                }
            }
            else // Hard/Insane (Stage 6+)
            {
                if (roll < 0.3f && !isBoss) // Very Fast Constant
                {
                     target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(Random.Range(120f, 180f)));
                }
                else if (roll < 0.7f) // Aggressive Flipping
                {
                    target.Behaviors.AddBehavior(new FlippingSpinBehaviorConfig(120f, 1f, 2.5f));
                }
                else // Curve (Stop & Go)
                {
                     AnimationCurve curve = new AnimationCurve();
                     // Create a "Stop and Go" curve
                     curve.AddKey(new Keyframe(0f, 0f));
                     curve.AddKey(new Keyframe(0.2f, 200f)); // Burst
                     curve.AddKey(new Keyframe(0.5f, 50f));  // Slow
                     curve.AddKey(new Keyframe(0.8f, 200f)); // Burst
                     curve.AddKey(new Keyframe(1f, 0f));
                     curve.preWrapMode = WrapMode.Loop;
                     curve.postWrapMode = WrapMode.Loop;
                     target.Behaviors.AddBehavior(new CurveSpinBehaviorConfig(curve));
                }
            }
        }

        private static void ConfigureObstacles(TargetConfig target, int stage, int level)
        {
            // Avoid obstacles on early levels of any stage unless it's later stages
            if (level <= 2 && stage < 5) return;

            int maxObstacles = Mathf.Min(6, Mathf.CeilToInt(stage * 0.7f));
            int count = Random.Range(0, maxObstacles + 1);

            // Safe capacity check (Constraint #1)
            int capacity = (int)(25 * target.BaseScale);
            int safeLimit = capacity - target.KnivesToBreak - 2; // -2 buffer
            count = Mathf.Min(count, safeLimit);

            if (count <= 0) return;

            List<float> angles = new List<float>();
            int attempts = 0;
            while (angles.Count < count && attempts < 50)
            {
                attempts++;
                float angle = Random.Range(0f, 360f);
                bool valid = true;
                foreach (float a in angles)
                {
                    if (Mathf.Abs(Mathf.DeltaAngle(angle, a)) < 25f) // 25 degrees separation (Constraint #3)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) angles.Add(angle);
            }

            foreach (float a in angles)
            {
                target.PreStuckKnives.Add(new PreStuckKnifeConfig(a));
            }
        }
    }
}
