using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Feedback.Animation
{
    /// <summary>
    /// Triggers runtime shatter sequences for any sprite-driven entity.
    /// </summary>
    public class SpriteShatterController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private SpriteShatterAnimationConfig _AnimationConfig;
        [SerializeField] private Material _ShardMaterial;

        /// <summary>
        /// Plays the configured shatter effect.
        /// </summary>
        public void PlayShatter()
        {
            if (_SpriteRenderer == null || _AnimationConfig == null || _ShardMaterial == null)
                return;

            SpriteShatterSequence.Create(_SpriteRenderer, _AnimationConfig, _ShardMaterial);
        }
    }
}
