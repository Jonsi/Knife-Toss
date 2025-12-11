using DG.Tweening;
using Game.Entities.Target.Interfaces;
using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Plays DOTween-based hit feedback for targets.
    /// </summary>
    public class TargetHitFeedback : MonoBehaviour, ITargetHitFeedback
    {
        [SerializeField] private TargetHitAnimationConfig _Config;
        [SerializeField] private Transform _TargetTransform;
        [SerializeField] private SpriteRenderer _SpriteRenderer;

        private Sequence _ActiveSequence;
        private Color _BaseColor = Color.white;

        private void Awake()
        {
            if (_SpriteRenderer != null)
            {
                _BaseColor = _SpriteRenderer.color;
            }
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        /// <inheritdoc />
        public void PlayHitFeedback()
        {
            if (_Config == null || _TargetTransform == null)
            {
                return;
            }

            KillSequence();
            Sequence sequence = DOTween.Sequence();
            sequence.Join(_TargetTransform.DOShakePosition(
                _Config.ShakeDuration,
                new Vector3(_Config.ShakeStrength, _Config.ShakeStrength, 0f),
                _Config.ShakeVibrato,
                _Config.ShakeRandomness,
                _Config.ShakeFadeOut,
                false));

            if (_SpriteRenderer != null)
            {
                sequence.Join(_SpriteRenderer.DOColor(_Config.TintColor, _Config.TintFadeIn));
                sequence.AppendInterval(_Config.TintHold);
                sequence.Append(_SpriteRenderer.DOColor(_BaseColor, _Config.TintFadeOut));
            }

            _ActiveSequence = sequence;
        }

        private void KillSequence()
        {
            if (_ActiveSequence != null)
            {
                _ActiveSequence.Kill();
                _ActiveSequence = null;
            }

            if (_SpriteRenderer != null)
            {
                _SpriteRenderer.color = _BaseColor;
            }
        }
    }
}

