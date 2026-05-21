using System;
using System.Collections;
using UnityEngine;

namespace Game.UI.Common
{
    [DisallowMultipleComponent]
    public sealed class PopupFadeInAnimation : MonoBehaviour
    {
        [Serializable]
        private struct TransitionSettings
        {
            [Min(0f)] public float Duration;
            public float StartAlpha;
            public float EndAlpha;
            public float StartScale;
            public float EndScale;
        }

        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private RectTransform _AnimatedContent;
        [SerializeField] private TransitionSettings _Show = new TransitionSettings
        {
            Duration = 0.2f,
            StartAlpha = 0f,
            EndAlpha = 1f,
            StartScale = 0f,
            EndScale = 1f
        };
        [SerializeField] private TransitionSettings _Hide = new TransitionSettings
        {
            Duration = 0.15f,
            StartAlpha = 1f,
            EndAlpha = 0f,
            StartScale = 1f,
            EndScale = 0f
        };

        private Coroutine _AnimationRoutine;
        private Action _CompletionCallback;

        public bool IsAnimatingOut { get; private set; }

        private void Reset()
        {
            AssignReferences();
        }

        private void Awake()
        {
            AssignReferences();
        }

        private void OnDisable()
        {
            StopAnimation();
            SetInteractionEnabled(true);
            IsAnimatingOut = false;
        }

        public void PlayShow()
        {
            IsAnimatingOut = false;
            StartTransition(_Show, true, null);
        }

        public void PlayHide(Action onComplete)
        {
            IsAnimatingOut = true;
            StartTransition(_Hide, false, onComplete);
        }

        private void StartTransition(TransitionSettings settings, bool enableInteractionOnComplete, Action onComplete)
        {
            StopAnimation();
            ApplyState(settings.StartAlpha, settings.StartScale);
            SetInteractionEnabled(false);
            _CompletionCallback = onComplete;
            _AnimationRoutine = StartCoroutine(PlayTransition(settings, enableInteractionOnComplete));
        }

        private IEnumerator PlayTransition(TransitionSettings settings, bool enableInteractionOnComplete)
        {
            if (settings.Duration <= 0f)
            {
                CompleteTransition(settings, enableInteractionOnComplete);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < settings.Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / settings.Duration);
                ApplyState(
                    Mathf.Lerp(settings.StartAlpha, settings.EndAlpha, progress),
                    Mathf.Lerp(settings.StartScale, settings.EndScale, progress));
                yield return null;
            }

            CompleteTransition(settings, enableInteractionOnComplete);
        }

        private void CompleteTransition(TransitionSettings settings, bool enableInteractionOnComplete)
        {
            ApplyState(settings.EndAlpha, settings.EndScale);
            SetInteractionEnabled(enableInteractionOnComplete);

            _AnimationRoutine = null;
            var completionCallback = _CompletionCallback;
            _CompletionCallback = null;
            completionCallback?.Invoke();
        }

        private void ApplyState(float alpha, float scale)
        {
            if (_CanvasGroup != null)
                _CanvasGroup.alpha = alpha;

            if (_AnimatedContent != null)
                _AnimatedContent.localScale = Vector3.one * scale;
        }

        private void SetInteractionEnabled(bool isEnabled)
        {
            if (_CanvasGroup == null)
                return;

            _CanvasGroup.interactable = isEnabled;
            _CanvasGroup.blocksRaycasts = isEnabled;
        }

        private void StopAnimation()
        {
            if (_AnimationRoutine != null)
            {
                StopCoroutine(_AnimationRoutine);
                _AnimationRoutine = null;
            }

            _CompletionCallback = null;
        }

        private void AssignReferences()
        {
            if (_CanvasGroup == null)
                _CanvasGroup = GetComponent<CanvasGroup>();

            if (_AnimatedContent == null)
            {
                var content = transform.Find("Content");
                if (content is RectTransform rectTransform)
                    _AnimatedContent = rectTransform;
            }
        }
    }
}
