using UnityEngine;

namespace Game.UI.Tokens
{
    /// <summary>
    /// Safe, code-level fallbacks for UI geometry that must remain available even when a
    /// token asset has not been injected yet. The production asset mirrors these values.
    /// </summary>
    public static class UiTokenDefaults
    {
        public const float GridDp = 8f;
        public const float MinimumTouchTargetDp = 48f;
        public const float ReferencePixelsPerDp = 3f;
        public const float CanvasWidthMatch = 0.5f;
        public const float SafeContentMarginDp = 16f;

        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1080, 1920);
    }
}
