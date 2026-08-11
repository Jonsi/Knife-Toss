using System.Collections.Generic;
using Game.UI.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common
{
    public enum TouchTargetValidationStatus
    {
        MeetsMinimum = 0,
        BelowMinimum = 1,
        DocumentedException = 2,
        MissingTarget = 3,
        InvalidConfiguration = 4
    }

    public readonly struct TouchTargetValidationResult
    {
        public TouchTargetValidationResult(
            RectTransform target,
            Vector2 sizeDp,
            float minimumSizeDp,
            TouchTargetValidationStatus status,
            string exceptionReason = null)
        {
            Target = target;
            SizeDp = sizeDp;
            MinimumSizeDp = minimumSizeDp;
            Status = status;
            ExceptionReason = exceptionReason ?? string.Empty;
        }

        public RectTransform Target { get; }
        public Vector2 SizeDp { get; }
        public float MinimumSizeDp { get; }
        public TouchTargetValidationStatus Status { get; }
        public string ExceptionReason { get; }
        public bool MeetsWidth => SizeDp.x + TouchTargetValidator.ComparisonTolerance >= MinimumSizeDp;
        public bool MeetsHeight => SizeDp.y + TouchTargetValidator.ComparisonTolerance >= MinimumSizeDp;
        public bool IsAccepted => Status == TouchTargetValidationStatus.MeetsMinimum ||
                                  Status == TouchTargetValidationStatus.DocumentedException;
    }

    /// <summary>
    /// Audits active Unity UI Selectables beneath a screen root against the project's 48 dp
    /// minimum action size. At the 1080x1920 design baseline one dp is three reference pixels.
    /// This is an audit tool only; it never changes layout or interactability.
    /// </summary>
    [AddComponentMenu("UI/Validation/Touch Target Validator")]
    [DisallowMultipleComponent]
    public sealed class TouchTargetValidator : MonoBehaviour
    {
        public const float DefaultMinimumSizeDp = UiTokenDefaults.MinimumTouchTargetDp;
        public const float DefaultReferencePixelsPerDp = UiTokenDefaults.ReferencePixelsPerDp;
        internal const float ComparisonTolerance = 0.001f;

        [SerializeField] private RectTransform _ValidationRoot;
        [SerializeField, Min(1f)] private float _MinimumSizeDp = DefaultMinimumSizeDp;
        [SerializeField, Min(0.01f)] private float _ReferencePixelsPerDp = DefaultReferencePixelsPerDp;
        [SerializeField] private bool _IncludeInactive;

        public float MinimumSizeDp => _MinimumSizeDp;
        public float ReferencePixelsPerDp => _ReferencePixelsPerDp;

        public IReadOnlyList<TouchTargetValidationResult> ValidateTargets()
        {
            Transform root = _ValidationRoot != null ? _ValidationRoot : transform;
            Selectable[] selectables = root.GetComponentsInChildren<Selectable>(_IncludeInactive);
            var results = new TouchTargetValidationResult[selectables.Length];

            for (var i = 0; i < selectables.Length; i++)
            {
                results[i] = Validate(
                    selectables[i].transform as RectTransform,
                    _MinimumSizeDp,
                    _ReferencePixelsPerDp);
            }

            return results;
        }

        public void Configure(
            RectTransform validationRoot,
            float minimumSizeDp = DefaultMinimumSizeDp,
            float referencePixelsPerDp = DefaultReferencePixelsPerDp,
            bool includeInactive = false)
        {
            _ValidationRoot = validationRoot;
            _MinimumSizeDp = minimumSizeDp;
            _ReferencePixelsPerDp = referencePixelsPerDp;
            _IncludeInactive = includeInactive;
        }

        public static TouchTargetValidationResult Validate(
            RectTransform target,
            float minimumSizeDp = DefaultMinimumSizeDp,
            float referencePixelsPerDp = DefaultReferencePixelsPerDp)
        {
            if (target == null)
            {
                return new TouchTargetValidationResult(
                    null,
                    Vector2.zero,
                    minimumSizeDp,
                    TouchTargetValidationStatus.MissingTarget);
            }

            if (!IsPositiveFinite(minimumSizeDp) || !IsPositiveFinite(referencePixelsPerDp))
            {
                return new TouchTargetValidationResult(
                    target,
                    Vector2.zero,
                    minimumSizeDp,
                    TouchTargetValidationStatus.InvalidConfiguration);
            }

            Vector2 sizeDp = MeasureInRootCanvasUnits(target) / referencePixelsPerDp;
            TouchTargetPadding padding = target.GetComponent<TouchTargetPadding>();
            if (padding != null)
            {
                sizeDp.x = Mathf.Max(sizeDp.x, padding.MinimumSizeDp);
                sizeDp.y = Mathf.Max(sizeDp.y, padding.MinimumSizeDp);
            }

            bool meetsMinimum = sizeDp.x + ComparisonTolerance >= minimumSizeDp &&
                                sizeDp.y + ComparisonTolerance >= minimumSizeDp;

            if (meetsMinimum)
            {
                return new TouchTargetValidationResult(
                    target,
                    sizeDp,
                    minimumSizeDp,
                    TouchTargetValidationStatus.MeetsMinimum);
            }

            TouchTargetException exception = target.GetComponent<TouchTargetException>();
            string exceptionReason = exception != null ? exception.Reason : string.Empty;
            TouchTargetValidationStatus status = string.IsNullOrEmpty(exceptionReason)
                ? TouchTargetValidationStatus.BelowMinimum
                : TouchTargetValidationStatus.DocumentedException;

            return new TouchTargetValidationResult(
                target,
                sizeDp,
                minimumSizeDp,
                status,
                exceptionReason);
        }

        private static Vector2 MeasureInRootCanvasUnits(RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Canvas canvas = target.GetComponentInParent<Canvas>();
            Transform measurementSpace = canvas != null ? canvas.rootCanvas.transform : null;

            Vector3 bottomLeft = ToMeasurementSpace(corners[0], measurementSpace);
            Vector3 topLeft = ToMeasurementSpace(corners[1], measurementSpace);
            Vector3 topRight = ToMeasurementSpace(corners[2], measurementSpace);
            Vector3 bottomRight = ToMeasurementSpace(corners[3], measurementSpace);

            return new Vector2(
                Vector3.Distance(bottomLeft, bottomRight),
                Vector3.Distance(bottomLeft, topLeft));
        }

        private static Vector3 ToMeasurementSpace(Vector3 worldPoint, Transform measurementSpace)
        {
            return measurementSpace != null
                ? measurementSpace.InverseTransformPoint(worldPoint)
                : worldPoint;
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        [ContextMenu("Validate Touch Targets")]
        private void ValidateAndLog()
        {
            IReadOnlyList<TouchTargetValidationResult> results = ValidateTargets();
            var failureCount = 0;
            var exceptionCount = 0;

            foreach (TouchTargetValidationResult result in results)
            {
                if (result.Status == TouchTargetValidationStatus.DocumentedException)
                {
                    exceptionCount++;
                    Debug.LogWarning(
                        $"Touch target '{result.Target.name}' is {result.SizeDp.x:0.#}x{result.SizeDp.y:0.#} dp " +
                        $"and uses the documented exception: {result.ExceptionReason}",
                        result.Target);
                }
                else if (!result.IsAccepted)
                {
                    failureCount++;
                    string targetName = result.Target != null ? result.Target.name : "<missing>";
                    Debug.LogError(
                        $"Touch target '{targetName}' failed: {result.SizeDp.x:0.#}x{result.SizeDp.y:0.#} dp; " +
                        $"minimum is {result.MinimumSizeDp:0.#}x{result.MinimumSizeDp:0.#} dp.",
                        result.Target);
                }
            }

            if (failureCount == 0)
            {
                Debug.Log(
                    $"Touch-target validation passed for {results.Count} actions " +
                    $"({exceptionCount} documented exceptions).",
                    this);
            }
        }
    }
}
