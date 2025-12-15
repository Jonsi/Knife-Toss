using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.RotationBehaviors
{
    /// <summary>
    /// Rotation behavior driven by an AnimationCurve evaluated over time.
    /// </summary>
    public class CurveRotationBehavior : ILocalTargetBehavior
    {
        private readonly AnimationCurve _Curve;
        private readonly float _Duration;
        private float _Elapsed;

        /// <summary>
        /// Initializes a new instance of the curve rotation behavior.
        /// </summary>
        /// <param name="curve">Curve defining speed over time.</param>
        public CurveRotationBehavior(AnimationCurve curve)
        {
            _Curve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 0f);
            _Duration = _Curve.length > 0 ? _Curve[_Curve.length - 1].time : 1f;
            if (_Duration <= 0f)
            {
                _Duration = 1f;
            }
        }

        /// <summary>
        /// Applies the curve-driven rotation for the current frame.
        /// </summary>
        /// <param name="runtime">Frame runtime context.</param>
        public void Tick(LocalBehaviorRuntime runtime)
        {
            _Elapsed += runtime.DeltaTime;
            if (_Elapsed > _Duration)
            {
                _Elapsed -= _Duration;
            }

            float speed = _Curve.Evaluate(_Elapsed);
            runtime.AddRotationDelta(speed * runtime.DeltaTime);
        }
    }
}
