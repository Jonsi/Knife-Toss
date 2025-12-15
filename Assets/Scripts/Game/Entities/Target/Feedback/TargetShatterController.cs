using Cysharp.Threading.Tasks;
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
        [SerializeField] private Transform _DetachedParent;
        [SerializeField] private GameObject _VisualRoot;

        private bool _VisualRootPreviousState;
        private bool _HasVisualRootState;

        /// <summary>
        /// Plays the configured shatter effect.
        /// </summary>
        /// <returns>Awaitable task completed after the shards finish animating.</returns>
        public UniTask PlayShatterAsync()
        {
            if (_SpriteRenderer == null || _AnimationConfig == null || _ShardMaterial == null)
                return UniTask.CompletedTask;

            bool previousRendererState = _SpriteRenderer.enabled;
            _SpriteRenderer.enabled = false;
            SetVisualRootActive(false);

            TargetShatterSequence sequence = TargetShatterSequence.Create(_SpriteRenderer, _AnimationConfig, _ShardMaterial, _DetachedParent);
            if (sequence == null)
            {
                _SpriteRenderer.enabled = previousRendererState;
                SetVisualRootActive(true);
                return UniTask.CompletedTask;
            }

            var completionSource = new UniTaskCompletionSource();

            void HandleCompleted(TargetShatterSequence completedSequence)
            {
                completedSequence.Completed -= HandleCompleted;
                _SpriteRenderer.enabled = previousRendererState;
                SetVisualRootActive(true);
                completionSource.TrySetResult();
            }

            sequence.Completed += HandleCompleted;
            return completionSource.Task;
        }

        private void SetVisualRootActive(bool isActive)
        {
            GameObject rootObject = ResolveVisualRoot();
            if (rootObject == null)
                return;

            if (!isActive)
            {
                _VisualRootPreviousState = rootObject.activeSelf;
                _HasVisualRootState = true;
                rootObject.SetActive(false);
                return;
            }

            rootObject.SetActive(_HasVisualRootState ? _VisualRootPreviousState : true);
            _HasVisualRootState = false;
        }

        private GameObject ResolveVisualRoot()
        {
            if (_VisualRoot != null)
                return _VisualRoot;

            if (_SpriteRenderer != null)
            {
                Transform rendererTransform = _SpriteRenderer.transform;
                return rendererTransform != null ? rendererTransform.gameObject : null;
            }

            return null;
        }
    }
}
