using System;
using System.Collections.Generic;
using Game.UI.Common;
using TMPro;
using UnityEngine;

namespace Game.UI.Tokens
{
    public enum UiColorRole
    {
        Ink,
        Ivory,
        WoodDark,
        WoodMid,
        WoodLight,
        Parchment,
        SunGold,
        Lagoon,
        Leaf,
        FailureDanger,
        Disabled,
        Scrim,
        Success,
        BlockedFeedback
    }

    public enum UiTypographyRole
    {
        Display,
        Heading,
        Action,
        Body,
        Legal,
        Value,
        Caption
    }

    public enum UiEasing
    {
        Linear,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutBack
    }

    [Serializable]
    public sealed class UiColorTokens
    {
        [SerializeField] private Color _Ink = new Color32(0x24, 0x16, 0x0F, 0xFF);
        [SerializeField] private Color _Ivory = new Color32(0xFF, 0xF4, 0xDE, 0xFF);
        [SerializeField] private Color _WoodDark = new Color32(0x4A, 0x26, 0x17, 0xFF);
        [SerializeField] private Color _WoodMid = new Color32(0x8B, 0x4B, 0x24, 0xFF);
        [SerializeField] private Color _WoodLight = new Color32(0xC2, 0x7A, 0x3A, 0xFF);
        [SerializeField] private Color _Parchment = new Color32(0xE8, 0xC7, 0x9B, 0xFF);
        [SerializeField] private Color _SunGold = new Color32(0xFF, 0xC4, 0x3D, 0xFF);
        [SerializeField] private Color _Lagoon = new Color32(0x19, 0xBF, 0xC4, 0xFF);
        [SerializeField] private Color _Leaf = new Color32(0x45, 0xC9, 0x68, 0xFF);
        [SerializeField] private Color _FailureDanger = new Color32(0xE8, 0x4B, 0x3C, 0xFF);
        [SerializeField] private Color _Disabled = new Color32(0x73, 0x67, 0x5C, 0xFF);
        [SerializeField] private Color _Scrim = new Color32(0x07, 0x17, 0x0F, 0xCC);
        [SerializeField] private Color _Success = new Color32(0x2F, 0xAF, 0x5D, 0xFF);
        [SerializeField] private Color _BlockedFeedback = new Color32(0xF2, 0x6A, 0x2E, 0xFF);

        public Color Get(UiColorRole role)
        {
            return role switch
            {
                UiColorRole.Ink => _Ink,
                UiColorRole.Ivory => _Ivory,
                UiColorRole.WoodDark => _WoodDark,
                UiColorRole.WoodMid => _WoodMid,
                UiColorRole.WoodLight => _WoodLight,
                UiColorRole.Parchment => _Parchment,
                UiColorRole.SunGold => _SunGold,
                UiColorRole.Lagoon => _Lagoon,
                UiColorRole.Leaf => _Leaf,
                UiColorRole.FailureDanger => _FailureDanger,
                UiColorRole.Disabled => _Disabled,
                UiColorRole.Scrim => _Scrim,
                UiColorRole.Success => _Success,
                UiColorRole.BlockedFeedback => _BlockedFeedback,
                _ => _Ink
            };
        }
    }

    [Serializable]
    public sealed class UiTypographyStyle
    {
        [SerializeField] private TMP_FontAsset _FontAsset;
        [SerializeField, Min(1f)] private float _SizeDp;
        [SerializeField, Min(1f)] private float _MinimumSizeDp;
        [SerializeField, Min(80f)] private float _LineHeightPercent;
        [SerializeField] private float _CharacterSpacing;

        public UiTypographyStyle(float sizeDp, float minimumSizeDp, float lineHeightPercent, float characterSpacing = 0f)
        {
            _SizeDp = sizeDp;
            _MinimumSizeDp = minimumSizeDp;
            _LineHeightPercent = lineHeightPercent;
            _CharacterSpacing = characterSpacing;
        }

        public TMP_FontAsset FontAsset => _FontAsset;
        public float SizeDp => _SizeDp;
        public float MinimumSizeDp => _MinimumSizeDp;
        public float LineHeightPercent => _LineHeightPercent;
        public float CharacterSpacing => _CharacterSpacing;
    }

