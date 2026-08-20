using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.UI.Tokens;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Common
{
    /// <summary>
    /// Shared pressed, disabled, loading, and blocked presentation for an existing Unity UI Button.
    /// Action owners explicitly begin/end submissions; the presenter never changes the action itself.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ActionButtonPresenter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public const string PressFeedbackCueId = "ui.press";

        private static readonly Color FallbackEmber = ParseColor("#F26A2E");
        private static readonly Color FallbackTextIvory = ParseColor("#FFF4DE");

        [SerializeField] private Button _Button;
        [SerializeField] private RectTransform _PresentationRoot;
        [SerializeField] private CanvasGroup _InteractionGroup;
        [SerializeField] private GameObject _LoadingIndicator;
        [SerializeField] private FeedbackEventChannel _FeedbackChannel;

        [Header("Semantic Skin (Optional)")]
        [SerializeField] private UiSemanticTokens _SemanticTokens;
        [SerializeField] private UiButtonStyle _SkinStyle = UiButtonStyle.Unskinned;
        [SerializeField] private Graphic _HitAreaGraphic;
        [SerializeField] private Image _SurfaceImage;
        [SerializeField] private UiButtonOwnershipMark _OwnershipMark;
        [SerializeField] private Image _OwnershipMarkImage;
        [System.NonSerialized] private bool _ReducedMotionPreview;

        [Header("Fallback Presentation")]
        [SerializeField, Range(0.8f, 1f)] private float _PressedScale = 0.96f;
        [SerializeField, Range(0f, 1f)] private float _DisabledAlpha = 0.55f;
        [SerializeField, Min(0f)] private float _BlockedSeconds = 0.38f;
        [SerializeField, Min(0f)] private float _BlockedShakeDistance = 8f;
        [SerializeField, Min(1f)] private float _ScaleResponse = 24f;

        private readonly ActionButtonStateModel _Model = new();
        private ActionButtonPresentationState _AppliedState = (ActionButtonPresentationState)(-1);
        private Graphic _TargetGraphic;
        private RectTransform _ShakeRoot;
        private CanvasGroup _LoadingCanvasGroup;
        private TMP_Text[] _LoadingHiddenLabels;
        private bool[] _LoadingHiddenLabelStates;
        private bool _CreatedLoadingIndicator;
        private bool _ContentLabelsHidden;
        private bool _HasStateBaseAlpha;
        private float _StateBaseAlpha = 1f;
        private bool _HasStateBaseGraphicColor;
        private Color _StateBaseGraphicColor = Color.white;
        private bool _BaseGroupInteractable = true;
        private bool _BaseGroupBlocksRaycasts = true;
        private bool _HasLoadingInteractionOverride;
        private bool _HasPressBaseScale;
        private Vector3 _PressBaseScale = Vector3.one;
        private Vector3 _ScaleStart = Vector3.one;
        private Vector3 _ScaleTarget = Vector3.one;
        private float _ScaleElapsed;
        private float _ScaleDuration;
        private UiEasing _ScaleEasing = UiEasing.EaseOutCubic;
        private bool _ScaleAnimating;
        private bool _HasBlockedBaseColor;
        private Color _BlockedBaseColor = Color.white;
        private bool _HasBlockedBasePosition;
        private Vector2 _BlockedBasePosition;
        private float _BlockedRemaining;
        private float _LoadingElapsed;
        private float _LoadingPulseMinimumAlpha = 0.65f;
        private float _ActivePressedScale = 0.96f;
        private float _ActiveDisabledAlpha = 0.55f;
        private float _ActiveBlockedSeconds = 0.38f;
        private float _ActiveBlockedShakeDistance = 8f;
        private float _ActiveScaleResponse = 24f;
        private bool _SemanticSkinApplied;
        private bool _HasOriginalSkinState;
        private Sprite _OriginalSurfaceSprite;
        private Image.Type _OriginalSurfaceType;
        private Color _OriginalSurfaceColor = Color.white;
        private bool _OriginalSurfaceRaycastTarget;
        private Color _OriginalHitAreaColor = Color.white;
        private Graphic _OriginalButtonTargetGraphic;
        private Selectable.Transition _OriginalButtonTransition;
        private Sprite _OriginalOwnershipSprite;
        private bool _OriginalOwnershipEnabled;
        private bool _OriginalOwnershipActive;

        public ActionButtonPresentationState State => _Model.State;
        public bool IsLoading => _Model.IsLoading;
        public Button Button => _Button;
        public UiButtonStyle SkinStyle => _SkinStyle;
        public bool IsSemanticSkinApplied => _SemanticSkinApplied;
        public bool IsReducedMotionPreview => _ReducedMotionPreview;

        public static ActionButtonPresenter GetOrAdd(Button button, FeedbackEventChannel feedbackChannel = null)
        {
            if (button == null)
                return null;

            ActionButtonPresenter presenter = button.GetComponent<ActionButtonPresenter>();
            if (presenter == null)
            {
                presenter = button.gameObject.AddComponent<ActionButtonPresenter>();
            }

            presenter.Configure(button, feedbackChannel);
            return presenter;
        }

        private void Awake()
        {
            AssignReferences();
            TryApplySemanticSkin();
            ApplySemanticPresentationMetrics();
            SyncAvailability();
            ApplyState(force: true);
        }

        private void OnEnable()
        {
            AssignReferences();
            TryApplySemanticSkin();
            ApplySemanticPresentationMetrics();
            SyncAvailability();
            ApplyState(force: true);
        }

        private void OnDisable()
        {
            _Model.ResetTransient();
            _BlockedRemaining = 0f;
            RestorePresentation();
            _AppliedState = (ActionButtonPresentationState)(-1);
        }

        private void OnDestroy()
        {
            RestorePresentation();
            RestoreSemanticSkin();
            if (_CreatedLoadingIndicator && _LoadingIndicator != null)
            {
                Destroy(_LoadingIndicator);
            }
        }

        private void Update()
        {
            AdvancePresentation(Time.unscaledDeltaTime);
        }

        public void Configure(Button button, FeedbackEventChannel feedbackChannel = null)
        {
            if (button != null)
            {
                _Button = button;
            }

            if (feedbackChannel != null)
            {
                _FeedbackChannel = feedbackChannel;
            }

            AssignReferences();
            SyncAvailability();
            ApplyState(force: true);
        }

        /// <summary>
        /// Applies an explicit production skin without inferring screen or domain meaning. A root-sized
        /// raycast Graphic and a visual-only child root are required so pressed scale never shrinks the
        /// Button's hit rectangle. If any reference is absent, prior Unity Button visuals remain untouched.
        /// </summary>
        public bool ConfigureSkin(
            UiSemanticTokens semanticTokens,
            UiButtonStyle style,
            Image surfaceImage,
            RectTransform presentationRoot,
            UiButtonOwnershipMark ownershipMark = UiButtonOwnershipMark.None,
            Image ownershipMarkImage = null,
            Graphic hitAreaGraphic = null)
        {
            if (!CanApplySemanticSkin(
                    semanticTokens,
                    style,
                    hitAreaGraphic,
                    surfaceImage,
                    presentationRoot,
                    ownershipMark,
                    ownershipMarkImage,
                    out _,
                    out _,
                    out Graphic resolvedHitAreaGraphic))
            {
                return false;
            }

            RestorePresentation();
            RestoreSemanticSkin();

            _SemanticTokens = semanticTokens;
            _SkinStyle = style;
            _HitAreaGraphic = resolvedHitAreaGraphic;
            _SurfaceImage = surfaceImage;
            _PresentationRoot = presentationRoot;
            _ShakeRoot = presentationRoot;
            _OwnershipMark = ownershipMark;
            _OwnershipMarkImage = ownershipMarkImage;

            AssignReferences();
            bool applied = TryApplySemanticSkin();
            ApplySemanticPresentationMetrics();
            SyncAvailability();
            ApplyState(force: true);
            return applied;
        }

        /// <summary>
        /// Presentation-only preview override for deterministic validation. This does not create a
        /// player setting, persistence rule, or product default; UX-406 remains the policy owner.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void SetReducedMotionPreview(bool reducedMotion)
        {
#if UNITY_EDITOR
            _ReducedMotionPreview = reducedMotion;
            ApplySemanticPresentationMetrics();

            if (_Model.IsBlocked)
            {
                _BlockedRemaining = Mathf.Min(_BlockedRemaining, _ActiveBlockedSeconds);
            }

            if (reducedMotion && _ShakeRoot != null && _HasBlockedBasePosition)
            {
                _ShakeRoot.anchoredPosition = _BlockedBasePosition;
            }

            ApplyState(force: true);
            AdvanceLoadingIndicator(0f);
#endif
        }

        public bool TryBeginSubmission()
        {
            AssignReferences();
            SyncAvailability();
            if (_Button == null || !_Button.IsActive() || !_Button.IsInteractable() || !_Model.TryBeginSubmission())
                return false;

            ApplyState();
            return true;
        }

        public void CompleteSubmission()
        {
            _Model.CompleteSubmission();
            ApplyState();
        }

        /// <summary>
        /// Presentation-only cancellation cleanup. It has the same state effect as completing a
        /// submission and never changes screen-owned action truth or emits an outcome.
        /// </summary>
        public void CancelSubmission()
        {
            CompleteSubmission();
        }

        public void PresentBlocked(FeedbackEventData feedback)
        {
            if (feedback.Kind != FeedbackKind.Blocked && feedback.Kind != FeedbackKind.InsufficientFunds)
            {
                throw new System.ArgumentException(
                    "Button blocked presentation requires Blocked or InsufficientFunds feedback.",
                    nameof(feedback));
            }

            _Model.CompleteSubmission();
            _Model.SetBlocked(true);
            _BlockedRemaining = Mathf.Max(0f, _ActiveBlockedSeconds);
            ApplyState();
            _FeedbackChannel?.Raise(feedback);
        }

        public void SetAvailable(bool available)
        {
            if (_Button != null)
            {
                _Button.interactable = available;
            }

            _Model.SetAvailable(available);
            ApplyState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SyncAvailability();
            if (_Button == null || !_Button.IsInteractable())
                return;

            CapturePressBaseScale();
            _Model.SetPressed(true);
            ApplyState();
            _FeedbackChannel?.Raise(new FeedbackEventData(
                FeedbackKind.Navigation,
                PressFeedbackCueId,
                gameObject.name));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _Model.SetPressed(false);
            ApplyState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _Model.SetPressed(false);
            ApplyState();
        }

        /// <summary>
        /// Advances transient presentation using unscaled time. Public for deterministic EditMode tests.
        /// </summary>
        public void AdvancePresentation(float unscaledDeltaTime)
        {
            float delta = Mathf.Max(0f, unscaledDeltaTime);
            SyncAvailability();

            if (_Model.IsBlocked)
            {
                _BlockedRemaining -= delta;
                if (_BlockedRemaining <= 0f)
                {
                    _Model.SetBlocked(false);
                }
            }

            ApplyState();
            AdvanceScale(delta);
            AdvanceBlockedShake();
            AdvanceLoadingIndicator(delta);
        }

        private void SyncAvailability()
        {
            if (_Button != null)
            {
                _Model.SetAvailable(_Button.interactable);
            }
        }

        private void ApplyState(bool force = false)
        {
            ActionButtonPresentationState state = _Model.State;
            if (!force && state == _AppliedState)
                return;

            ExitState(_AppliedState);
            _AppliedState = state;
            EnterState(state);
        }

        private void EnterState(ActionButtonPresentationState state)
        {
            switch (state)
            {
                case ActionButtonPresentationState.Pressed:
                    CapturePressBaseScale();
                    if (_SemanticSkinApplied)
                    {
                        CaptureStateBaseGraphicColor();
                    }
                    SetScaleTarget(_PressBaseScale * _ActivePressedScale);
                    if (_SemanticSkinApplied && _TargetGraphic != null)
                    {
                        _TargetGraphic.color = Color.Lerp(
                            _StateBaseGraphicColor,
                            SemanticColor(UiColorRole.Ink, _StateBaseGraphicColor),
                            _SemanticTokens.Buttons.PressedTintStrength);
                    }
                    break;
                case ActionButtonPresentationState.Disabled:
                    CaptureStateBaseAlpha();
                    if (_SemanticSkinApplied)
                    {
                        CaptureStateBaseGraphicColor();
                    }
                    if (_InteractionGroup != null)
                    {
                        _InteractionGroup.alpha = _StateBaseAlpha * _ActiveDisabledAlpha;
                    }
                    if (_SemanticSkinApplied && _TargetGraphic != null)
                    {
                        _TargetGraphic.color = Color.Lerp(
                            _StateBaseGraphicColor,
                            SemanticColor(UiColorRole.Disabled, _StateBaseGraphicColor),
                            _SemanticTokens.Buttons.DisabledTintStrength);
                    }
                    break;
                case ActionButtonPresentationState.Loading:
                    CaptureStateBaseAlpha();
                    SetLoadingInteraction(false);
                    ShowLoadingIndicator();
                    break;
                case ActionButtonPresentationState.Blocked:
                    CaptureBlockedVisuals();
                    if (_TargetGraphic != null)
                    {
                        _TargetGraphic.color = SemanticColor(UiColorRole.BlockedFeedback, FallbackEmber);
                    }
                    break;
            }
        }

        private void ExitState(ActionButtonPresentationState state)
        {
            switch (state)
            {
                case ActionButtonPresentationState.Pressed:
                    if (_SemanticSkinApplied)
                    {
                        RestoreStateBaseGraphicColor();
                    }
                    if (_HasPressBaseScale)
                    {
                        SetScaleTarget(_PressBaseScale);
                    }
                    break;
                case ActionButtonPresentationState.Disabled:
                    RestoreStateBaseAlpha();
                    if (_SemanticSkinApplied)
                    {
                        RestoreStateBaseGraphicColor();
                    }
                    break;
                case ActionButtonPresentationState.Loading:
                    HideLoadingIndicator();
                    SetLoadingInteraction(true);
                    RestoreStateBaseAlpha();
                    break;
                case ActionButtonPresentationState.Blocked:
                    RestoreBlockedVisuals();
                    break;
            }
        }

        private void CapturePressBaseScale()
        {
            if (_PresentationRoot == null || _HasPressBaseScale)
                return;

            _PressBaseScale = _PresentationRoot.localScale;
            _ScaleTarget = _PressBaseScale;
            _HasPressBaseScale = true;
        }

        private void SetScaleTarget(Vector3 target)
        {
            _ScaleTarget = target;
            _ScaleAnimating = _PresentationRoot != null;
            if (_PresentationRoot == null)
                return;

            _ScaleStart = _PresentationRoot.localScale;
            _ScaleElapsed = 0f;
            if (_SemanticSkinApplied && _SemanticTokens != null && _SemanticTokens.Motion != null)
            {
                _ScaleDuration = _ReducedMotionPreview
                    ? _SemanticTokens.Motion.ReducedPressSeconds
                    : _SemanticTokens.Motion.PressSeconds;
                _ScaleEasing = _ReducedMotionPreview
                    ? _SemanticTokens.Motion.ReducedEasing
                    : _SemanticTokens.Motion.StandardEasing;
                if (_ScaleDuration <= 0f)
                {
                    _PresentationRoot.localScale = _ScaleTarget;
                    _ScaleAnimating = false;
                }
            }
        }

        private void AdvanceScale(float delta)
        {
            if (!_ScaleAnimating || _PresentationRoot == null)
                return;

            if (_SemanticSkinApplied)
            {
                _ScaleElapsed += delta;
                float progress = _ScaleDuration <= 0f ? 1f : Mathf.Clamp01(_ScaleElapsed / _ScaleDuration);
                _PresentationRoot.localScale = Vector3.LerpUnclamped(
                    _ScaleStart,
                    _ScaleTarget,
                    EvaluateEasing(_ScaleEasing, progress));
            }
            else
            {
                float blend = 1f - Mathf.Exp(-_ActiveScaleResponse * delta);
                _PresentationRoot.localScale = Vector3.Lerp(_PresentationRoot.localScale, _ScaleTarget, blend);
            }

            if ((_PresentationRoot.localScale - _ScaleTarget).sqrMagnitude > 0.000001f)
                return;

            _PresentationRoot.localScale = _ScaleTarget;
            _ScaleAnimating = false;
            if (_Model.State != ActionButtonPresentationState.Pressed)
            {
                _HasPressBaseScale = false;
            }
        }

        private void CaptureStateBaseAlpha()
        {
            if (_InteractionGroup == null || _HasStateBaseAlpha)
                return;

            _StateBaseAlpha = _InteractionGroup.alpha;
            _HasStateBaseAlpha = true;
        }

        private void RestoreStateBaseAlpha()
        {
            if (_InteractionGroup != null && _HasStateBaseAlpha)
            {
                _InteractionGroup.alpha = _StateBaseAlpha;
            }

            _HasStateBaseAlpha = false;
        }

        private void CaptureStateBaseGraphicColor()
        {
            if (_TargetGraphic == null || _HasStateBaseGraphicColor)
                return;

            _StateBaseGraphicColor = _TargetGraphic.color;
            _HasStateBaseGraphicColor = true;
        }

        private void RestoreStateBaseGraphicColor()
        {
            if (_TargetGraphic != null && _HasStateBaseGraphicColor)
            {
                _TargetGraphic.color = _StateBaseGraphicColor;
            }

            _HasStateBaseGraphicColor = false;
        }

        private void SetLoadingInteraction(bool restore)
        {
            if (_InteractionGroup == null)
                return;

            if (restore)
            {
                if (!_HasLoadingInteractionOverride)
                    return;

                _InteractionGroup.interactable = _BaseGroupInteractable;
                _InteractionGroup.blocksRaycasts = _BaseGroupBlocksRaycasts;
                _HasLoadingInteractionOverride = false;
                return;
            }

            if (!_HasLoadingInteractionOverride)
            {
                _BaseGroupInteractable = _InteractionGroup.interactable;
                _BaseGroupBlocksRaycasts = _InteractionGroup.blocksRaycasts;
                _HasLoadingInteractionOverride = true;
            }
            _InteractionGroup.interactable = false;
            _InteractionGroup.blocksRaycasts = true;
        }

        private void CaptureBlockedVisuals()
        {
            if (_TargetGraphic != null && !_HasBlockedBaseColor)
            {
                _BlockedBaseColor = _TargetGraphic.color;
                _HasBlockedBaseColor = true;
            }

            if (_ShakeRoot != null && !_HasBlockedBasePosition)
            {
                _BlockedBasePosition = _ShakeRoot.anchoredPosition;
                _HasBlockedBasePosition = true;
            }
        }

        private void AdvanceBlockedShake()
        {
            if (!_Model.IsBlocked || _ShakeRoot == null || !_HasBlockedBasePosition)
                return;

            if (!_SemanticSkinApplied)
            {
                float fallbackElapsed = Mathf.Max(0f, _BlockedSeconds - _BlockedRemaining);
                float fallbackDecay = _BlockedSeconds <= 0f
                    ? 0f
                    : Mathf.Clamp01(_BlockedRemaining / _BlockedSeconds);
                float fallbackOffset = Mathf.Sin(fallbackElapsed * 70f) * _BlockedShakeDistance * fallbackDecay;
                _ShakeRoot.anchoredPosition = _BlockedBasePosition + Vector2.right * fallbackOffset;
                return;
            }

            if (_ActiveBlockedShakeDistance <= 0f)
            {
                _ShakeRoot.anchoredPosition = _BlockedBasePosition;
                return;
            }

            float elapsed = Mathf.Max(0f, _ActiveBlockedSeconds - _BlockedRemaining);
            float decay = _ActiveBlockedSeconds <= 0f
                ? 0f
                : Mathf.Clamp01(_BlockedRemaining / _ActiveBlockedSeconds);
            float normalized = _ActiveBlockedSeconds <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / _ActiveBlockedSeconds);
            float offset = Mathf.Sin(normalized * Mathf.PI * 6f) * _ActiveBlockedShakeDistance * decay;
            _ShakeRoot.anchoredPosition = _BlockedBasePosition + Vector2.right * offset;
        }

        private void RestoreBlockedVisuals()
        {
            if (_TargetGraphic != null && _HasBlockedBaseColor)
            {
                _TargetGraphic.color = _BlockedBaseColor;
            }

            if (_ShakeRoot != null && _HasBlockedBasePosition)
            {
                _ShakeRoot.anchoredPosition = _BlockedBasePosition;
            }

            _HasBlockedBaseColor = false;
            _HasBlockedBasePosition = false;
        }

        private void ShowLoadingIndicator()
        {
            EnsureLoadingIndicator();
            _LoadingElapsed = 0f;
            if (_LoadingIndicator != null)
            {
                if (!_SemanticSkinApplied)
                {
                    SetContentLabelsVisible(false);
                }
                else if (_LoadingCanvasGroup != null)
                {
                    _LoadingCanvasGroup.alpha = _ReducedMotionPreview
                        ? 1f
                        : _LoadingPulseMinimumAlpha;
                }
                _LoadingIndicator.SetActive(true);
            }
        }

        private void HideLoadingIndicator()
        {
            if (_LoadingIndicator != null)
            {
                _LoadingIndicator.SetActive(false);
            }

            SetContentLabelsVisible(true);
        }

        private void AdvanceLoadingIndicator(float delta)
        {
            if (!_Model.IsLoading || _LoadingCanvasGroup == null)
                return;

            if (!_SemanticSkinApplied)
            {
                _LoadingElapsed += delta;
                _LoadingCanvasGroup.alpha = 0.65f + Mathf.PingPong(_LoadingElapsed * 1.8f, 0.35f);
                return;
            }

            if (_ReducedMotionPreview)
            {
                _LoadingCanvasGroup.alpha = 1f;
                return;
            }

            _LoadingElapsed += delta;
            float pulseSeconds = _SemanticTokens != null && _SemanticTokens.Motion != null
                ? Mathf.Max(0.01f, _SemanticTokens.Motion.FeedbackSeconds)
                : 0.56f;
            float pulse = Mathf.PingPong(_LoadingElapsed / pulseSeconds, 1f);
            _LoadingCanvasGroup.alpha = Mathf.Lerp(_LoadingPulseMinimumAlpha, 1f, pulse);
        }

        private void EnsureLoadingIndicator()
        {
            TMP_Text source = GetComponentInChildren<TMP_Text>(true);
            if (_LoadingIndicator == null)
            {
                var indicator = new GameObject(
                    "LoadingIndicator",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI),
                    typeof(CanvasGroup));
                Transform parent = _SemanticSkinApplied && _PresentationRoot != null
                    ? _PresentationRoot
                    : transform;
                indicator.transform.SetParent(parent, false);
                var rect = indicator.GetComponent<RectTransform>();

                var text = indicator.GetComponent<TextMeshProUGUI>();
                text.text = "•••";
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
                text.color = source != null
                    ? source.color
                    : SemanticColor(UiColorRole.Ivory, FallbackTextIvory);
                text.fontSize = source != null ? Mathf.Max(18f, source.fontSize * 0.85f) : 24f;
                if (source != null)
                {
                    text.font = source.font;
                }

                _LoadingIndicator = indicator;
                _CreatedLoadingIndicator = true;
            }

            if (_CreatedLoadingIndicator && _LoadingIndicator != null)
            {
                Transform parent = _SemanticSkinApplied && _PresentationRoot != null
                    ? _PresentationRoot
                    : transform;
                if (_LoadingIndicator.transform.parent != parent)
                {
                    _LoadingIndicator.transform.SetParent(parent, false);
                }

                RectTransform rect = _LoadingIndicator.GetComponent<RectTransform>();
                if (_SemanticSkinApplied)
                {
                    rect.anchorMin = new Vector2(0.78f, 0.18f);
                    rect.anchorMax = new Vector2(0.95f, 0.82f);
                }
                else
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                }
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            _LoadingCanvasGroup = _LoadingIndicator.GetComponent<CanvasGroup>();
            if (_LoadingCanvasGroup == null)
            {
                _LoadingCanvasGroup = _LoadingIndicator.AddComponent<CanvasGroup>();
            }

            _LoadingCanvasGroup.interactable = false;
            _LoadingCanvasGroup.blocksRaycasts = false;
            CacheContentLabels();
        }

        private void CacheContentLabels()
        {
            TMP_Text[] labels = GetComponentsInChildren<TMP_Text>(true);
            var contentLabels = new System.Collections.Generic.List<TMP_Text>();
            for (int i = 0; i < labels.Length; i++)
            {
                TMP_Text label = labels[i];
                if (label != null && (_LoadingIndicator == null || !label.transform.IsChildOf(_LoadingIndicator.transform)))
                {
                    contentLabels.Add(label);
                }
            }

            _LoadingHiddenLabels = contentLabels.ToArray();
            _LoadingHiddenLabelStates = new bool[_LoadingHiddenLabels.Length];
        }

        private void SetContentLabelsVisible(bool visible)
        {
            if (_LoadingHiddenLabels == null || _LoadingHiddenLabelStates == null)
                return;

            if (visible && !_ContentLabelsHidden)
                return;
            if (!visible && _ContentLabelsHidden)
                return;

            for (int i = 0; i < _LoadingHiddenLabels.Length; i++)
            {
                TMP_Text label = _LoadingHiddenLabels[i];
                if (label == null)
                    continue;

                if (!visible)
                {
                    _LoadingHiddenLabelStates[i] = label.enabled;
                    label.enabled = false;
                }
                else
                {
                    label.enabled = _LoadingHiddenLabelStates[i];
                }
            }

            _ContentLabelsHidden = !visible;
        }

        private void AssignReferences()
        {
            _Button ??= GetComponent<Button>();
            _PresentationRoot ??= transform as RectTransform;
            _HitAreaGraphic ??= _Button != null ? _Button.targetGraphic : null;
            _ShakeRoot = _SemanticSkinApplied && _PresentationRoot != null
                ? _PresentationRoot
                : _ShakeRoot ?? _PresentationRoot;
            _SurfaceImage ??= _Button != null ? _Button.targetGraphic as Image : null;
            _TargetGraphic = _SemanticSkinApplied && _SurfaceImage != null
                ? _SurfaceImage
                : _Button != null
                    ? _Button.targetGraphic
                    : GetComponent<Graphic>();

            if (_InteractionGroup == null)
            {
                _InteractionGroup = GetComponent<CanvasGroup>();
                if (_InteractionGroup == null)
                {
                    _InteractionGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_InteractionGroup != null && _AppliedState == (ActionButtonPresentationState)(-1))
            {
                _BaseGroupInteractable = _InteractionGroup.interactable;
                _BaseGroupBlocksRaycasts = _InteractionGroup.blocksRaycasts;
            }
        }

        private bool TryApplySemanticSkin()
        {
            if (_SemanticSkinApplied)
                return true;

            if (!CanApplySemanticSkin(
                    _SemanticTokens,
                    _SkinStyle,
                    _HitAreaGraphic,
                    _SurfaceImage,
                    _PresentationRoot,
                    _OwnershipMark,
                    _OwnershipMarkImage,
                    out Sprite surfaceSprite,
                    out Sprite ownershipSprite,
                    out Graphic hitAreaGraphic))
            {
                return false;
            }

            _OriginalSurfaceSprite = _SurfaceImage.sprite;
            _OriginalSurfaceType = _SurfaceImage.type;
            _OriginalSurfaceColor = _SurfaceImage.color;
            _OriginalSurfaceRaycastTarget = _SurfaceImage.raycastTarget;
            _OriginalHitAreaColor = hitAreaGraphic.color;
            _OriginalButtonTargetGraphic = _Button.targetGraphic;
            _OriginalButtonTransition = _Button.transition;

            if (_OwnershipMarkImage != null)
            {
                _OriginalOwnershipSprite = _OwnershipMarkImage.sprite;
                _OriginalOwnershipEnabled = _OwnershipMarkImage.enabled;
                _OriginalOwnershipActive = _OwnershipMarkImage.gameObject.activeSelf;
            }

            _HasOriginalSkinState = true;
            _SurfaceImage.sprite = surfaceSprite;
            _SurfaceImage.type = Image.Type.Sliced;
            _SurfaceImage.color = Color.white;
            _SurfaceImage.raycastTarget = false;
            _HitAreaGraphic = hitAreaGraphic;
            _HitAreaGraphic.color = Color.clear;
            _Button.targetGraphic = _HitAreaGraphic;
            _Button.transition = Selectable.Transition.None;

            if (_OwnershipMarkImage != null)
            {
                bool showOwnership = _OwnershipMark != UiButtonOwnershipMark.None;
                _OwnershipMarkImage.sprite = showOwnership ? ownershipSprite : null;
                _OwnershipMarkImage.enabled = showOwnership;
                if (showOwnership)
                {
                    _OwnershipMarkImage.gameObject.SetActive(true);
                }
            }

            _SemanticSkinApplied = true;
            _TargetGraphic = _SurfaceImage;
            _ShakeRoot = _PresentationRoot;
            ApplySemanticPresentationMetrics();
            return true;
        }

        private bool CanApplySemanticSkin(
            UiSemanticTokens semanticTokens,
            UiButtonStyle style,
            Graphic hitAreaGraphic,
            Image surfaceImage,
            RectTransform presentationRoot,
            UiButtonOwnershipMark ownershipMark,
            Image ownershipMarkImage,
            out Sprite surfaceSprite,
            out Sprite ownershipSprite,
            out Graphic resolvedHitAreaGraphic)
        {
            surfaceSprite = null;
            ownershipSprite = null;
            resolvedHitAreaGraphic = null;

            Button button = _Button != null ? _Button : GetComponent<Button>();
            if (button == null || semanticTokens == null || semanticTokens.Buttons == null ||
                semanticTokens.Colors == null || semanticTokens.Motion == null || semanticTokens.Accessibility == null ||
                style == UiButtonStyle.Unskinned || surfaceImage == null || presentationRoot == null)
            {
                return false;
            }

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect == null || presentationRoot == buttonRect || !presentationRoot.IsChildOf(buttonRect))
                return false;

            resolvedHitAreaGraphic = hitAreaGraphic != null ? hitAreaGraphic : button.targetGraphic;
            if (resolvedHitAreaGraphic == null || resolvedHitAreaGraphic.transform != button.transform ||
                resolvedHitAreaGraphic.rectTransform != buttonRect || !resolvedHitAreaGraphic.raycastTarget ||
                !resolvedHitAreaGraphic.enabled)
            {
                return false;
            }

            Transform surfaceTransform = surfaceImage.transform;
            if (surfaceTransform != presentationRoot && !surfaceTransform.IsChildOf(presentationRoot))
                return false;

            surfaceSprite = semanticTokens.Buttons.GetSurface(style);
            if (surfaceSprite == null)
                return false;

            if (ownershipMarkImage != null)
            {
                if (ownershipMarkImage == surfaceImage)
                    return false;

                Transform ownershipTransform = ownershipMarkImage.transform;
                if (ownershipTransform != presentationRoot && !ownershipTransform.IsChildOf(presentationRoot))
                    return false;
            }

            if (ownershipMark == UiButtonOwnershipMark.None)
                return true;

            if (ownershipMarkImage == null)
                return false;

            ownershipSprite = semanticTokens.Buttons.GetOwnershipMark(ownershipMark);
            return ownershipSprite != null;
        }

        private void RestoreSemanticSkin()
        {
            if (!_HasOriginalSkinState)
            {
                _SemanticSkinApplied = false;
                ApplySemanticPresentationMetrics();
                return;
            }

            if (_SurfaceImage != null)
            {
                _SurfaceImage.sprite = _OriginalSurfaceSprite;
                _SurfaceImage.type = _OriginalSurfaceType;
                _SurfaceImage.color = _OriginalSurfaceColor;
                _SurfaceImage.raycastTarget = _OriginalSurfaceRaycastTarget;
            }

            if (_Button != null)
            {
                _Button.targetGraphic = _OriginalButtonTargetGraphic;
                _Button.transition = _OriginalButtonTransition;
            }

            if (_HitAreaGraphic != null)
            {
                _HitAreaGraphic.color = _OriginalHitAreaColor;
            }

            if (_OwnershipMarkImage != null)
            {
                _OwnershipMarkImage.sprite = _OriginalOwnershipSprite;
                _OwnershipMarkImage.enabled = _OriginalOwnershipEnabled;
                _OwnershipMarkImage.gameObject.SetActive(_OriginalOwnershipActive);
            }

            _SemanticSkinApplied = false;
            _HasOriginalSkinState = false;
            _TargetGraphic = _Button != null ? _Button.targetGraphic : GetComponent<Graphic>();
            ApplySemanticPresentationMetrics();
        }

        private void ApplySemanticPresentationMetrics()
        {
            _ActivePressedScale = _PressedScale;
            _ActiveDisabledAlpha = _DisabledAlpha;
            _ActiveBlockedSeconds = _BlockedSeconds;
            _ActiveBlockedShakeDistance = _BlockedShakeDistance;
            _ActiveScaleResponse = _ScaleResponse;
            _LoadingPulseMinimumAlpha = 0.65f;

            if (!_SemanticSkinApplied || _SemanticTokens == null || _SemanticTokens.Buttons == null)
                return;

            UiButtonSkinTokens buttons = _SemanticTokens.Buttons;
            _ActivePressedScale = buttons.PressedScale;
            _ActiveDisabledAlpha = buttons.DisabledAlpha;
            _ActiveBlockedSeconds = _ReducedMotionPreview
                ? _SemanticTokens.Motion.ReducedEmphasisSeconds
                : _SemanticTokens.Motion.EmphasisSeconds;
            bool allowShake = !_ReducedMotionPreview || _SemanticTokens.Motion.ReducedMotionAllowsShake;
            float pixelsPerDp = _SemanticTokens.Accessibility != null
                ? _SemanticTokens.Accessibility.ReferencePixelsPerDp
                : UiTokenDefaults.ReferencePixelsPerDp;
            _ActiveBlockedShakeDistance = allowShake
                ? buttons.BlockedShakeDistanceDp * pixelsPerDp
                : 0f;
            _LoadingPulseMinimumAlpha = buttons.LoadingPulseMinimumAlpha;
        }

        private Color SemanticColor(UiColorRole role, Color fallback)
        {
            return _SemanticSkinApplied && _SemanticTokens != null && _SemanticTokens.Colors != null
                ? _SemanticTokens.Colors.Get(role)
                : fallback;
        }

        private static float EvaluateEasing(UiEasing easing, float progress)
        {
            float t = Mathf.Clamp01(progress);
            return easing switch
            {
                UiEasing.Linear => t,
                UiEasing.EaseInCubic => t * t * t,
                UiEasing.EaseOutCubic => 1f - Mathf.Pow(1f - t, 3f),
                UiEasing.EaseInOutCubic => t < 0.5f
                    ? 4f * t * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f,
                UiEasing.EaseOutBack => 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f),
                _ => t
            };
        }

        private void RestorePresentation()
        {
            HideLoadingIndicator();
            RestoreStateBaseAlpha();
            RestoreStateBaseGraphicColor();
            SetLoadingInteraction(true);
            RestoreBlockedVisuals();

            if (_PresentationRoot != null && _HasPressBaseScale)
            {
                _PresentationRoot.localScale = _PressBaseScale;
            }

            _HasPressBaseScale = false;
            _ScaleAnimating = false;
            _ContentLabelsHidden = false;
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.white;
        }
    }
}
