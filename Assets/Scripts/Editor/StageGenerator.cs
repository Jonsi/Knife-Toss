using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Level.Configurations.Stage;
using Game.Level.Data;
using Game.Entities.Target.Data;
using Game.Entities.Target.Data.Behaviors.Local;
using Game.Entities.Target.Data.Behaviors.World;
using Game.Entities.Collectible.Data;
using Game.Level.Catalogs;

namespace Editor.Generators
{
    public static class StageGenerator
    {
        private const int TotalStages = 10;
        private const int LevelsPerStage = 5; // Standardized for consistent progression curve
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
            Debug.Log($"Successfully updated {TotalStages} stages with Sawtooth difficulty progression.");
        }

        private static void UpdateStagesCollection()
        {
            StagesCollection collection = AssetDatabase.LoadAssetAtPath<StagesCollection>(CollectionPath);
            if (collection == null)
            {
                string dir = System.IO.Path.GetDirectoryName(CollectionPath);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                
                collection = ScriptableObject.CreateInstance<StagesCollection>();
                AssetDatabase.CreateAsset(collection, CollectionPath);
            }

            if (collection.Stages == null) collection.Stages = new List<StageConfig>();
            collection.Stages.Clear();
            
            for (int i = 1; i <= TotalStages; i++)
            {
                string path = System.IO.Path.Combine(OutputPath, $"Stage{i}.asset");
                StageConfig stage = AssetDatabase.LoadAssetAtPath<StageConfig>(path);
                if (stage != null) collection.Stages.Add(stage);
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
            
            SerializedObject so = new SerializedObject(stageConfig);
            SerializedProperty nameProp = so.FindProperty("<StageName>k__BackingField");
            if (nameProp != null)
            {
                nameProp.stringValue = $"Stage {stageIndex}";
                so.ApplyModifiedProperties();
            }

            if (stageConfig.Levels == null)
            {
                 // Handle null
            }
            else
            {
                stageConfig.Levels.Clear();
            }
            
            for (int lvl = 1; lvl <= LevelsPerStage; lvl++)
            {
                float difficulty = CalculateDifficultyScore(stageIndex, lvl);
                LevelConfig levelConfig = CreateLevel(stageIndex, lvl, difficulty);
                stageConfig.Levels.Add(levelConfig);
            }

            if (isNew) AssetDatabase.CreateAsset(stageConfig, path);
            else EditorUtility.SetDirty(stageConfig);
        }

        /// <summary>
        /// Calculates a 'Score' that determines HP, Speed, and Complexity.
        /// Implements a Sawtooth pattern: Start of new stage is easier than end of previous.
        /// </summary>
        private static float CalculateDifficultyScore(int stage, int level)
        {
            // Stage 1: 1.0 -> 3.0
            // Stage 2: 2.0 -> 4.0
            // Stage 3: 3.0 -> 5.0
            // ...
            float baseStageScore = stage; 
            float levelProgress = (level - 1) * 0.5f; // Adds 0 to 2.0
            return baseStageScore + levelProgress;
        }

        private static LevelConfig CreateLevel(int stage, int level, float difficulty)
        {
            var levelConfig = new LevelConfig();
            levelConfig.ExtraKnives = (stage == 1 && level <= 2) ? 1 : 0;

            // --- Deterministic Layout Logic ---
            // Level 1: Intro / Rest (Always 1 target, simple)
            // Level 3: Challenge (Chance of multi-target)
            // Level 5: Boss (Complex behavior or Multi-target)
            
            bool isFirstLevel = level == 1;
            bool isBoss = level == LevelsPerStage;
            int targetCount = 1;

            if (!isFirstLevel)
            {
                // Guarantee Multi-targets on specific levels for exposure
                if (stage >= 2 && level == 3) targetCount = 2; // Mid-stage challenge
                if (stage >= 4 && level == 4) targetCount = 2;
                if (stage >= 6 && level == 3) targetCount = 3;
                if (stage >= 3 && isBoss) targetCount = Random.value < 0.5f ? 2 : 1; // Boss can be multi
            }

            if (targetCount == 1)
            {
                Vector3 center = new Vector3(0, isFirstLevel ? 1.0f : 1.5f, 0); // Lower for intro
                levelConfig.Targets.Add(GenerateTarget(stage, level, difficulty, center, false));
            }
            else if (targetCount == 2)
            {
                // Layouts: Orbit OR Vertical Stack
                // Static Side-by-Side is IMPOSSIBLE if player shoots straight up.
                
                bool useOrbit = stage >= 3 && (level % 2 == 0); 
                
                if (useOrbit)
                {
                    // Orbiting Pair (Crosses center)
                    var t1 = GenerateTarget(stage, level, difficulty * 0.8f, Vector3.zero, true);
                    var t2 = GenerateTarget(stage, level, difficulty * 0.8f, Vector3.zero, true);
                    
                    float orbitSpeed = 40f + (difficulty * 5f);
                    float dist = 1.5f;
                    t1.Behaviors.AddWorldBehavior(new OrbitBehaviorConfig(0, orbitSpeed, dist));
                    t2.Behaviors.AddWorldBehavior(new OrbitBehaviorConfig(180, orbitSpeed, dist));
                    
                    levelConfig.Targets.Add(t1);
                    levelConfig.Targets.Add(t2);
                }
                else
                {
                    // Vertical Stack (Both on x=0)
                    // Hittable by straight shot
                    var t1 = GenerateTarget(stage, level, difficulty * 0.8f, new Vector3(0, 1.0f, 0), true);
                    var t2 = GenerateTarget(stage, level, difficulty * 0.8f, new Vector3(0, 3.2f, 0), true); // Higher up
                    
                    // Top target usually needs to move or bottom needs to break first.
                    // Actually, if bottom breaks, top is hittable. 
                    // Or if they have SinMovement (Side to Side).
                    
                    // Let's add slight SinMovement to both to make it dynamic but keeping them crossing center
                    // Or just keep them static vertical (easier).
                    
                    levelConfig.Targets.Add(t1);
                    levelConfig.Targets.Add(t2);
                }
            }
            else // 3 Targets
            {
                // Orbiting Trio (Ensures they cross center)
                // Static Triangle is impossible.
                
                var t1 = GenerateTarget(stage, level, difficulty * 0.6f, Vector3.zero, true);
                var t2 = GenerateTarget(stage, level, difficulty * 0.6f, Vector3.zero, true);
                var t3 = GenerateTarget(stage, level, difficulty * 0.6f, Vector3.zero, true);
                
                float orbitSpeed = 30f + (difficulty * 4f);
                float dist = 1.6f;
                
                t1.Behaviors.AddWorldBehavior(new OrbitBehaviorConfig(0, orbitSpeed, dist));
                t2.Behaviors.AddWorldBehavior(new OrbitBehaviorConfig(120, orbitSpeed, dist));
                t3.Behaviors.AddWorldBehavior(new OrbitBehaviorConfig(240, orbitSpeed, dist));
                
                levelConfig.Targets.Add(t1); levelConfig.Targets.Add(t2); levelConfig.Targets.Add(t3);
            }

            return levelConfig;
        }

        private static TargetConfig GenerateTarget(int stage, int level, float difficulty, Vector3 pos, bool isMulti)
        {
            var target = new TargetConfig();
            target.InitialOffset = pos;

            // 1. Scale
            // Easier difficulty -> Larger targets
            float baseScale = 1.0f;
            if (difficulty <= 2.0f) baseScale = 1.2f; // Very Easy
            else if (difficulty <= 4.0f) baseScale = 1.0f;
            else if (difficulty <= 7.0f) baseScale = 0.9f;
            else baseScale = 0.8f;

            if (isMulti) baseScale *= 0.65f; // Shrink for multi-target
            target.BaseScale = baseScale;

            // 2. HP
            // HP scales with difficulty but respects Scale
            float hpCalc = 3f + (difficulty * 1.5f);
            if (isMulti) hpCalc *= 0.6f;
            if (target.BaseScale < 0.9f) hpCalc *= 0.8f;
            target.KnivesToBreak = Mathf.Clamp(Mathf.RoundToInt(hpCalc), 3, 25);

            // 3. Behaviors
            ConfigureBehaviors(target, stage, level, difficulty, isMulti);

            // 4. Items (Obstacles & Collectibles)
            ConfigureItems(target, stage, level, difficulty);

            return target;
        }

        private static void ConfigureBehaviors(TargetConfig target, int stage, int level, float difficulty, bool isMulti)
        {
            bool isFirstLevel = level == 1;
            
            // Speed Calculation
            float speed = 60f + (difficulty * 12f); // 1.0->72, 10.0->180
            if (Random.value > 0.5f) speed *= -1;

            if (isFirstLevel)
            {
                // Always simple on Level 1
                target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(speed * 0.8f));
                return;
            }

            // Feature Introduction Roadmap
            // Stage 1: Constant
            // Stage 2: Pulsating
            // Stage 3: Flipping / SinMovement
            // Stage 4: Curve / Orbit
            
            // Determine Behavior Type based on Stage exposure
            if (stage == 1)
            {
                 target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(speed));
            }
            else if (stage == 2)
            {
                // Intro Pulsating
                if (level >= 3)
                    target.Behaviors.AddBehavior(new PulsatingSpinBehaviorConfig(speed, 20f, 1f));
                else
                    target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(speed));
            }
            else if (stage == 3)
            {
                // Intro Flipping
                if (level >= 3)
                     target.Behaviors.AddBehavior(new FlippingSpinBehaviorConfig(speed, 2f, 4f));
                else
                     target.Behaviors.AddBehavior(new PulsatingSpinBehaviorConfig(speed, 30f, 1.5f));
            }
            else // Stage 4+
            {
                // Mix
                float roll = Random.value;
                if (roll < 0.3f) target.Behaviors.AddBehavior(new ConstantSpinBehaviorConfig(speed * 1.2f));
                else if (roll < 0.6f) target.Behaviors.AddBehavior(new FlippingSpinBehaviorConfig(speed, 1.5f, 3f));
                else 
                {
                    // Curve (Stop & Go)
                    AnimationCurve curve = new AnimationCurve();
                    curve.AddKey(0, speed * 0.2f);
                    curve.AddKey(0.4f, speed * 1.5f);
                    curve.AddKey(1, speed * 0.2f);
                    curve.preWrapMode = WrapMode.Loop; target.Behaviors.AddBehavior(new CurveSpinBehaviorConfig(curve));
                }
            }

            // World Movement (SinMovement)
            // Explicitly force on Level 4 of Stage 2+, and Level 2 of Stage 4+
            bool forceMove = (stage >= 2 && level == 4) || (stage >= 4 && level == 2);
            bool randomMove = stage >= 5 && Random.value < 0.3f;

            if (!isMulti && (forceMove || randomMove)) // Don't add Sin to Orbiting targets
            {
                float dist = 1.0f + (difficulty * 0.1f);
                float freq = 0.5f + (difficulty * 0.05f);
                target.Behaviors.AddWorldBehavior(new SinMovementBehaviorConfig(dist, freq, Random.Range(0, 360)));
            }
        }

        private static void ConfigureItems(TargetConfig target, int stage, int level, float difficulty)
        {
            List<float> occupiedAngles = new List<float>();
            float minSeparation = 22f;

            // Obstacles
            // Difficulty 1 -> 0
            // Difficulty 5 -> ~3
            int maxObstacles = Mathf.FloorToInt((difficulty - 1.5f) * 0.8f);
            if (maxObstacles < 0) maxObstacles = 0;
            
            // Cap based on scale
            int capacity = (int)(25 * target.BaseScale);
            int safeLimit = capacity - target.KnivesToBreak - 3;
            int count = Mathf.Min(maxObstacles, safeLimit);

            for (int i = 0; i < count; i++)
            {
                float angle = GetRandomFreeAngle(occupiedAngles, minSeparation);
                if (angle >= 0)
                {
                    target.PreStuckKnives.Add(new PreStuckKnifeConfig(angle));
                    occupiedAngles.Add(angle);
                }
            }

            bool spawnApple = (level == 1) || (Random.value < 0.4f);
            if (spawnApple)
            {
                CollectibleCurrencyConfig defaultCollectible = GetDefaultCollectibleCurrencyConfig();
                if (defaultCollectible != null)
                {
                    int appleCount = (level == 1) ? Random.Range(2, 4) : 1;
                    for (int i = 0; i < appleCount; i++)
                    {
                        float angle = GetRandomFreeAngle(occupiedAngles, minSeparation);
                        if (angle >= 0)
                        {
                            target.Collectibles.Add(new CollectibleConfig(defaultCollectible, angle, 0f));
                            occupiedAngles.Add(angle);
                        }
                    }
                }
            }
        }

        private static CollectibleCurrencyConfig GetDefaultCollectibleCurrencyConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:CollectibleCurrencyConfig");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CollectibleCurrencyConfig>(path);
        }

        private static float GetRandomFreeAngle(List<float> occupied, float separation)
        {
            int attempts = 0;
            while (attempts < 50)
            {
                attempts++;
                float angle = Random.Range(0f, 360f);
                bool valid = true;
                foreach (float a in occupied)
                {
                    if (Mathf.Abs(Mathf.DeltaAngle(angle, a)) < separation)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) return angle;
            }
            return -1f;
        }
    }
}