using System;
using Game.Entities.Target.Views;
using UnityEngine;

namespace Game.Entities.Target.Data
{
    /// <summary>
    /// Configuration data for a single target instance in a level.
    /// </summary>
    [Serializable]
    public class TargetConfig
    {
        [field: Header("Visuals")]
        [field: Tooltip("Optional override for the target prefab. If null, default is used.")]
        [field: SerializeField] public TargetView PrefabOverride { get; private set; }
        
        [field: Tooltip("Base scale multiplier for the target.")]
        [field: SerializeField] public float BaseScale { get; private set; } = 1f;

        [field: Header("Positioning")]
        [field: Tooltip("Distance from the center of the level.")]
        [field: SerializeField] public float OrbitRadius { get; private set; }
        
        [field: Tooltip("Starting angle on the orbit in degrees.")]
        [field: SerializeField] public float StartingAngle { get; private set; }
        
        [field: Tooltip("Speed at which the target orbits the center (degrees per second).")]
        [field: SerializeField] public float OrbitSpeed { get; private set; }

        [field: Header("Gameplay")]
        [field: Tooltip("Number of knife hits required to destroy this target.")]
        [field: SerializeField] public int KnivesToBreak { get; private set; } = 5;

        [field: Header("Rotation")]
        [field: Tooltip("Speed of the target's self-rotation in degrees per second.")]
        [field: SerializeField] public float RotationSpeed { get; private set; } = 50f;
        
        [field: Tooltip("Direction of rotation (1 for clockwise, -1 for counter-clockwise).")]
        [field: SerializeField] public int RotationDirection { get; private set; } = 1;

        [field: Header("Rotation Behavior")]
        [field: Tooltip("Rotation strategy type: Constant, Pulsating, Flipping, Curve.")]
        [field: SerializeField] public string RotationBehaviorType { get; private set; } = "Constant";
        
        [field: Tooltip("Amplitude for pulsating rotation speed.")]
        [field: SerializeField] public float PulsateAmplitude { get; private set; }
        
        [field: Tooltip("Frequency for pulsating rotation speed in hertz.")]
        [field: SerializeField] public float PulsateFrequency { get; private set; } = 1f;
        
        [field: Tooltip("Minimum seconds before a flip occurs.")]
        [field: SerializeField] public float FlipMinInterval { get; private set; } = 1f;
        
        [field: Tooltip("Maximum seconds before a flip occurs.")]
        [field: SerializeField] public float FlipMaxInterval { get; private set; } = 3f;
        
        [field: Tooltip("Curve-driven rotation speed over time.")]
        [field: SerializeField] public AnimationCurve RotationCurve { get; private set; }

        [field: Header("Collectibles")]
        [field: Tooltip("List of collectibles to attach to this target.")]
        [field: SerializeField] public System.Collections.Generic.List<Game.Entities.Collectible.Data.CollectibleConfig> Collectibles { get; private set; } = new();

        [field: Header("Pre-Stuck Knives")]
        [field: Tooltip("Knives that should start already stuck to this target.")]
        [field: SerializeField] public System.Collections.Generic.List<PreStuckKnifeConfig> PreStuckKnives { get; private set; } = new();
    }
}

