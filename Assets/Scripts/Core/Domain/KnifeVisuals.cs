using UnityEngine;

namespace KnifeToss.Core.Domain
{
    public readonly struct KnifeVisuals
    {
        public readonly Vector3 Scale;
        public readonly Color Color;
        public readonly bool IsVisible;

        public KnifeVisuals(Vector3 scale, Color color, bool isVisible)
        {
            Scale = scale;
            Color = color;
            IsVisible = isVisible;
        }

        public static KnifeVisuals Visible(Vector3 scale, Color color) => new KnifeVisuals(scale, color, true);
        public static KnifeVisuals Invisible() => new KnifeVisuals(Vector3.zero, Color.clear, false);
    }
}
