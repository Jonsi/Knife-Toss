using Game.Feedback.Data;
using Game.Feedback.Events;
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
        private static readonly Color Ember = ParseColor("#F26A2E");
        private static readonly Color TextIvory = ParseColor("#FFF4DE");

        [SerializeField] private Button _Button;
        [SerializeField] private RectTransform _PresentationRoot;
        [SerializeField] private CanvasGroup _InteractionGroup;
        [SerializeField] private GameObject _LoadingIndicator;
        [SerializeField] private FeedbackEventChannel _FeedbackChannel;

        [Header("Tokenized Motion")]
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
        private bool _HasStateBaseAlpha;
        private float _StateBaseAlpha = 1f;
        private bool _BaseGroupInteractable = true;
        private bool _BaseGroupBlocksRaycasts = true;
        private bool _HasPressBaseScale;
        private Vector3 _PressBaseScale = Vector3.one;
        private Vector3 _ScaleTarget = Vector3.one;
        private bool _ScaleAnimating;
        private bool _HasBlockedBaseColor;
        private Color _BlockedBaseColor = Color.white;
        private bool _HasBlockedBasePosition;
        private Vector2 _BlockedBasePosition;
        private float _BlockedRemaining;
        private float _LoadingElapsed;

        public ActionButtonPresentationState State => _Model.State;
        public bool IsLoading => _Model.IsLoading;
        public Button Button => _Button;

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
            SyncAvailability();
            ApplyState(force: true);
        }

        private void OnEnable()
        {
            AssignReferences();
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
            _BlockedRemaining = Mathf.Max(0f, _BlockedSeconds);
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
                    SetScaleTarget(_PressBaseScale * _PressedScale);
                    break;
                case ActionButtonPresentationState.Disabled:
                    CaptureStateBaseAlpha();
                    if (_InteractionGroup != null)
                    {
                        _InteractionGroup.alpha = _StateBaseAlpha * _DisabledAlpha;
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
                        _TargetGraphic.color = Ember;
                    }
                    break;
            }
        }

        private void ExitState(ActionButtonPresentationState state)
        {
            switch (state)
            {
                case ActionButtonPresentationState.Pressed:
                    if (_HasPressBaseScale)
                    {
                        SetScaleTarget(_PressBaseScale);
                    }
                    break;
                case ActionButtonPresentationState.Disabled:
                    RestoreStateBaseAlpha();
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
        }

        private void AdvanceScale(float delta)
        {
            if (!_ScaleAnimating || _PresentationRoot == null)
                return;

            float blend = 1f - Mathf.Exp(-_ScaleResponse * delta);
            _PresentationRoot.localScale = Vector3.Lerp(_PresentationRoot.localScale, _ScaleTarget, blend);
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

        private void SetLoadingInteraction(bool restore)
        {
            if (_InteractionGroup == null)
                return;

            if (restore)
            {
                _InteractionGroup.interactable = _BaseGroupInteractable;
                _InteractionGroup.blocksRaycasts = _BaseGroupBlocksRaycasts;
                return;
            }

            _BaseGroupInteractable = _InteractionGroup.interactable;
            _BaseGroupBlocksRaycasts = _InteractionGroup.blocksRaycasts;
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

            float elapsed = Mathf.Max(0f, _BlockedSeconds - _BlockedRemaining);
            float decay = _BlockedSeconds <= 0f ? 0f : Mathf.Clamp01(_BlockedRemaining / _BlockedSeconds);
            float offset = Mathf.Sin(elapsed * 70f) * _BlockedShakeDistance * decay;
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
                SetContentLabelsVisible(false);
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

            _LoadingElapsed += delta;
            _LoadingCanvasGroup.alpha = 0.65f + Mathf.PingPong(_LoadingElapsed * 1.8f, 0.35f);
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
                indicator.transform.SetParent(transform, false);
                var rect = indicator.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var text = indicator.GetComponent<TextMeshProUGUI>();
                text.text = "•••";
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
                text.color = source != null ? source.color : TextIvory;
                text.fontSize = source != null ? Mathf.Max(18f, source.fontSize * 0.85f) : 24f;
                if (source != null)
                {
                    text.font = source.font;
                }

                _LoadingIndicator = indicator;
                _CreatedLoadingIndicator = true;
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
        }

        private void AssignReferences()
        {
            _Button ??= GetComponent<Button>();
            _PresentationRoot ??= transform as RectTransform;
            _ShakeRoot ??= _PresentationRoot;
            _TargetGraphic = _Button != null ? _Button.targetGraphic : GetComponent<Graphic>();

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

        private void RestorePresentation()
        {
            HideLoadingIndicator();
            RestoreStateBaseAlpha();
            SetLoadingInteraction(true);
            RestoreBlockedVisuals();

            if (_PresentationRoot != null && _HasPressBaseScale)
            {
                _PresentationRoot.localScale = _PressBaseScale;
            }

            _HasPressBaseScale = false;
            _ScaleAnimating = false;
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.white;
        }
    }
}