    [Serializable]
    public sealed class UiTypographyTokens
    {
        [SerializeField] private UiTypographyStyle _Display = new UiTypographyStyle(64f, 40f, 100f);
        [SerializeField] private UiTypographyStyle _Heading = new UiTypographyStyle(40f, 28f, 105f);
        [SerializeField] private UiTypographyStyle _Action = new UiTypographyStyle(24f, 18f, 100f, 1f);
        [SerializeField] private UiTypographyStyle _Body = new UiTypographyStyle(18f, 16f, 130f);
        [SerializeField] private UiTypographyStyle _Legal = new UiTypographyStyle(14f, 12f, 145f);
        [SerializeField] private UiTypographyStyle _Value = new UiTypographyStyle(28f, 20f, 100f);
        [SerializeField] private UiTypographyStyle _Caption = new UiTypographyStyle(14f, 12f, 125f);

        public UiTypographyStyle Get(UiTypographyRole role)
        {
            return role switch
            {
                UiTypographyRole.Display => _Display,
                UiTypographyRole.Heading => _Heading,
                UiTypographyRole.Action => _Action,
                UiTypographyRole.Body => _Body,
                UiTypographyRole.Legal => _Legal,
                UiTypographyRole.Value => _Value,
                UiTypographyRole.Caption => _Caption,
                _ => _Body
            };
        }
    }

    [Serializable]
    public sealed class UiLayoutTokens
    {
        [SerializeField, Min(1f)] private float _GridDp = UiTokenDefaults.GridDp;
        [SerializeField, Min(0f)] private float _HalfGridDp = 4f;
        [SerializeField, Min(0f)] private float _CompactGapDp = 8f;
        [SerializeField, Min(0f)] private float _RegularGapDp = 16f;
        [SerializeField, Min(0f)] private float _RoomyGapDp = 24f;
        [SerializeField, Min(0f)] private float _SectionGapDp = 32f;
        [SerializeField, Min(0f)] private float _ScreenMarginDp = 24f;
        [SerializeField, Min(0f)] private float _PanelInnerPaddingDp = 24f;
        [SerializeField, Min(0f)] private float _CompactControlHeightDp = 48f;
        [SerializeField, Min(0f)] private float _RegularControlHeightDp = 56f;
        [SerializeField, Min(0f)] private float _LargeControlHeightDp = 72f;
        [SerializeField, Min(0f)] private float _SmallIconDp = 32f;
        [SerializeField, Min(0f)] private float _RegularIconDp = 48f;
        [SerializeField, Min(0f)] private float _LargeIconDp = 64f;
        [SerializeField, Min(0f)] private float _HeroIconDp = 96f;

        public float GridDp => _GridDp;
        public float HalfGridDp => _HalfGridDp;
        public float CompactGapDp => _CompactGapDp;
        public float RegularGapDp => _RegularGapDp;
        public float RoomyGapDp => _RoomyGapDp;
        public float SectionGapDp => _SectionGapDp;
        public float ScreenMarginDp => _ScreenMarginDp;
        public float PanelInnerPaddingDp => _PanelInnerPaddingDp;
        public float CompactControlHeightDp => _CompactControlHeightDp;
        public float RegularControlHeightDp => _RegularControlHeightDp;
        public float LargeControlHeightDp => _LargeControlHeightDp;
        public float SmallIconDp => _SmallIconDp;
        public float RegularIconDp => _RegularIconDp;
        public float LargeIconDp => _LargeIconDp;
        public float HeroIconDp => _HeroIconDp;
    }

    [Serializable]
    public sealed class UiSurfaceTokens
    {
        [SerializeField, Min(0f)] private float _CompactRadiusDp = 8f;
        [SerializeField, Min(0f)] private float _RegularRadiusDp = 16f;
        [SerializeField, Min(0f)] private float _LargeRadiusDp = 24f;
        [SerializeField, Min(0f)] private float _ThinOutlineDp = 1f;
        [SerializeField, Min(0f)] private float _RegularOutlineDp = 2f;
        [SerializeField, Min(0f)] private float _StrongOutlineDp = 3f;
        [SerializeField, Range(0f, 0.1f)] private float _MaximumOutlineProportion = 0.04f;
        [SerializeField] private Vector2 _ShadowOffsetDp = new Vector2(0f, -4f);
        [SerializeField, Min(0f)] private float _ShadowSoftnessDp = 4f;
        [SerializeField, Range(0f, 1f)] private float _ShadowOpacity = 0.35f;
        [SerializeField, Min(0f)] private float _NineSliceEdgeDp = 16f;
        [SerializeField, Min(0f)] private float _NineSliceMinimumCenterDp = 8f;
        [SerializeField] private bool _PreserveNineSliceBorderAspect = true;

