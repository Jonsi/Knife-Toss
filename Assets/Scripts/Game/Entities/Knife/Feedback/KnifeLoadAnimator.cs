using DG.Tweening;
using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Entities.Knife.Feedback
{
    /// <summary>
    /// Plays a spawn animation for knives using DOTween.
    /// </summary>
    public class KnifeLoadAnimator : MonoBehaviour
    {
        [SerializeField] private KnifeLoadAnimationConfig _Config;

        /// <summary>
        /// Plays the configured load animation on the provided targets.
        /// </summary>
        /// <param name="visualRoot">Root transform to animate.</param>
        /// <param name="spriteRenderer">Sprite renderer to fade.</param>
        public void PlayLoadAnimation(Transform visualRoot, SpriteRenderer spriteRenderer)
        {
            if (_Config == null || visualRoot == null || spriteRenderer == null)
            {
                return;
            }

            Vector3 originalLocalPosition = visualRoot.localPosition;
            Vector3 startLocalPosition = originalLocalPosition + new Vector3(0f, -_Config.VerticalOffset, 0f);
            visualRoot.localPosition = startLocalPosition;

            Color fadedColor = spriteRenderer.color;
            fadedColor.a = _Config.FadeFromAlpha;
            spriteRenderer.color = fadedColor;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(visualRoot.DOLocalMove(originalLocalPosition, _Config.Duration).SetEase(Ease.OutQuad));
            sequence.Join(spriteRenderer.DOFade(_Config.TargetAlpha, _Config.Duration));
        }
    }
}

