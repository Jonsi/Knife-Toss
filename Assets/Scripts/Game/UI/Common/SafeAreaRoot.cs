using System;
using UnityEngine;

namespace Game.UI.Common
{
    [Flags]
    public enum SafeAreaEdges
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Bottom = 1 << 2,
        Top = 1 << 3,
        All = Left | Right | Bottom | Top
    }

    /// <summary>
    /// Converts a screen-space safe-area rectangle into normalized anchors.
    /// Keeping the calculation separate makes device-profile previews deterministic in EditMode tests.
    /// </summary>
    public static class SafeAreaUtility
    {
        public static bool TryGetNormalizedAnchors(
            Rect safeArea,
            Vector2Int screenSize,
            out Vector2 anchorMin,
            out Vector2 anchorMax)
        {
            anchorMin = Vector2.zero;
            anchorMax = Vector2.one;

            if (screenSize.x <= 0 || screenSize.y <= 0 || !IsFinite(safeArea))
                return false;

            float xMin = Mathf.Clamp(safeArea.xMin, 0f, screenSize.x);
            float yMin = Mathf.Clamp(safeArea.yMin, 0f, screenSize.y);
            float xMax = Mathf.Clamp(safeArea.xMax, 0f, screenSize.x);
            float yMax = Mathf.Clamp(safeArea.yMax, 0f, screenSize.y);

            if (xMax <= xMin || yMax <= yMin)
                return false;

            anchorMin = new Vector2(xMin / screenSize.x, yMin / screenSize.y);
            anchorMax = new Vector2(xMax / screenSize.x, yMax / screenSize.y);
            return true;
        }

        private static bool IsFinite(Rect rect)
        {
            return IsFinite(rect.x) &&
                   IsFinite(rect.y) &&
                   IsFinite(rect.width) &&
                   IsFinite(rect.height);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Reusable root for UI that must remain inside <see cref="Screen.safeArea"/>.
    /// Place this on a full-stretch RectTransform directly below a root Canvas, then parent
    /// safe-area-sensitive controls beneath it. Decorative backgrounds should remain outside.
    /// </summary>
    [AddComponentMenu("UI/Layout/Safe Area Root")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaRoot : MonoBehaviour
    {
        [SerializeField] private SafeAreaEdges _Edges = SafeAreaEdges.All;

        private RectTransform _RectTransform;
        private Rect _LastSafeArea;
        private Vector2Int _LastScreenSize;
        private bool _HasAppliedGeometry;

        public SafeAreaEdges Edges => _Edges;

        private void OnEnable()
        {
            _HasAppliedGeometry = false;
            ApplyCurrentSafeArea();
        }

        private void Update()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            Rect safeArea = Screen.safeArea;

            if (!_HasAppliedGeometry || screenSize != _LastScreenSize || safeArea != _LastSafeArea)
                Apply(safeArea, screenSize);
        }

        private void OnTransformParentChanged()
        {
            _HasAppliedGeometry = false;
        }

        public bool ApplyCurrentSafeArea()
        {
            return Apply(Screen.safeArea, new Vector2Int(Screen.width, Screen.height));
        }

        /// <summary>
        /// Applies explicit geometry. This entry point is also used by resolution-preview tooling
        /// and does not depend on the Editor's current Game View.
        /// </summary>
        public bool Apply(Rect safeArea, Vector2Int screenSize)
        {
            if (!SafeAreaUtility.TryGetNormalizedAnchors(
                    safeArea,
                    screenSize,
                    out Vector2 safeAnchorMin,
                    out Vector2 safeAnchorMax))
            {
                return false;
            }

            RectTransform rectTransform = GetRectTransform();
            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;

            if ((_Edges & SafeAreaEdges.Left) != 0)
                anchorMin.x = safeAnchorMin.x;
            if ((_Edges & SafeAreaEdges.Right) != 0)
                anchorMax.x = safeAnchorMax.x;
            if ((_Edges & SafeAreaEdges.Bottom) != 0)
                anchorMin.y = safeAnchorMin.y;
            if ((_Edges & SafeAreaEdges.Top) != 0)
                anchorMax.y = safeAnchorMax.y;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            _LastSafeArea = safeArea;
            _LastScreenSize = screenSize;
            _HasAppliedGeometry = true;
            return true;
        }

        /// <summary>
        /// Selects which physical edges constrain this root. The next explicit or runtime refresh
        /// applies the new policy.
        /// </summary>
        public void SetEdges(SafeAreaEdges edges)
        {
            _Edges = edges;
            _HasAppliedGeometry = false;
        }

        private RectTransform GetRectTransform()
        {
            if (_RectTransform == null)
                _RectTransform = (RectTransform)transform;

            return _RectTransform;
        }
    }
}
