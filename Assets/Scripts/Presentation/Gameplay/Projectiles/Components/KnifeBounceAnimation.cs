using DG.Tweening;
using UnityEngine;
using KnifeToss.Core.Data;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Handles the bounce-off animation when a knife hits another knife.
    /// </summary>
    public sealed class KnifeBounceAnimation : MonoBehaviour
    {
        private const float DefaultFallDistance = 3f;
        private const float DefaultFallDuration = 0.5f;
        private const float DefaultHorizontalDrift = 0.2f;
        private const float DefaultRotationDegrees = 540f;

        [Header("Bounce Animation")]
        [SerializeField] private KnifeBounceOffAnimationConfig bounceAnimationConfig;

        private Sequence _bounceSequence;
        private System.Action _onComplete;

        public void PlayBounceOff(Vector3 startPosition, System.Action onComplete = null)
        {
            KillBounceSequence();
            _onComplete = onComplete;

            float fallDistance = ResolveOrDefault(() => bounceAnimationConfig.FallDistance, DefaultFallDistance);
            float fallDuration = ResolveOrDefault(() => bounceAnimationConfig.FallDuration, DefaultFallDuration);
            float drift = ResolveOrDefault(() => bounceAnimationConfig.HorizontalDrift, DefaultHorizontalDrift);
            float rotationDegrees = ResolveOrDefault(() => bounceAnimationConfig.RotationDegrees, DefaultRotationDegrees);
            float rotationDuration = ResolveOrDefault(() => bounceAnimationConfig.RotationDuration, fallDuration);
            Ease moveEase = ResolveOrDefault(() => bounceAnimationConfig.MovementEase, Ease.InQuad);
            Ease rotationEase = ResolveOrDefault(() => bounceAnimationConfig.RotationEase, Ease.OutQuad);

            Vector3 horizontalOffset = Vector3.right * Random.Range(-drift, drift);
            Vector3 targetPosition = startPosition + horizontalOffset + Vector3.down * fallDistance;

            _bounceSequence = DOTween.Sequence().SetTarget(this);
            _bounceSequence.Join(transform.DOMove(targetPosition, fallDuration).SetEase(moveEase));
            _bounceSequence.Join(
                transform
                    .DORotate(Vector3.forward * rotationDegrees, rotationDuration, RotateMode.FastBeyond360)
                    .SetEase(rotationEase)
                    .SetRelative());
            _bounceSequence.OnComplete(() =>
            {
                _onComplete?.Invoke();
                _bounceSequence = null;
            });
            _bounceSequence.OnKill(() => _bounceSequence = null);
        }

        public void KillBounceSequence()
        {
            if (_bounceSequence != null)
            {
                _bounceSequence.Kill();
                _bounceSequence = null;
            }
        }

        private T ResolveOrDefault<T>(System.Func<T> resolver, T fallback)
        {
            if (bounceAnimationConfig == null)
            {
                return fallback;
            }

            return resolver();
        }
    }
}

