using System;
using UnityEngine;

namespace KnifeToss.Core.Data.Target
{
    /// <summary>
    /// Configuration data for a single target instance in a level.
    /// </summary>
    [Serializable]
    public class TargetConfig
    {
        [Header("Visuals")]
        [Tooltip("Optional override for the target prefab. If null, default is used.")]
        [field: SerializeField] public GameObject PrefabOverride { get; private set; }
        
        [Tooltip("Base scale multiplier for the target.")]
        [field: SerializeField] public float BaseScale { get; private set; } = 1f;

        [Header("Positioning")]
        [Tooltip("Distance from the center of the level.")]
        [field: SerializeField] public float OrbitRadius { get; private set; }
        
        [Tooltip("Starting angle on the orbit in degrees.")]
        [field: SerializeField] public float StartingAngle { get; private set; }
        
        [Tooltip("Speed at which the target orbits the center (degrees per second).")]
        [field: SerializeField] public float OrbitSpeed { get; private set; }

        [Header("Gameplay")]
        [Tooltip("Number of knife hits required to destroy this target.")]
        [field: SerializeField] public int KnivesToBreak { get; private set; } = 5;

        [Header("Rotation")]
        [Tooltip("Speed of the target's self-rotation in degrees per second.")]
        [field: SerializeField] public float RotationSpeed { get; private set; } = 50f;
        
        [Tooltip("Direction of rotation (1 for clockwise, -1 for counter-clockwise).")]
        [field: SerializeField] public int RotationDirection { get; private set; } = 1;
    }
}

