using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Triggers runtime shatter sequences for the owning target.
    /// </summary>
    public class TargetShatterController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private TargetShatterAnimationConfig _AnimationConfig;
        [SerializeField] private Material _ShardMaterial;

        /// <summary>
        /// Plays the configured shatter effect.
        /// </summary>
        public void PlayShatter()
        {
            if (_SpriteRenderer == null || _AnimationConfig == null || _ShardMaterial == null)
                return;

            TargetShatterSequence.Create(_SpriteRenderer, _AnimationConfig, _ShardMaterial);
        }
    }
}
