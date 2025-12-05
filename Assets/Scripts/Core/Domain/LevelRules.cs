using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Domain
{



    public readonly struct LevelRules
    {
        public int KnifeCount { get; }
        public float RotationSpeed { get; }
        public float DirectionChangeInterval { get; }
        public bool RandomizeDirection { get; }
        public int TargetCount { get; }
        public LevelObjectivesConfig Objectives { get; }
        public int TotalHitableObjects { get; }

        /// <summary>
        /// Performs the LevelRules operation.
        /// </summary>
        public LevelRules(
            int knifeCount,
            float rotationSpeed,
            float directionChangeInterval,
            bool randomizeDirection,
            int targetCount,
            LevelObjectivesConfig objectives,
            int totalHitableObjects)
        {
            KnifeCount = Mathf.Max(1, knifeCount);
            RotationSpeed = rotationSpeed;
            DirectionChangeInterval = Mathf.Max(0f, directionChangeInterval);
            RandomizeDirection = randomizeDirection;
            TargetCount = Mathf.Max(1, targetCount);
            Objectives = objectives ?? new LevelObjectivesConfig();
            TotalHitableObjects = Mathf.Max(0, totalHitableObjects);
        }

        /// <summary>
        /// Performs the FromLevelData operation.
        /// </summary>
        public static LevelRules FromLevelData(LevelData data)
        {
            if (data == null)
            {
                LevelObjectivesConfig fallbackObjectives = BuildFallbackObjectives(5);
                return new LevelRules(5, 90f, 3f, true, 1, fallbackObjectives, 0);
            }

            LevelObjectivesConfig objectives = CloneObjectives(data.objectives);
            TargetGroupConfig targetGroup = data.targetGroup ?? new TargetGroupConfig();

            int targetCount = targetGroup.targets != null && targetGroup.targets.Count > 0 ? targetGroup.targets.Count : 1;
            int totalHitables = CountTotalHitableObjects(targetGroup);
            int knifeCount = Mathf.Max(1, data.knives);
            EnsureOneStarObjective(objectives, knifeCount);

            float rotationSpeed = ResolveFallbackRotationSpeed(targetGroup);
            float directionChangeInterval = 0f;
            bool randomizeDirection = false;

            return new LevelRules(
                knifeCount,
                rotationSpeed,
                directionChangeInterval,
                randomizeDirection,
                targetCount,
                objectives,
                totalHitables);
        }

        /// <summary>
        /// Performs the ResolveFallbackRotationSpeed operation.
        /// </summary>
        private static float ResolveFallbackRotationSpeed(TargetGroupConfig targetGroup)
        {
            if (targetGroup?.targets != null && targetGroup.targets.Count > 0)
            {
                TargetRotationConfig rotation = targetGroup.targets[0]?.rotation;
                if (rotation != null)
                {
                    return Mathf.Abs(rotation.speed);
                }
            }

            return 90f;
        }

        /// <summary>
        /// Builds a fallback objective set for simple levels.
        /// </summary>
        private static LevelObjectivesConfig BuildFallbackObjectives(int requiredKnives)
        {
            LevelObjectivesConfig objectives = new LevelObjectivesConfig();
            objectives.oneStar.objectives.Add(new ObjectiveDefinition
            {
                type = ObjectiveType.StickKnives,
                requireAll = false,
                count = Mathf.Max(1, requiredKnives)
            });
            return objectives;
        }

        /// <summary>
        /// Ensures the primary tier has at least one objective to gate completion.
        /// </summary>
        private static void EnsureOneStarObjective(LevelObjectivesConfig objectives, int knifeCount)
        {
            if (objectives == null)
            {
                return;
            }

            objectives.oneStar ??= new LevelObjectiveTier();
            objectives.oneStar.objectives ??= new List<ObjectiveDefinition>();

            if (objectives.oneStar.objectives.Count == 0)
            {
                objectives.oneStar.objectives.Add(new ObjectiveDefinition
                {
                    type = ObjectiveType.StickKnives,
                    requireAll = true,
                    count = knifeCount
                });
            }
        }

        /// <summary>
        /// Counts how many hitable objects are authored on the level.
        /// </summary>
        private static int CountTotalHitableObjects(TargetGroupConfig targetGroup)
        {
            if (targetGroup?.targets == null || targetGroup.targets.Count == 0)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < targetGroup.targets.Count; i++)
            {
                TargetDefinition target = targetGroup.targets[i];
                if (target?.hitableObjects == null)
                {
                    continue;
                }

                for (int j = 0; j < target.hitableObjects.Count; j++)
                {
                    HitableObjectPlacement placement = target.hitableObjects[j];
                    if (placement != null && placement.count > 0)
                    {
                        total += placement.count;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// Creates a runtime copy of the objectives so the asset stays untouched.
        /// </summary>
        private static LevelObjectivesConfig CloneObjectives(LevelObjectivesConfig source)
        {
            LevelObjectivesConfig clone = new LevelObjectivesConfig();
            clone.oneStar.objectives = CloneObjectiveList(source?.oneStar?.objectives);
            clone.twoStar.objectives = CloneObjectiveList(source?.twoStar?.objectives);
            clone.threeStar.objectives = CloneObjectiveList(source?.threeStar?.objectives);
            return clone;
        }

        private static List<ObjectiveDefinition> CloneObjectiveList(List<ObjectiveDefinition> source)
        {
            if (source == null || source.Count == 0)
            {
                return new List<ObjectiveDefinition>();
            }

            List<ObjectiveDefinition> clone = new List<ObjectiveDefinition>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                ObjectiveDefinition objective = source[i];
                if (objective == null)
                {
                    continue;
                }

                clone.Add(new ObjectiveDefinition
                {
                    type = objective.type,
                    requireAll = objective.requireAll,
                    count = objective.count
                });
            }

            return clone;
        }
    }
}
