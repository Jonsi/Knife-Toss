using Game.Feedback.Data;
using Game.Feedback.Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Feedback.Presentation
{
    /// <summary>
    /// Reusable, non-modal toast presenter for the shared typed feedback channel.
    /// Queueing and duplicate semantics are delegated to <see cref="FeedbackPresentationQueue"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackToastPresenter : MonoBehaviour
    {
        private enum PresentationPhase
        {
            Hidden,
            Entering,
            Visible,
            Exiting
        }

        private static readonly Color Leaf = ParseColor("#45C968");
        private static readonly Color Lagoon = ParseColor("#19BFC4");
        private static readonly Color SunGold = ParseColor("#FFC43D");
        private static readonly Color Ember = ParseColor("#F26A2E");
        private static readonly Color Failure = ParseColor("#E84B3C");
        private static readonly Color Parchment = ParseColor("#E8C79B");

        [SerializeField] private FeedbackEventChannel _Channel;
        [SerializeField] private FeedbackToastCatalog _Catalog;
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private RectTransform _ToastRoot;
        [SerializeField] private TextMeshProUGUI _MessageText;
        [SerializeField] private Image _AccentImage;
        [SerializeField, Min(0f)] private float _EnterSeconds = 0.16f;
        [SerializeField, Min(0f)] private float _ExitSeconds = 0.18f;
        [SerializeField, Min(0f)] private float _SlideDistance = 48f;

        private readonly FeedbackPresentationQueue _Queue = new();
        private PresentationPhase _Phase;
        private Vector2 _VisiblePosition;
        private float _PhaseElapsed;
        private float _VisibleSeconds;

        public bool HasActiveFeedback => _Queue.HasActive;
        public int PendingFeedbackCount => _Queue.PendingCount;
        public FeedbackEventData ActiveFeedback => _Queue.Active;

        private void Awake()
        {
            AssignReferences();
            if (_ToastRoot != null)
            {
                _VisiblePosition = _ToastRoot.anchoredPosition;
            }

            HideImmediately();
        }

        private void OnEnable()
        {
            _Channel?.Subscribe(OnFeedbackRequested);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            _Channel?.Unsubscribe(OnFeedbackRequested);
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _Queue.Clear();
            HideImmediately();
        }

        private void Update()
        {
            AdvancePresentation(Time.unscaledDeltaTime, Time.unscaledTimeAsDouble);
        }

        /// <summary>
        /// Advances the presentation with caller-supplied unscaled time. Public for deterministic
        /// presenter validation; normal runtime use is driven by <see cref="Update"/>.
        /// </summary>
        public void AdvancePresentation(float unscaledDeltaTime, double unscaledNow)
        {
            if (_Phase == PresentationPhase.Hidden)
                return;

            _PhaseElapsed += Mathf.Max(0f, unscaledDeltaTime);
            switch (_Phase)
            {
                case PresentationPhase.Entering:
                    AdvanceEntering();
                    break;
                case PresentationPhase.Visible:
                    if (_PhaseElapsed >= _VisibleSeconds)
                    {
                        _Phase = PresentationPhase.Exiting;
                        _PhaseElapsed = 0f;
                    }
                    break;
                case PresentationPhase.Exiting:
                    AdvanceExiting(unscaledNow);
                    break;
            }
        }

        private void OnFeedbackRequested(FeedbackEventData request)
        {
            FeedbackEnqueueResult result = _Queue.Enqueue(request, Time.unscaledTimeAsDouble);
            if (result == FeedbackEnqueueResult.Presented ||
                result == FeedbackEnqueueResult.Interrupted)
            {
                Show(_Queue.Active);
            }
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _Queue.Clear();
            HideImmediately();
        }

        private void Show(FeedbackEventData request)
        {
            FeedbackToastPresentation presentation = Resolve(request);
            if (_MessageText != null)
            {
                _MessageText.text = presentation.Message;
            }

            if (_AccentImage != null)
            {
                _AccentImage.color = presentation.AccentColor;
            }

            _VisibleSeconds = presentation.VisibleSeconds;
            _Phase = PresentationPhase.Entering;
            _PhaseElapsed = 0f;
            ApplyVisual(0f, 1f);
        }

        private void AdvanceEntering()
        {
            float duration = Mathf.Max(0.0001f, _EnterSeconds);
            float progress = Mathf.Clamp01(_PhaseElapsed / duration);
            ApplyVisual(Mathf.SmoothStep(0f, 1f, progress), 1f - progress);
            if (progress < 1f)
                return;

            _Phase = PresentationPhase.Visible;
            _PhaseElapsed = 0f;
            ApplyVisual(1f, 0f);
        }

        private void AdvanceExiting(double unscaledNow)
        {
            float duration = Mathf.Max(0.0001f, _ExitSeconds);
            float progress = Mathf.Clamp01(_PhaseElapsed / duration);
            ApplyVisual(1f - Mathf.SmoothStep(0f, 1f, progress), progress);
            if (progress < 1f)
                return;

            if (_Queue.CompleteActive(unscaledNow, out FeedbackEventData next))
            {
                Show(next);
            }
            else
            {
                HideImmediately();
            }
        }

        private FeedbackToastPresentation Resolve(FeedbackEventData request)
        {
            if (_Catalog != null && _Catalog.TryResolve(request, out FeedbackToastPresentation cue))
                return cue;

            return request.Kind switch
            {
                FeedbackKind.Success => new FeedbackToastPresentation("Success", Leaf, 1.35f),
                FeedbackKind.Blocked => new FeedbackToastPresentation("Action unavailable", Ember, 1.65f),
                FeedbackKind.InsufficientFunds => new FeedbackToastPresentation("Not enough currency", Ember, 1.65f),
                FeedbackKind.Purchase => new FeedbackToastPresentation("Purchase complete", SunGold, 1.55f),
                FeedbackKind.Reward => new FeedbackToastPresentation("Reward received", SunGold, 1.55f),
                FeedbackKind.Consume => new FeedbackToastPresentation("Used", Lagoon, 1.35f),
                FeedbackKind.Navigation => new FeedbackToastPresentation("Opened", Parchment, 1.1f),
                FeedbackKind.Failure => new FeedbackToastPresentation("Try again", Failure, 1.8f),
                FeedbackKind.Completion => new FeedbackToastPresentation("Complete", SunGold, 1.8f),
                _ => new FeedbackToastPresentation(string.Empty, Parchment, 1.35f)
            };
        }

        private void HideImmediately()
        {
            _Phase = PresentationPhase.Hidden;
            _PhaseElapsed = 0f;
            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = 0f;
                _CanvasGroup.interactable = false;
                _CanvasGroup.blocksRaycasts = false;
            }

            if (_ToastRoot != null)
            {
                _ToastRoot.anchoredPosition = _VisiblePosition + Vector2.up * _SlideDistance;
            }
        }

        private void ApplyVisual(float alpha, float hiddenOffset)
        {
            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = Mathf.Clamp01(alpha);
                _CanvasGroup.interactable = false;
                _CanvasGroup.blocksRaycasts = false;
            }

            if (_ToastRoot != null)
            {
                _ToastRoot.anchoredPosition =
                    _VisiblePosition + Vector2.up * (_SlideDistance * hiddenOffset);
            }
        }

        private void AssignReferences()
        {
            if (_ToastRoot == null)
                _ToastRoot = transform as RectTransform;
            if (_CanvasGroup == null)
                _CanvasGroup = GetComponent<CanvasGroup>();
            if (_MessageText == null)
                _MessageText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_AccentImage == null)
            {
                Transform accent = transform.Find("Accent");
                if (accent != null)
                    _AccentImage = accent.GetComponent<Image>();
            }
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.white;
        }
    }
}
