using UnityEngine;

namespace Core.Domain
{
    public readonly struct KnifeVisuals
    {
        public readonly Vector3 Scale;
        public readonly Color Color;
        public readonly bool IsVisible;

        /// <summary>
        /// Performs the KnifeVisuals operation.
        /// </summary>
        public KnifeVisuals(Vector3 scale, Color color, bool isVisible)
        {
            Scale = scale;
            Color = color;
            IsVisible = isVisible;
        }

        /// <summary>
        /// Performs the Visible operation.
        /// </summary>
        public static KnifeVisuals Visible(Vector3 scale, Color color) => new KnifeVisuals(scale, color, true);
        /// <summary>
        /// Performs the Invisible operation.
        /// </summary>
        public static KnifeVisuals Invisible() => new KnifeVisuals(Vector3.zero, Color.clear, false);
    }
}
