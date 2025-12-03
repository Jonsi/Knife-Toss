using System;
using UnityEngine;

namespace KnifeToss.Core.Data
{
    /// <summary>
    /// Enumerates RotationBehaviourType options.
    /// </summary>
    public enum RotationBehaviourType
    {
        Constant = 0,
        Pulsating = 1,
        Flipping = 2,
        AccelerationCurve = 3
    }

    [Serializable]
    public sealed class PulsatingRotationSettings
    {
        [Tooltip("Amplitude applied to the base speed when pulsating.")]
        public float amplitude = 30f;

        [Tooltip("Frequency of pulsation in Hz.")]
        public float frequency = 1f;
    }

    [Serializable]
    public sealed class FlippingRotationSettings
    {
        [Tooltip("How often a flip can occur (seconds).")]
        public float flipInterval = 2f;

        [Range(0f, 1f)]
        [Tooltip("Chance of flipping direction each interval.")]
        public float flipProbability = 0.5f;
    }

    [Serializable]
    public sealed class AccelerationRotationSettings
    {
        [Tooltip("Speed multiplier over time; evaluated over one loop duration.")]
        public AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Tooltip("Duration of a full curve loop in seconds.")]
        public float curveLoopDuration = 2f;
    }

    [Serializable]
    public sealed class TargetRotationConfig
    {
        [Tooltip("Base rotation speed in degrees per second.")]
        public float speed = 90f;

        [Tooltip("Which rotation behaviour to use for this target.")]
        public RotationBehaviourType behaviourType = RotationBehaviourType.Constant;

        [Tooltip("Settings used when behaviour type is Pulsating.")]
        public PulsatingRotationSettings pulsating = new PulsatingRotationSettings();

        [Tooltip("Settings used when behaviour type is Flipping.")]
        public FlippingRotationSettings flipping = new FlippingRotationSettings();

        [Tooltip("Settings used when behaviour type is Acceleration Curve.")]
        public AccelerationRotationSettings acceleration = new AccelerationRotationSettings();
    }
}
