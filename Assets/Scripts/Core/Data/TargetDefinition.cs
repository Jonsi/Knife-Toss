using System;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public sealed class TargetDefinition
    {
        [Tooltip("Prefab for this target. Falls back to the installer prefab if left empty.")]
        public GameObject prefab;

        [Tooltip("Radius from the pivot where this target orbits.")]
        public float orbitRadius = 0f;

        [Tooltip("Initial orbit angle in degrees.")]
        public float startingAngle = 0f;

        [Tooltip("Orbit angular speed in degrees per second around the central pivot.")]
        public float orbitDegreesPerSecond = 0f;

        [Tooltip("Base scale multiplier applied before distance scaling.")]
        public float baseScale = 1f;

        [Header("Pre-Stuck Knives")]
        [Tooltip("Number of knives to spawn already embedded in this target.")]
        public int preStuckKnives = 0;

        [Tooltip("Angular spacing between pre-stuck knives (degrees).")]
        public float preStuckKnifeSpacing = 45f;

        [Tooltip("Offset in degrees applied to all pre-stuck knife positions. Each knife's position will be: offset + (index * spacing).")]
        public float preStuckKnifeSpacingOffset = 0f;

        [Tooltip("Rotation behaviour settings for this target.")]
        public TargetRotationConfig rotation = new TargetRotationConfig();

        [Header("Hitable Objects")]
        [Tooltip("Optional hitable objects placed around this target.")]
        public System.Collections.Generic.List<HitableObjectPlacement> hitableObjects = new System.Collections.Generic.List<HitableObjectPlacement>();

        [Tooltip("Offset in degrees applied to all hitable object positions. Each object's position will be: offset + (index * spacing).")]
        public float hitableObjectSpacingOffset = 0f;

        [Tooltip("Number of knives required to break this target. 0 hides the indicator.")]
        public int knivesToBreak = 0;
    }
}
