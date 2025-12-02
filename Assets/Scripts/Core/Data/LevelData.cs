using UnityEngine;

namespace KnifeToss.Core.Data
{
    [System.Serializable]
    public class LevelData
    {
        [Header("Level Requirements")]
        [Tooltip("Number of knives the player must stick to complete the level")]
        public int requiredKnives = 5;

        [Tooltip("Maximum number of knives the player can throw (0 = unlimited)")]
        public int maxKnives = 0;

        [Header("Target Settings")]
        [Tooltip("Rotation speed of the target in degrees per second")]
        public float rotationSpeed = 90f;

        [Tooltip("How often the target changes direction (in seconds, 0 = never)")]
        public float directionChangeInterval = 3f;

        [Tooltip("Whether the target should randomize direction")]
        public bool randomizeDirection = true;

        [Header("Pre-Stuck Knives")]
        [Tooltip("Number of knives already stuck to the target at level start")]
        public int preStuckKnives = 0;

        [Tooltip("Angular spacing between pre-stuck knives (in degrees)")]
        public float preStuckKnifeSpacing = 45f;
    }
}