        public float CompactRadiusDp => _CompactRadiusDp;
        public float RegularRadiusDp => _RegularRadiusDp;
        public float LargeRadiusDp => _LargeRadiusDp;
        public float ThinOutlineDp => _ThinOutlineDp;
        public float RegularOutlineDp => _RegularOutlineDp;
        public float StrongOutlineDp => _StrongOutlineDp;
        public float MaximumOutlineProportion => _MaximumOutlineProportion;
        public Vector2 ShadowOffsetDp => _ShadowOffsetDp;
        public float ShadowSoftnessDp => _ShadowSoftnessDp;
        public float ShadowOpacity => _ShadowOpacity;
        public float NineSliceEdgeDp => _NineSliceEdgeDp;
        public float NineSliceMinimumCenterDp => _NineSliceMinimumCenterDp;
        public bool PreserveNineSliceBorderAspect => _PreserveNineSliceBorderAspect;
    }

    [Serializable]
    public sealed class UiMotionTokens
    {
        [SerializeField, Min(0f)] private float _InstantSeconds;
        [SerializeField, Min(0f)] private float _PressSeconds = 0.10f;
        [SerializeField, Min(0f)] private float _FeedbackSeconds = 0.18f;
        [SerializeField, Min(0f)] private float _TransitionSeconds = 0.22f;
        [SerializeField, Min(0f)] private float _ModalSeconds = 0.20f;
        [SerializeField, Min(0f)] private float _EmphasisSeconds = 0.38f;
        [SerializeField] private UiEasing _StandardEasing = UiEasing.EaseOutCubic;
        [SerializeField] private UiEasing _EnterEasing = UiEasing.EaseOutBack;
        [SerializeField] private UiEasing _ExitEasing = UiEasing.EaseInCubic;
        [SerializeField, Min(0f)] private float _ReducedPressSeconds = 0.05f;
        [SerializeField, Min(0f)] private float _ReducedFeedbackSeconds = 0.08f;
        [SerializeField, Min(0f)] private float _ReducedTransitionSeconds = 0.08f;
        [SerializeField, Min(0f)] private float _ReducedModalSeconds = 0.08f;
        [SerializeField, Min(0f)] private float _ReducedEmphasisSeconds = 0.10f;
        [SerializeField] private UiEasing _ReducedEasing = UiEasing.Linear;
        [SerializeField] private bool _ReducedMotionAllowsShake;
        [SerializeField] private bool _UseUnscaledTime = true;

        public float InstantSeconds => _InstantSeconds;
        public float PressSeconds => _PressSeconds;
        public float FeedbackSeconds => _FeedbackSeconds;
        public float TransitionSeconds => _TransitionSeconds;
        public float ModalSeconds => _ModalSeconds;
        public float EmphasisSeconds => _EmphasisSeconds;
        public UiEasing StandardEasing => _StandardEasing;
        public UiEasing EnterEasing => _EnterEasing;
        public UiEasing ExitEasing => _ExitEasing;
        public float ReducedPressSeconds => _ReducedPressSeconds;
        public float ReducedFeedbackSeconds => _ReducedFeedbackSeconds;
        public float ReducedTransitionSeconds => _ReducedTransitionSeconds;
        public float ReducedModalSeconds => _ReducedModalSeconds;
        public float ReducedEmphasisSeconds => _ReducedEmphasisSeconds;
        public UiEasing ReducedEasing => _ReducedEasing;
        public bool ReducedMotionAllowsShake => _ReducedMotionAllowsShake;
        public bool UseUnscaledTime => _UseUnscaledTime;
    }

    [Serializable]
    public sealed class UiPresentationLayerTokens
    {
        [SerializeField] private string _UiSortingLayer = "UI";
        [SerializeField] private int _WorldOrder;
        [SerializeField] private int _HudOrder = 100;
        [SerializeField] private int _ScrimOrder = 200;
        [SerializeField] private int _ModalOrder = 300;
        [SerializeField] private int _ToastOrder = 400;
        [SerializeField] private int _CriticalFeedbackOrder = 500;

