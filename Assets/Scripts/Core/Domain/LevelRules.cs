using KnifeToss.Core.Data;
using UnityEngine;

namespace KnifeToss.Core.Domain
{
    /// <summary>
    /// Immutable snapshot of the rules that describe a level.
    /// </summary>
    public readonly struct LevelRules
    {
        public int RequiredKnives { get; }
        public int MaxKnives { get; }
        public float RotationSpeed { get; }
        public float DirectionChangeInterval { get; }
        public bool RandomizeDirection { get; }
        public int PreStuckKnives { get; }
        public float PreStuckKnifeSpacing { get; }

        public LevelRules(
            int requiredKnives,
            int maxKnives,
            float rotationSpeed,
            float directionChangeInterval,
            bool randomizeDirection,
            int preStuckKnives,
            float preStuckKnifeSpacing)
        {
            RequiredKnives = Mathf.Max(0, requiredKnives);
            MaxKnives = Mathf.Max(0, maxKnives);
            RotationSpeed = rotationSpeed;
            DirectionChangeInterval = Mathf.Max(0f, directionChangeInterval);
            RandomizeDirection = randomizeDirection;
            PreStuckKnives = Mathf.Max(0, preStuckKnives);
            PreStuckKnifeSpacing = Mathf.Max(1f, preStuckKnifeSpacing);
        }

        public static LevelRules FromLevelData(LevelData data)
        {
            if (data == null)
            {
                return new LevelRules(5, 0, 90f, 3f, true, 0, 45f);
            }

            return new LevelRules(
                data.requiredKnives,
                data.maxKnives,
                data.rotationSpeed,
                data.directionChangeInterval,
                data.randomizeDirection,
                data.preStuckKnives,
                data.preStuckKnifeSpacing);
        }
    }
}
