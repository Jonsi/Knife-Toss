using System;
using UnityEngine;

namespace KnifeToss.Core.Data
{
    [Serializable]
    public sealed class TargetScalingConfig
    {
        [Tooltip("Radius treated as the minimum for scaling.")]
        public float minRadius = 0f;

        [Tooltip("Radius treated as the maximum for scaling.")]
        public float maxRadius = 5f;

        [Tooltip("Scale applied at min radius.")]
        public float minScale = 0.9f;

        [Tooltip("Scale applied at max radius.")]
        public float maxScale = 1.1f;

        [Tooltip("Curve mapping normalized radius (0-1) to normalized scale (0-1).")]
        public AnimationCurve radiusToScale = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float Evaluate(float radius)
        {
            if (maxRadius <= minRadius)
            {
                return Mathf.Clamp(maxScale, 0.01f, 10f);
            }

            float t = Mathf.InverseLerp(minRadius, maxRadius, radius);
            float curveValue = radiusToScale != null ? radiusToScale.Evaluate(t) : t;
            float scale = Mathf.Lerp(minScale, maxScale, curveValue);
            return Mathf.Max(0.01f, scale);
        }

        public TargetScalingConfig Clone()
        {
            return new TargetScalingConfig
            {
                minRadius = minRadius,
                maxRadius = maxRadius,
                minScale = minScale,
                maxScale = maxScale,
                radiusToScale = radiusToScale != null ? new AnimationCurve(radiusToScale.keys) : AnimationCurve.Linear(0f, 0f, 1f, 1f)
            };
        }
    }
}