        public string UiSortingLayer => _UiSortingLayer;
        public int WorldOrder => _WorldOrder;
        public int HudOrder => _HudOrder;
        public int ScrimOrder => _ScrimOrder;
        public int ModalOrder => _ModalOrder;
        public int ToastOrder => _ToastOrder;
        public int CriticalFeedbackOrder => _CriticalFeedbackOrder;
    }

    [Serializable]
    public sealed class UiAccessibilityTokens
    {
        [SerializeField, Min(1f)] private float _MinimumTouchTargetDp = UiTokenDefaults.MinimumTouchTargetDp;
        [SerializeField, Min(0.01f)] private float _ReferencePixelsPerDp = UiTokenDefaults.ReferencePixelsPerDp;
        [SerializeField] private Vector2Int _ReferenceResolution = new Vector2Int(1080, 1920);
        [SerializeField, Range(0f, 1f)] private float _CanvasWidthMatch = UiTokenDefaults.CanvasWidthMatch;
        [SerializeField] private bool _ApplyDeviceSafeArea = true;
        [SerializeField] private SafeAreaEdges _SafeAreaEdges = SafeAreaEdges.All;
        [SerializeField, Min(0f)] private float _SafeContentMarginDp = UiTokenDefaults.SafeContentMarginDp;
        [SerializeField, Min(1f)] private float _BodyTextContrast = 4.5f;
        [SerializeField, Min(1f)] private float _LargeTextAndBoundaryContrast = 3f;

        public float MinimumTouchTargetDp => _MinimumTouchTargetDp;
        public float ReferencePixelsPerDp => _ReferencePixelsPerDp;
        public Vector2Int ReferenceResolution => _ReferenceResolution;
        public float CanvasWidthMatch => _CanvasWidthMatch;
        public bool ApplyDeviceSafeArea => _ApplyDeviceSafeArea;
        public SafeAreaEdges SafeAreaEdges => _SafeAreaEdges;
        public float SafeContentMarginDp => _SafeContentMarginDp;
        public float BodyTextContrast => _BodyTextContrast;
        public float LargeTextAndBoundaryContrast => _LargeTextAndBoundaryContrast;
    }

    /// <summary>
    /// Central semantic UI vocabulary for Hand-Carved Forest Arcade. It deliberately contains
    /// presentation policy only: no screen routes, prices, rewards, inventory, or gameplay values.
    /// </summary>
    [CreateAssetMenu(fileName = "UiSemanticTokens", menuName = "Game/UI/Semantic Tokens")]
    public sealed class UiSemanticTokens : ScriptableObject
    {
        public const string ProductionAssetPath = "Assets/Configurations/UI/UiSemanticTokens.asset";
        public const int CurrentSchemaVersion = 1;

        [SerializeField] private int _SchemaVersion = CurrentSchemaVersion;
        [SerializeField] private UiColorTokens _Colors = new UiColorTokens();
        [SerializeField] private UiTypographyTokens _Typography = new UiTypographyTokens();
        [SerializeField] private UiLayoutTokens _Layout = new UiLayoutTokens();
        [SerializeField] private UiSurfaceTokens _Surfaces = new UiSurfaceTokens();
        [SerializeField] private UiMotionTokens _Motion = new UiMotionTokens();
        [SerializeField] private UiPresentationLayerTokens _Layers = new UiPresentationLayerTokens();
        [SerializeField] private UiAccessibilityTokens _Accessibility = new UiAccessibilityTokens();

        public int SchemaVersion => _SchemaVersion;
        public UiColorTokens Colors => _Colors;
        public UiTypographyTokens Typography => _Typography;
        public UiLayoutTokens Layout => _Layout;
        public UiSurfaceTokens Surfaces => _Surfaces;
        public UiMotionTokens Motion => _Motion;
        public UiPresentationLayerTokens Layers => _Layers;
        public UiAccessibilityTokens Accessibility => _Accessibility;

        public IReadOnlyList<string> GetValidationIssues()
        {
            var issues = new List<string>();
            if (_SchemaVersion != CurrentSchemaVersion)
                issues.Add($"Schema version must be {CurrentSchemaVersion}.");

            ValidateColors(issues);
            ValidateTypography(issues);
            ValidateLayout(issues);
            ValidateSurfaces(issues);
            ValidateMotion(issues);
            ValidateLayers(issues);
            ValidateAccessibility(issues);
            return issues;
        }

