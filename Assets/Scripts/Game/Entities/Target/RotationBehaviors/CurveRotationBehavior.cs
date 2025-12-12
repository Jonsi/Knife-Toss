using Game.Entities.Target.Interfaces;
using UnityEngine;

namespace Game.Entities.Target.RotationBehaviors
{
    /// <summary>
    /// Rotation behavior driven by an AnimationCurve evaluated over time.
    /// </summary>
    public class CurveRotationBehavior : IRotationBehavior
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
        /// Calculates the rotation delta for the current frame.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last frame.</param>
        /// <returns>Rotation delta in degrees.</returns>
        public float GetRotationDelta(float deltaTime)
        {
            _Elapsed += deltaTime;
            if (_Elapsed > _Duration)
            {
                _Elapsed -= _Duration;
            }

            float speed = _Curve.Evaluate(_Elapsed);
            return speed * deltaTime;
        }
    }
}
