using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class RetryButtonAnimator : MonoBehaviour
    {
        [SerializeField] private RetryButtonAnimationConfig config;

        private Tween pulseTween;
        private Vector3 defaultScale;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            StopAnimation();
            ResetScale();
        }

        /// <summary>
        /// Performs the Play operation.
        /// </summary>
        public void Play()
        {
            StopAnimation();

            float min = config != null ? config.minScale : 0.9f;
            float max = config != null ? config.maxScale : 1.1f;
            float duration = config != null ? Mathf.Max(0.05f, config.pulseDuration) : 0.5f;
            Ease ease = config != null ? config.pulseEase : Ease.InOutSine;

            pulseTween = transform
                .DOScale(Vector3.one * max, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .SetLoops(-1, LoopType.Yoyo)
                .From(Vector3.one * min);
        }

        /// <summary>
        /// Performs the StopAnimation operation.
        /// </summary>
        public void StopAnimation()
        {
            if (pulseTween != null)
            {
                pulseTween.Kill();
                pulseTween = null;
            }
            ResetScale();
        }

        /// <summary>
        /// Performs the ResetScale operation.
        /// </summary>
        private void ResetScale()
        {
            transform.localScale = defaultScale == Vector3.zero ? Vector3.one : defaultScale;
        }
    }
}