        private void ValidateColors(ICollection<string> issues)
        {
            if (_Colors == null)
            {
                issues.Add("Color tokens are missing.");
                return;
            }

            var unique = new HashSet<uint>();
            foreach (UiColorRole role in Enum.GetValues(typeof(UiColorRole)))
            {
                Color32 color = _Colors.Get(role);
                uint rgba = ((uint)color.r << 24) | ((uint)color.g << 16) | ((uint)color.b << 8) | color.a;
                if (!unique.Add(rgba))
                    issues.Add($"Semantic color {role} duplicates another role ({ColorUtility.ToHtmlStringRGBA(color)}).");
            }
        }

        private void ValidateTypography(ICollection<string> issues)
        {
            if (_Typography == null)
            {
                issues.Add("Typography tokens are missing.");
                return;
            }

            foreach (UiTypographyRole role in Enum.GetValues(typeof(UiTypographyRole)))
            {
                UiTypographyStyle style = _Typography.Get(role);
                if (style == null)
                {
                    issues.Add($"Typography role {role} is missing.");
                    continue;
                }

                if (style.FontAsset == null)
                    issues.Add($"Typography role {role} has no font asset.");
                if (!IsPositiveFinite(style.SizeDp) || !IsPositiveFinite(style.MinimumSizeDp))
                    issues.Add($"Typography role {role} has an invalid size.");
                if (style.MinimumSizeDp > style.SizeDp)
                    issues.Add($"Typography role {role} minimum size exceeds its nominal size.");
                if (!IsPositiveFinite(style.LineHeightPercent))
                    issues.Add($"Typography role {role} has an invalid line height.");
            }
        }

        private void ValidateLayout(ICollection<string> issues)
        {
            if (_Layout == null)
            {
                issues.Add("Layout tokens are missing.");
                return;
            }

            if (!Approximately(_Layout.GridDp, UiTokenDefaults.GridDp))
                issues.Add($"Base grid must remain {UiTokenDefaults.GridDp:0.#} dp.");
            if (!Approximately(_Layout.HalfGridDp * 2f, _Layout.GridDp))
                issues.Add("Half-grid spacing must be half the base grid.");
            if (!IsAscending(
                    _Layout.CompactGapDp,
                    _Layout.RegularGapDp,
                    _Layout.RoomyGapDp,
                    _Layout.SectionGapDp))
                issues.Add("Spacing steps must be strictly ascending.");
            if (!IsAscending(
                    _Layout.CompactControlHeightDp,
                    _Layout.RegularControlHeightDp,
                    _Layout.LargeControlHeightDp))
                issues.Add("Control heights must be strictly ascending.");
            if (!IsAscending(_Layout.SmallIconDp, _Layout.RegularIconDp, _Layout.LargeIconDp, _Layout.HeroIconDp))
                issues.Add("Icon sizes must be strictly ascending.");
        }

        private void ValidateSurfaces(ICollection<string> issues)
        {
            if (_Surfaces == null)
            {
                issues.Add("Surface tokens are missing.");
                return;
            }

            if (!IsAscending(_Surfaces.CompactRadiusDp, _Surfaces.RegularRadiusDp, _Surfaces.LargeRadiusDp))
                issues.Add("Corner radii must be strictly ascending.");
            if (!IsAscending(_Surfaces.ThinOutlineDp, _Surfaces.RegularOutlineDp, _Surfaces.StrongOutlineDp))
                issues.Add("Outline weights must be strictly ascending.");
            if (_Surfaces.MaximumOutlineProportion <= 0f || _Surfaces.MaximumOutlineProportion > 0.04f)
                issues.Add("Maximum outline proportion must stay within the approved 0-4% range.");
            if (_Surfaces.ShadowOffsetDp.x < 0f || _Surfaces.ShadowOffsetDp.y > 0f)
                issues.Add("Structural shadows must use the approved lower-right direction.");
            if (!IsPositiveFinite(_Surfaces.NineSliceEdgeDp) || !IsPositiveFinite(_Surfaces.NineSliceMinimumCenterDp))
                issues.Add("Nine-slice edge and center values must be positive.");
        }

