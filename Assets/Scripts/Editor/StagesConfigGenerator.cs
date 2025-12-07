using System;
using System.Collections.Generic;
using Core.Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Creates a refreshed StagesConfig that organizes levels into stages.
    /// </summary>
    public sealed class StagesConfigGenerator : EditorWindow
    {
        private const string AssetPath = "Assets/Configurations/Gameplay/StagesConfig.asset";

        private static readonly StageProfile[] StagePlan =
        {
            new StageProfile("Campfire Warmup", 1, 3, new Vector2Int(1, 1))
            {
                AllowOrbiting = false,
                AllowRotationMix = false,
                EnableQuotas = false,
                EnableHitables = false,
                EnablePreStuck = false,
                BaseRotation = 60f,
                RotationVariance = 15f
            },
            new StageProfile("Juggling School", 4, 6, new Vector2Int(2, 2))
            {
                AllowOrbiting = true,
                AllowRotationMix = true,
                EnableQuotas = true,
                EnableHitables = false,
                EnablePreStuck = true,
                BaseRotation = 90f,
                RotationVariance = 35f,
                BaseOrbitSpeed = 30f,
                QuotaRange = new Vector2Int(1, 3)
            },
            new StageProfile("Orbit Rodeo", 7, 10, new Vector2Int(2, 3))
            {
                AllowOrbiting = true,
                AllowRotationMix = true,
                EnableQuotas = true,
                EnableHitables = true,
                EnablePreStuck = true,
                BaseRotation = 120f,
                RotationVariance = 45f,
                BaseOrbitSpeed = 45f,
                QuotaRange = new Vector2Int(2, 4)
            },
            new StageProfile("Chaos Carnival", 11, 14, new Vector2Int(3, 4))
            {
                AllowOrbiting = true,
                AllowRotationMix = true,
                EnableQuotas = true,
                EnableHitables = true,
                EnablePreStuck = true,
                BaseRotation = 150f,
                RotationVariance = 60f,
                BaseOrbitSpeed = 60f,
                QuotaRange = new Vector2Int(3, 5)
            },
            new StageProfile("Finale Frenzy", 15, 18, new Vector2Int(3, 4))
            {
                AllowOrbiting = true,
                AllowRotationMix = true,
                EnableQuotas = true,
                EnableHitables = true,
                EnablePreStuck = true,
                BaseRotation = 180f,
                RotationVariance = 70f,
                BaseOrbitSpeed = 75f,
                QuotaRange = new Vector2Int(4, 6)
            }
        };

        private static List<HitableObjectConfig> cachedHitables;

        [MenuItem("Knife Toss/Generate Stages Config")]
        public static void ShowWindow()
        {
            GetWindow<StagesConfigGenerator>("Stages Config Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Stages Configuration Generator", EditorStyles.boldLabel);
            GUILayout.Space(6f);
            int totalLevels = CalculateTotalLevels();
            if (GUILayout.Button($"Create Stages Config Asset with {StagePlan.Length} stages ({totalLevels} total levels)"))
            {
                CreateStagesConfig();
            }
        }

        private static int CalculateTotalLevels()
        {
            int total = 0;
            foreach (StageProfile stage in StagePlan)
            {
                total += stage.MaxLevel - stage.MinLevel + 1;
            }
            return total;
        }

        private static void CreateStagesConfig()
        {
            StagesConfig config = AssetDatabase.LoadAssetAtPath<StagesConfig>(AssetPath);
            bool createdAsset = config == null;
            if (createdAsset)
            {
                config = ScriptableObject.CreateInstance<StagesConfig>();
            }

            config.stages ??= new List<StageData>();
            config.stages.Clear();

            for (int stageIndex = 0; stageIndex < StagePlan.Length; stageIndex++)
            {
                StageProfile stageProfile = StagePlan[stageIndex];
                StageData stageData = new StageData
                {
                    stageName = stageProfile.Name,
                    levels = new List<LevelData>()
                };

                for (int levelNumber = stageProfile.MinLevel; levelNumber <= stageProfile.MaxLevel; levelNumber++)
                {
                    stageData.levels.Add(CreateLevel(levelNumber, stageProfile));
                }

                config.stages.Add(stageData);
            }

            if (createdAsset)
            {
                AssetDatabase.CreateAsset(config, AssetPath);
            }
            else
            {
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = config;
        }

        private static LevelData CreateLevel(int levelNumber, StageProfile stage)
        {
            float stageProgress = stage.ResolveProgress(levelNumber);
            System.Random rng = new System.Random(levelNumber * 7919);

            List<TargetDefinition> targets = BuildTargets(stage, levelNumber, stageProgress, rng);

            return new LevelData
            {
                knives = 0,
                targetGroup = new TargetGroupConfig { targets = targets }
            };
        }

        private static List<TargetDefinition> BuildTargets(StageProfile stage, int levelNumber, float stageProgress, System.Random rng)
        {
            int targetCount = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(stage.TargetCountRange.x, stage.TargetCountRange.y, stageProgress)),
                stage.TargetCountRange.x,
                stage.TargetCountRange.y);

            float baseRadius = stage.AllowOrbiting ? Mathf.Lerp(1.1f, 3.2f, stageProgress) : 0f;
            var targets = new List<TargetDefinition>(targetCount);

            for (int i = 0; i < targetCount; i++)
            {
                targets.Add(CreateTarget(stage, levelNumber, stageProgress, i, targetCount, baseRadius, rng));
            }

            return targets;
        }

        private static TargetDefinition CreateTarget(StageProfile stage, int levelNumber, float stageProgress, int targetIndex, int targetCount, float baseRadius, System.Random rng)
        {
            float angle = targetCount == 1 ? 0f : (360f / targetCount) * targetIndex;
            float orbitRadius = stage.AllowOrbiting ? baseRadius + Mathf.Sin(targetIndex + stageProgress * Mathf.PI) * 0.35f : 0f;

            float orbitDriftPhase = stage.AllowOrbiting ? Mathf.PerlinNoise(levelNumber * 0.12f, targetIndex * 0.35f) : 0f;
            float orbitDrift = stage.AllowOrbiting ? Mathf.Lerp(-0.6f, 0.6f, orbitDriftPhase) : 0f;
            float orbitSpeed = stage.AllowOrbiting ? stage.BaseOrbitSpeed + orbitDrift : 0f;

            float rotationSpeed = stage.BaseRotation + UnityEngine.Random.Range(-stage.RotationVariance, stage.RotationVariance);
            float scalePulse = 1f + Mathf.Sin((levelNumber + targetIndex) * 0.35f) * 0.075f;
            float baseScale = Mathf.Lerp(1.05f, 0.85f, stageProgress) * scalePulse;

            TargetDefinition target = new TargetDefinition
            {
                orbitRadius = orbitRadius,
                startingAngle = angle,
                orbitDegreesPerSecond = stage.AllowOrbiting ? orbitSpeed * (targetIndex % 2 == 0 ? 1f : -1f) : 0f,
                baseScale = Mathf.Clamp(baseScale, 0.65f, 1.2f),
                rotation = CreateRotation(stage, stageProgress, rotationSpeed, targetIndex, rng),
                knivesToBreak = ResolveQuota(stage, stageProgress, rng, targetIndex, targetCount, levelNumber)
            };

            if (stage.EnablePreStuck)
            {
                ApplyPreStuckKnives(target, stage, stageProgress, rng);
            }

            if (stage.EnableHitables)
            {
                AddHitables(stage, stageProgress, rng, levelNumber, targetIndex, target);
            }

            return target;
        }

        private static void ApplyPreStuckKnives(TargetDefinition target, StageProfile stage, float progress, System.Random rng)
        {
            float chance = Mathf.Lerp(0.15f, 0.45f, progress);
            if (rng.NextDouble() < chance)
            {
                target.preStuckKnives = 1 + Mathf.RoundToInt(progress * 2f);
                target.preStuckKnifeSpacing = target.preStuckKnives > 0 ? 360f / target.preStuckKnives : 360f;
            }
        }

        private static void AddHitables(StageProfile stage, float progress, System.Random rng, int levelNumber, int targetIndex, TargetDefinition target)
        {
            var configs = LoadHitableConfigs();
            if (configs.Count == 0)
            {
                return;
            }

            float chance = Mathf.Lerp(0.2f, 0.7f, progress);
            if (rng.NextDouble() > chance)
            {
                return;
            }

            HitableObjectConfig config = configs[rng.Next(configs.Count)];
            int count = 1 + (int)Mathf.Round(progress * 2.5f);
            float jitter = Mathf.PerlinNoise(levelNumber * 0.41f, targetIndex * 0.18f);
            float targetRadiusOffset = Mathf.Lerp(0.3f, 0.85f, jitter);

            target.hitableObjects.Add(new HitableObjectPlacement
            {
                config = config,
                count = count,
                angularSpacing = count > 0 ? 360f / count : 360f,
                orientOutwards = true
            });

            target.baseScale = Mathf.Clamp(target.baseScale - targetRadiusOffset * 0.05f, 0.6f, 1.2f);
        }

        private static TargetRotationConfig CreateRotation(StageProfile stage, float progress, float rotationSpeed, int targetIndex, System.Random rng)
        {
            RotationBehaviourType type = DetermineRotationType(stage, progress, targetIndex);
            float curvePhase = Mathf.PerlinNoise(targetIndex * 0.21f, progress * 1.3f);
            float localSpeedMod = Mathf.Lerp(0.85f, 1.4f, curvePhase);
            float signedSpeed = rotationSpeed * localSpeedMod * (targetIndex % 2 == 0 ? 1f : -1f);

            return new TargetRotationConfig
            {
                behaviourType = type,
                speed = signedSpeed,
                pulsating = new PulsatingRotationSettings
                {
                    amplitude = Mathf.Abs(signedSpeed) * Mathf.Lerp(0.25f, 0.6f, progress),
                    frequency = Mathf.Lerp(0.7f, 2f, progress + curvePhase * 0.2f)
                },
                flipping = new FlippingRotationSettings
                {
                    flipInterval = Mathf.Lerp(2.6f, 0.85f, progress),
                    flipProbability = Mathf.Clamp01(0.25f + progress * 0.6f)
                },
                acceleration = new AccelerationRotationSettings
                {
                    accelerationCurve = AnimationCurve.EaseInOut(0f, 0.55f + curvePhase * 0.2f, 1f, 1.45f),
                    curveLoopDuration = Mathf.Lerp(2.4f, 0.8f, progress)
                }
            };
        }

        private static RotationBehaviourType DetermineRotationType(StageProfile stage, float progress, int targetIndex)
        {
            if (!stage.AllowRotationMix || progress < 0.35f)
            {
                return RotationBehaviourType.Constant;
            }

            int paletteSize = Enum.GetValues(typeof(RotationBehaviourType)).Length;
            int paletteIndex = Mathf.Abs(targetIndex + stage.StageSeed) % Mathf.Max(1, paletteSize);
            return (RotationBehaviourType)paletteIndex;
        }

        private static int ResolveQuota(StageProfile stage, float progress, System.Random rng, int targetIndex, int targetCount, int levelNumber)
        {
            if (!stage.EnableQuotas || stage.QuotaRange == Vector2Int.zero)
            {
                return 1;
            }

            int baseMin = Mathf.Max(1, stage.QuotaRange.x);
            int baseMax = Mathf.Max(baseMin, stage.QuotaRange.y);

            float levelProgression = progress;
            float targetPosition = targetCount > 1 ? (float)targetIndex / (targetCount - 1) : 0.5f;
            
            float perlinNoise = Mathf.PerlinNoise(levelNumber * 0.17f, targetIndex * 0.23f);
            float noiseVariation = (perlinNoise - 0.5f) * 0.5f;
            
            float baseQuota = Mathf.Lerp(baseMin, baseMax, levelProgression);
            
            float targetPattern = Mathf.Sin(targetPosition * Mathf.PI);
            float targetOffset = targetPattern * 0.4f;
            
            float incrementalBoost = levelNumber * 0.1f;
            float finalQuota = baseQuota + targetOffset + noiseVariation + incrementalBoost;
            
            int quota = Mathf.RoundToInt(finalQuota);
            
            if (targetCount > 1)
            {
                if (targetIndex == 0)
                {
                    quota = Mathf.Max(baseMin, quota - 1);
                }
                else if (targetIndex == targetCount - 1)
                {
                    quota = Mathf.Min(baseMax + 1, quota + 1);
                }
                else
                {
                    float midVariation = Mathf.Sin((targetIndex / (float)targetCount) * Mathf.PI * 2f) * 0.3f;
                    quota += Mathf.RoundToInt(midVariation);
                }
            }
            
            if (rng.NextDouble() < 0.2f)
            {
                int randomAdjust = rng.NextDouble() < 0.6f ? -1 : 1;
                quota += randomAdjust;
            }
            
            return Mathf.Clamp(quota, baseMin, baseMax + 1);
        }

        private static List<HitableObjectConfig> LoadHitableConfigs()
        {
            if (cachedHitables != null)
            {
                return cachedHitables;
            }

            cachedHitables = new List<HitableObjectConfig>();
            string[] guids = AssetDatabase.FindAssets("t:HitableObjectConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HitableObjectConfig config = AssetDatabase.LoadAssetAtPath<HitableObjectConfig>(path);
                if (config != null)
                {
                    cachedHitables.Add(config);
                }
            }

            return cachedHitables;
        }

        private sealed class StageProfile
        {
            public readonly string Name;
            public readonly int MinLevel;
            public readonly int MaxLevel;
            public readonly Vector2Int TargetCountRange;

            public bool AllowOrbiting = true;
            public bool AllowRotationMix = true;
            public bool EnableHitables = true;
            public bool EnablePreStuck = true;
            public bool EnableQuotas = true;
            public float BaseRotation = 120f;
            public float RotationVariance = 35f;
            public float BaseOrbitSpeed = 30f;
            public Vector2Int QuotaRange = new Vector2Int(1, 4);

            public int StageSeed => Name.GetHashCode();

            public StageProfile(string name, int minLevel, int maxLevel, Vector2Int targetRange)
            {
                Name = name;
                MinLevel = minLevel;
                MaxLevel = maxLevel;
                TargetCountRange = targetRange;
            }

            public float ResolveProgress(int level)
            {
                if (MaxLevel == MinLevel)
                {
                    return 1f;
                }
                return Mathf.InverseLerp(MinLevel, MaxLevel, level);
            }
        }
    }
}
