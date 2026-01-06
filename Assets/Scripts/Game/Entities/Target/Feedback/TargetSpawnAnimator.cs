using System;
using DG.Tweening;
using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Plays a DOTween-driven spawn animation for targets (scale pop).
    /// </summary>
    public class TargetSpawnAnimator : MonoBehaviour
    {
        [SerializeField] private TargetSpawnAnimationConfig _Config;
        [SerializeField] private Transform _AnimatedRoot;

        public event Action OnAnimationComplete;

        private Sequence _ActiveSequence;
        private Vector3 _FinalScale = Vector3.one;
        private bool _HasCachedScale;

        private void Awake()
        {
            EnsureAnimatedRoot();
        }

        private void OnDisable()
        {
            KillSequence();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        /// <summary>
        /// Plays the configured spawn animation if possible.
        /// </summary>
        public void PlaySpawnAnimation()
        {
            if (_Config == null)
            {
                OnAnimationComplete?.Invoke();
                return;
            }

            EnsureAnimatedRoot();
            KillSequence();

            _FinalScale = _AnimatedRoot.localScale;
            _HasCachedScale = true;
            _AnimatedRoot.localScale = Vector3.zero;

            Sequence sequence = DOTween.Sequence();
            AppendScale(sequence);
            sequence.OnComplete(() => OnAnimationComplete?.Invoke());
            _ActiveSequence = sequence;
        }

        private void AppendScale(Sequence sequence)
        {
            float scaleUpDuration = Mathf.Max(0.01f, _Config.ScaleUpDuration);
            float settleDuration = Mathf.Max(0f, _Config.ScaleSettleDuration);
            float overshoot = Mathf.Max(1f, _Config.ScaleOvershootMultiplier);

            if (overshoot > 1f && settleDuration > 0f)
            {
                sequence.Append(_AnimatedRoot
                    .DOScale(_FinalScale * overshoot, scaleUpDuration)
                    .SetEase(_Config.ScaleUpEase));
                sequence.Append(_AnimatedRoot
                    .DOScale(_FinalScale, settleDuration)
                    .SetEase(_Config.ScaleSettleEase));
            }
            else
            {
                sequence.Append(_AnimatedRoot
                    .DOScale(_FinalScale, scaleUpDuration)
                    .SetEase(_Config.ScaleUpEase));
            }
        }

        private void KillSequence()
        {
            if (_ActiveSequence != null)
            {
                _ActiveSequence.Kill();
                _ActiveSequence = null;
            }

            if (_HasCachedScale && _AnimatedRoot != null)
            {
                _AnimatedRoot.localScale = _FinalScale;
            }
        }

        private void EnsureAnimatedRoot()
        {
            if (_AnimatedRoot == null)
            {
                _AnimatedRoot = transform;
            }
        }

    }
}
