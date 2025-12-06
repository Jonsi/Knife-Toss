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
            int totalHitableObjects)
        {
            KnifeCount = Mathf.Max(1, knifeCount);
            RotationSpeed = rotationSpeed;
            DirectionChangeInterval = Mathf.Max(0f, directionChangeInterval);
            RandomizeDirection = randomizeDirection;
            TargetCount = Mathf.Max(1, targetCount);
            TotalHitableObjects = Mathf.Max(0, totalHitableObjects);
        }

        /// <summary>
        /// Performs the FromLevelData operation.
        /// </summary>
        public static LevelRules FromLevelData(LevelData data)
        {
            if (data == null)
            {
                return new LevelRules(5, 90f, 3f, true, 1, 0);
            }

            TargetGroupConfig targetGroup = data.targetGroup ?? new TargetGroupConfig();

            int targetCount = targetGroup.targets != null && targetGroup.targets.Count > 0 ? targetGroup.targets.Count : 1;
            int totalHitables = CountTotalHitableObjects(targetGroup);
            int knifeCount = data.GetEffectiveKnifeCount();

            float rotationSpeed = ResolveFallbackRotationSpeed(targetGroup);
            float directionChangeInterval = 0f;
            bool randomizeDirection = false;

            return new LevelRules(
                knifeCount,
                rotationSpeed,
                directionChangeInterval,
                randomizeDirection,
                targetCount,
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
    }
}
