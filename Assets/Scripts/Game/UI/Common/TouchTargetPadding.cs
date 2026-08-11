using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common
{
    /// <summary>
    /// Expands a Selectable's raycast area without resizing its visible artwork.
    /// Pointer events received by the transparent child bubble to the Selectable on
    /// this GameObject, preserving the existing click outcome and visual layout.
    /// </summary>
    [AddComponentMenu("UI/Layout/Touch Target Padding")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Selectable))]
    public sealed class TouchTargetPadding : MonoBehaviour
    {
        private const string HitAreaName = "__TouchTargetPadding";

        [SerializeField, Min(1f)]
        private float _MinimumSizeDp = TouchTargetValidator.DefaultMinimumSizeDp;

        [SerializeField, Min(0.01f)]
        private float _ReferencePixelsPerDp = TouchTargetValidator.DefaultReferencePixelsPerDp;

        private RectTransform _HitArea;

        public float MinimumSizeDp => _MinimumSizeDp;
        public float ReferencePixelsPerDp => _ReferencePixelsPerDp;

        private void Awake()
        {
            EnsureHitArea();
        }

        private void OnEnable()
        {
            EnsureHitArea();
        }

        /// <summary>
        /// Creates or refreshes the invisible raycast graphic. Public for deterministic
        /// EditMode validation; normal runtime callers do not need to invoke it.
        /// </summary>
        public RectTransform EnsureHitArea()
        {
            RectTransform target = transform as RectTransform;
            if (target == null || !IsPositiveFinite(_MinimumSizeDp) ||
                !IsPositiveFinite(_ReferencePixelsPerDp))
            {
                return null;
            }

            if (_HitArea == null)
            {
                Transform existing = transform.Find(HitAreaName);
                _HitArea = existing as RectTransform;
            }

            if (_HitArea == null)
            {
                var hitAreaObject = new GameObject(
                    HitAreaName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                hitAreaObject.layer = gameObject.layer;
                _HitArea = (RectTransform)hitAreaObject.transform;
                _HitArea.SetParent(transform, false);
            }

            _HitArea.SetAsFirstSibling();
            _HitArea.anchorMin = new Vector2(0.5f, 0.5f);
            _HitArea.anchorMax = new Vector2(0.5f, 0.5f);
            _HitArea.pivot = new Vector2(0.5f, 0.5f);
            _HitArea.anchoredPosition = Vector2.zero;
            _HitArea.localRotation = Quaternion.identity;
            _HitArea.localScale = Vector3.one;

            float minimumCanvasUnits = _MinimumSizeDp * _ReferencePixelsPerDp;
            _HitArea.sizeDelta = new Vector2(
                Mathf.Max(target.rect.width, minimumCanvasUnits),
                Mathf.Max(target.rect.height, minimumCanvasUnits));

            Image image = _HitArea.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            image.maskable = true;
            return _HitArea;
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