        private void ValidateMotion(ICollection<string> issues)
        {
            if (_Motion == null)
            {
                issues.Add("Motion tokens are missing.");
                return;
            }

            if (_Motion.InstantSeconds != 0f)
                issues.Add("Instant motion must be zero seconds.");
            if (!IsNonNegativeFinite(
                    _Motion.PressSeconds,
                    _Motion.FeedbackSeconds,
                    _Motion.TransitionSeconds,
                    _Motion.ModalSeconds,
                    _Motion.EmphasisSeconds))
                issues.Add("Standard motion durations must be finite and non-negative.");
            if (!IsNonNegativeFinite(
                    _Motion.ReducedPressSeconds,
                    _Motion.ReducedFeedbackSeconds,
                    _Motion.ReducedTransitionSeconds,
                    _Motion.ReducedModalSeconds,
                    _Motion.ReducedEmphasisSeconds))
                issues.Add("Reduced-motion durations must be finite and non-negative.");
            if (_Motion.ReducedPressSeconds > _Motion.PressSeconds ||
                _Motion.ReducedFeedbackSeconds > _Motion.FeedbackSeconds ||
                _Motion.ReducedTransitionSeconds > _Motion.TransitionSeconds ||
                _Motion.ReducedModalSeconds > _Motion.ModalSeconds ||
                _Motion.ReducedEmphasisSeconds > _Motion.EmphasisSeconds)
                issues.Add("Every reduced-motion duration must be no longer than its standard counterpart.");
            if (_Motion.ReducedMotionAllowsShake)
                issues.Add("Reduced motion must not depend on shake.");
        }

        private void ValidateLayers(ICollection<string> issues)
        {
            if (_Layers == null)
            {
                issues.Add("Presentation layer tokens are missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_Layers.UiSortingLayer))
                issues.Add("UI sorting layer name is empty.");
            if (!IsAscending(
                    _Layers.WorldOrder,
                    _Layers.HudOrder,
                    _Layers.ScrimOrder,
                    _Layers.ModalOrder,
                    _Layers.ToastOrder,
                    _Layers.CriticalFeedbackOrder))
                issues.Add("Presentation layers must be strictly ordered World < HUD < Scrim < Modal < Toast < CriticalFeedback.");
        }

        private void ValidateAccessibility(ICollection<string> issues)
        {
            if (_Accessibility == null)
            {
                issues.Add("Accessibility tokens are missing.");
                return;
            }

            if (!Approximately(_Accessibility.MinimumTouchTargetDp, UiTokenDefaults.MinimumTouchTargetDp))
                issues.Add($"Minimum touch target must remain {UiTokenDefaults.MinimumTouchTargetDp:0.#} dp.");
            if (!Approximately(_Accessibility.ReferencePixelsPerDp, UiTokenDefaults.ReferencePixelsPerDp))
                issues.Add($"Reference pixel density must remain {UiTokenDefaults.ReferencePixelsPerDp:0.#} pixels per dp.");
            if (_Accessibility.ReferenceResolution != UiTokenDefaults.ReferenceResolution)
                issues.Add($"Reference resolution must remain {UiTokenDefaults.ReferenceResolution.x}x{UiTokenDefaults.ReferenceResolution.y}.");
            if (!Approximately(_Accessibility.CanvasWidthMatch, UiTokenDefaults.CanvasWidthMatch))
                issues.Add($"Canvas width/height match must remain {UiTokenDefaults.CanvasWidthMatch:0.##}.");
            if (!_Accessibility.ApplyDeviceSafeArea || _Accessibility.SafeAreaEdges != SafeAreaEdges.All)
                issues.Add("Production UI must apply all device safe-area edges.");
            if (_Accessibility.SafeContentMarginDp < UiTokenDefaults.SafeContentMarginDp)
                issues.Add($"Safe content margin must be at least {UiTokenDefaults.SafeContentMarginDp:0.#} dp.");
            if (_Accessibility.BodyTextContrast < 4.5f || _Accessibility.LargeTextAndBoundaryContrast < 3f)
                issues.Add("Contrast defaults are below the approved 4.5:1 body / 3:1 large-text boundary thresholds.");
        }

        private static bool IsAscending(params float[] values)
        {
            for (var i = 1; i < values.Length; i++)
            {
                if (!IsPositiveFinite(values[i]) || values[i] <= values[i - 1])
                    return false;
            }

            return values.Length > 0 && values[0] >= 0f && IsFinite(values[0]);
        }

        private static bool IsAscending(params int[] values)
        {
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i] <= values[i - 1])
                    return false;
            }

            return values.Length > 0;
        }

        private static bool IsNonNegativeFinite(params float[] values)
        {
            foreach (float value in values)
            {
                if (value < 0f || !IsFinite(value))
                    return false;
            }

            return true;
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && IsFinite(value);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.0001f;
        }
    }
}
