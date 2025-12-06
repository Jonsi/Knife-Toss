using System;
using System.Collections.Generic;
using Core.Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Creates a refreshed LevelConfig that better reflects the shipped features.
    /// </summary>
    public sealed class LevelConfigGenerator : EditorWindow
    {
        private const string AssetPath = "Assets/Configurations/Gameplay/LevelConfig.asset";
        private const int TotalLevels = 18;

        private static readonly StageProfile[] StagePlan =
        {
            new StageProfile("Campfire Warmup", 1, 3, new Vector2Int(1, 1), new Vector2Int(5, 7))
            {
                AllowOrbiting = false,
                AllowRotationMix = false,
                EnableQuotas = false,
                EnableHitables = false,
                EnablePreStuck = false,
                BaseRotation = 60f,
                RotationVariance = 15f
            },
            new StageProfile("Juggling School", 4, 6, new Vector2Int(2, 2), new Vector2Int(7, 9))
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
            new StageProfile("Orbit Rodeo", 7, 10, new Vector2Int(2, 3), new Vector2Int(8, 11))
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
            new StageProfile("Chaos Carnival", 11, 14, new Vector2Int(3, 4), new Vector2Int(10, 13))
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
            new StageProfile("Finale Frenzy", 15, 18, new Vector2Int(3, 4), new Vector2Int(12, 15))
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

        [MenuItem("Knife Toss/Generate Level Config")]
        public static void ShowWindow()
        {
            GetWindow<LevelConfigGenerator>("Level Config Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Configuration Generator", EditorStyles.boldLabel);
            GUILayout.Space(6f);
            if (GUILayout.Button($"Create Level Config Asset with {TotalLevels} levels"))
            {
                CreateLevelConfig();
            }
        }

        private static void CreateLevelConfig()
        {
            LevelConfig config = AssetDatabase.LoadAssetAtPath<LevelConfig>(AssetPath);
            bool createdAsset = false;
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                createdAsset = true;
            }

            config.levels ??= new List<LevelData>();
            config.levels.Clear();

            for (int i = 0; i < TotalLevels; i++)
            {
                config.levels.Add(CreateLevel(i + 1));
            }

            config.generateProceduralLevels = true;
            config.proceduralBaseKnives = 12;
            config.proceduralBaseRotationSpeed = 160f;
            config.proceduralKnifeIncrease = 1.25f;
            config.proceduralSpeedIncrease = 6f;

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
            Debug.Log($"LevelConfig updated with {config.levels.Count} handcrafted levels at {AssetPath}");
        }

        private static LevelData CreateLevel(int levelNumber)
        {
            StageProfile stage = ResolveStage(levelNumber);
            float stageProgress = stage.ResolveProgress(levelNumber);
            System.Random rng = new System.Random(levelNumber * 7919);

            int knives = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(stage.BaseKnifeRange.x, stage.BaseKnifeRange.y, stageProgress)),
                3,
                20);

            List<TargetDefinition> targets = BuildTargets(stage, levelNumber, stageProgress, rng, out int quotaSum, out int totalHitables);

            LevelData level = new LevelData
            {
                knives = knives,
                targetGroup = new TargetGroupConfig { targets = targets }
            };

            return level;
        }

        private static StageProfile ResolveStage(int levelNumber)
        {
            for (int i = 0; i < StagePlan.Length; i++)
            {
                if (StagePlan[i].Contains(levelNumber))
                {
                    return StagePlan[i];
                }
            }

            return StagePlan[StagePlan.Length - 1];
        }

        private static List<TargetDefinition> BuildTargets(StageProfile stage, int levelNumber, float stageProgress, System.Random rng, out int quotaSum, out int hitableCount)
        {
            quotaSum = 0;
            hitableCount = 0;

            int targetCount = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(stage.TargetCountRange.x, stage.TargetCountRange.y, stageProgress)),
                stage.TargetCountRange.x,
                stage.TargetCountRange.y);

            float baseRadius = stage.AllowOrbiting
                ? Mathf.Lerp(1.1f, 3.2f, stageProgress)
                : 0f;

            var targets = new List<TargetDefinition>(targetCount);
            for (int i = 0; i < targetCount; i++)
            {
                float angle = targetCount == 1 ? 0f : (360f / targetCount) * i;
                float orbitRadius = stage.AllowOrbiting ? baseRadius + Mathf.Sin(i + stageProgress * Mathf.PI) * 0.35f : 0f;

                float orbitDriftPhase = stage.AllowOrbiting ? Mathf.PerlinNoise(levelNumber * 0.12f, i * 0.35f) : 0f;
                float orbitDrift = stage.AllowOrbiting ? Mathf.Lerp(-0.6f, 0.6f, orbitDriftPhase) : 0f;
                float orbitSpeed = stage.AllowOrbiting ? stage.BaseOrbitSpeed + orbitDrift : 0f;

                float rotationSpeed = stage.BaseRotation + UnityEngine.Random.Range(-stage.RotationVariance, stage.RotationVariance);
                float scalePulse = 1f + Mathf.Sin((levelNumber + i) * 0.35f) * 0.075f;
                float baseScale = Mathf.Lerp(1.05f, 0.85f, stageProgress) * scalePulse;

                TargetDefinition target = new TargetDefinition
                {
                    orbitRadius = orbitRadius,
                    startingAngle = angle,
                    orbitDegreesPerSecond = stage.AllowOrbiting ? orbitSpeed * (i % 2 == 0 ? 1f : -1f) : 0f,
                    baseScale = Mathf.Clamp(baseScale, 0.65f, 1.2f),
                    rotation = CreateRotation(stage, stageProgress, rotationSpeed, i, rng)
                };

                if (stage.EnablePreStuck)
                {
                    float chance = Mathf.Lerp(0.15f, 0.45f, stageProgress);
                    if (rng.NextDouble() < chance)
                    {
                        target.preStuckKnives = 1 + Mathf.RoundToInt(stageProgress * 2f);
                        target.preStuckKnifeSpacing = Mathf.Approximately(0f, target.preStuckKnives) ? 360f : 360f / Mathf.Max(1, target.preStuckKnives);
                    }
                }

                int quota = ResolveQuota(stage, stageProgress, rng);
                quotaSum += quota;
                target.knifeQuota = quota;

                hitableCount += AddHitables(stage, stageProgress, rng, levelNumber, i, target);
                targets.Add(target);
            }

            return targets;
        }

        private static TargetRotationConfig CreateRotation(StageProfile stage, float progress, float rotationSpeed, int targetIndex, System.Random rng)
        {
            RotationBehaviourType type = RotationBehaviourType.Constant;
            if (stage.AllowRotationMix)
            {
                int paletteSize = Enum.GetValues(typeof(RotationBehaviourType)).Length;
                int paletteIndex = Mathf.Abs(targetIndex + stage.StageSeed) % Mathf.Max(1, paletteSize);
                type = (RotationBehaviourType)paletteIndex;
                if (progress < 0.35f)
                {
                    type = RotationBehaviourType.Constant;
                }
            }

            float curvePhase = Mathf.PerlinNoise(targetIndex * 0.21f, progress * 1.3f);
            float localSpeedMod = Mathf.Lerp(0.85f, 1.4f, curvePhase);
            float signedSpeed = rotationSpeed * localSpeedMod * (targetIndex % 2 == 0 ? 1f : -1f);

            TargetRotationConfig config = new TargetRotationConfig
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

            return config;
        }

        private static int AddHitables(StageProfile stage, float progress, System.Random rng, int levelNumber, int targetIndex, TargetDefinition target)
        {
            if (!stage.EnableHitables)
            {
                return 0;
            }

            var configs = LoadHitableConfigs();
            if (configs.Count == 0)
            {
                return 0;
            }

            float chance = Mathf.Lerp(0.2f, 0.7f, progress);
            double roll = rng.NextDouble();
            if (roll > chance)
            {
                return 0;
            }

            HitableObjectConfig config = configs[rng.Next(configs.Count)];
            int count = 1 + (int)Mathf.Round(progress * 2.5f);
            float jitter = Mathf.PerlinNoise(levelNumber * 0.41f, targetIndex * 0.18f);
            float targetRadiusOffset = Mathf.Lerp(0.3f, 0.85f, jitter);

            HitableObjectPlacement placement = new HitableObjectPlacement
            {
                config = config,
                count = count,
                angularSpacing = Mathf.Approximately(count, 0) ? 360f : 360f / count,
                orientOutwards = true
            };

            target.hitableObjects.Add(placement);
            target.baseScale = Mathf.Clamp(target.baseScale - targetRadiusOffset * 0.05f, 0.6f, 1.2f);
            return count;
        }

        private static int ResolveQuota(StageProfile stage, float progress, System.Random rng)
        {
            if (!stage.EnableQuotas || stage.QuotaRange == Vector2Int.zero)
            {
                return 0;
            }

            int min = stage.QuotaRange.x;
            int max = stage.QuotaRange.y;
            float modifier = Mathf.Lerp(0f, max - min, progress);
            int quota = min + Mathf.RoundToInt(modifier);

            if (rng.NextDouble() < 0.2)
            {
                quota = Mathf.Max(min, quota - 1);
            }

            return Mathf.Clamp(quota, min, max);
        }

        private static List<HitableObjectConfig> LoadHitableConfigs()
        {
            if (cachedHitables != null)
            {
                return cachedHitables;
            }

            cachedHitables = new List<HitableObjectConfig>();
            string[] guids = AssetDatabase.FindAssets("t:HitableObjectConfig");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
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
            public readonly Vector2Int BaseKnifeRange;

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

            public StageProfile(string name, int minLevel, int maxLevel, Vector2Int targetRange, Vector2Int knifeRange)
            {
                Name = name;
                MinLevel = minLevel;
                MaxLevel = maxLevel;
                TargetCountRange = targetRange;
                BaseKnifeRange = knifeRange;
            }

            public bool Contains(int level) => level >= MinLevel && level <= MaxLevel;

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
