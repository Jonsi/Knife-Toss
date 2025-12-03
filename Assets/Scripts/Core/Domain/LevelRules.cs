using KnifeToss.Core.Data;
using UnityEngine;

namespace KnifeToss.Core.Domain
{



    public readonly struct LevelRules
    {
        public int RequiredKnives { get; }
        public int MaxKnives { get; }
        public float RotationSpeed { get; }
        public float DirectionChangeInterval { get; }
        public bool RandomizeDirection { get; }
        public int TargetCount { get; }

        public LevelRules(
            int requiredKnives,
            int maxKnives,
            float rotationSpeed,
            float directionChangeInterval,
            bool randomizeDirection,
            int targetCount = 1)
        {
            RequiredKnives = Mathf.Max(0, requiredKnives);
            MaxKnives = Mathf.Max(0, maxKnives);
            RotationSpeed = rotationSpeed;
            DirectionChangeInterval = Mathf.Max(0f, directionChangeInterval);
            RandomizeDirection = randomizeDirection;
            TargetCount = Mathf.Max(1, targetCount);
        }

        public static LevelRules FromLevelData(LevelData data)
        {
            if (data == null)
            {
                return new LevelRules(5, 0, 90f, 3f, true, 1);
            }

            LevelRequirementsConfig requirements = data.requirements ?? new LevelRequirementsConfig();
            TargetGroupConfig targetGroup = data.targetGroup ?? new TargetGroupConfig();

            int targetCount = targetGroup.targets != null && targetGroup.targets.Count > 0 ? targetGroup.targets.Count : 1;

            // Legacy rotation values kept for compatibility; derived from first target if available.
            float rotationSpeed = ResolveFallbackRotationSpeed(targetGroup);
            float directionChangeInterval = 0f;
            bool randomizeDirection = false;

            return new LevelRules(
                requirements.requiredKnives,
                requirements.maxKnives,
                rotationSpeed,
                directionChangeInterval,
                randomizeDirection,
                targetCount);
        }

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
    }
}
