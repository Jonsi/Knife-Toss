using DG.Tweening;
using KnifeToss.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{



    public sealed class FailFeedbackPresenter : MonoBehaviour
    {
        [Header("Event Source")]
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("Retry Prompt")]
        [SerializeField] private RectTransform retryButtonTransform;
        [SerializeField] private Button retryButton;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;
        [SerializeField, Range(1f, 1.5f)] private float retryBounceScale = 1.08f;
        [SerializeField, Range(0.05f, 0.6f)] private float retryBounceDuration = 0.28f;
        [SerializeField, Range(0f, 1f)] private float retryBounceInterval = 0.1f;
        [SerializeField, Tooltip("Number of grow+shrink loops. Set -1 for endless until clicked.")]
        private int retryBounceLoops = -1;

        private bool isSubscribed;
        private Vector3 retryBaseScale = Vector3.one;
        private Vector2 retryBaseAnchoredPosition;
        private Sequence retrySequence;

        private void Awake()
        {
            CacheDefaults();
            Subscribe();
            ResetFeedback();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            KillTweens();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            KillTweens();
        }

        public void ResetFeedback()
        {
            KillTweens();
            ResetRetryAnimation();
        }

        private void Subscribe()
        {
            if (isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeFailed += HandleKnifeFailed;
            retryButton?.onClick.AddListener(HandleRetryClicked);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeFailed -= HandleKnifeFailed;
            retryButton?.onClick.RemoveListener(HandleRetryClicked);
            isSubscribed = false;
        }

        private void HandleKnifeFailed()
        {
            PlayFeedback();
        }

        private void HandleRetryClicked()
        {

            StopRetryAnimation();
        }

        private void PlayFeedback()
        {
            AnimateRetryButton();
        }

        private void AnimateRetryButton()
        {
            if (retryButtonTransform == null)
            {
                return;
            }

            KillRetryTweens();

            retryButtonTransform.anchoredPosition = retryBaseAnchoredPosition;
            retryButtonTransform.localScale = retryBaseScale;

            retrySequence = DOTween.Sequence().SetTarget(retryButtonTransform);
            float halfDuration = Mathf.Max(0.01f, retryBounceDuration * 0.5f);
            Tween grow = retryButtonTransform
                .DOScale(retryBaseScale * retryBounceScale, halfDuration)
                .SetEase(Ease.OutBack);
            Tween shrink = retryButtonTransform
                .DOScale(retryBaseScale, halfDuration)
                .SetEase(Ease.OutBack);

            retrySequence.Append(grow);
            retrySequence.Append(shrink);
            retrySequence.AppendInterval(Mathf.Max(0f, retryBounceInterval));

            int loops = retryBounceLoops < 0 ? -1 : Mathf.Max(1, retryBounceLoops);
            retrySequence.SetLoops(loops, LoopType.Restart);
        }

        private void ResetRetryAnimation()
        {
            if (retryButtonTransform == null)
            {
                return;
            }

            KillRetryTweens();
            retryButtonTransform.localScale = retryBaseScale;
            retryButtonTransform.anchoredPosition = retryBaseAnchoredPosition;
        }

        private void StopRetryAnimation()
        {
            KillRetryTweens();
            if (retryButtonTransform != null)
            {
                retryButtonTransform.localScale = retryBaseScale;
            }
        }

        private void KillTweens()
        {
            KillRetryTweens();
        }

        private void KillRetryTweens()
        {
            retrySequence?.Kill();
            retrySequence = null;

            DOTween.Kill(retryButtonTransform);
        }

        private void CacheDefaults()
        {
            if (retryButtonTransform != null)
            {
                retryBaseScale = retryButtonTransform.localScale;
                retryBaseAnchoredPosition = retryButtonTransform.anchoredPosition;
            }
        }
    }
}
