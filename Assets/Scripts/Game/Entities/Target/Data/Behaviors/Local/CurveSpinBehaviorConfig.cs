using System;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.RotationBehaviors;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors.Local
{
    /// <summary>
    /// Configuration for a curve-driven spin behavior.
    /// </summary>
    [Serializable]
    public class CurveSpinBehaviorConfig : ILocalBehaviorConfig
    {
        /// <inheritdoc />
        public TargetLocalBehaviorKind Kind => TargetLocalBehaviorKind.CurveSpin;

        [Tooltip("Curve describing rotation speed over time.")]
        [SerializeField] private AnimationCurve _Curve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        public CurveSpinBehaviorConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance with a custom curve.
        /// </summary>
        /// <param name="curve">Curve describing the speed.</param>
        public CurveSpinBehaviorConfig(AnimationCurve curve)
        {
            _Curve = curve;
        }

        /// <inheritdoc />
        public ILocalTargetBehavior CreateLocalBehavior()
        {
            return new CurveRotationBehavior(_Curve);
        }
    }
}
