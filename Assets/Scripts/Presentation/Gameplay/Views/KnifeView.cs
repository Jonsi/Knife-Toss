using DG.Tweening;
using UnityEngine;

namespace Presentation.Gameplay.Views
{
    [DisallowMultipleComponent]
    public sealed class KnifeView : MonoBehaviour
    {
        [field: SerializeField] public SpriteRenderer Renderer { get; private set; }
        [field: SerializeField] public Collider2D[] Colliders { get; private set; }

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            CacheIfNeeded();
        }

        /// <summary>
        /// Performs the CacheIfNeeded operation.
        /// </summary>
        public void CacheIfNeeded()
        {
            Renderer ??= GetComponent<SpriteRenderer>();
            if (Colliders == null || Colliders.Length == 0)
            {
                Colliders = GetComponents<Collider2D>();
            }
        }

        /// <summary>
        /// Performs the SetCollidersEnabled operation.
        /// </summary>
        public void SetCollidersEnabled(bool enabled)
        {
            if (Colliders == null)
            {
                return;
            }

            for (int i = 0; i < Colliders.Length; i++)
            {
                Collider2D collider = Colliders[i];
                if (collider != null)
                {
                    collider.enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Performs the SetColor operation.
        /// </summary>
        public void SetColor(Color color)
        {
            if (Renderer != null)
            {
                Renderer.color = color;
            }
        }

        /// <summary>
        /// Performs the SetParent operation.
        /// </summary>
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }

        /// <summary>
        /// Performs the ResetTransform operation.
        /// </summary>
        public void ResetTransform(Transform parent, Vector3 targetScale)
        {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = targetScale;
        }

        /// <summary>
        /// Performs the ApplyVisuals operation.
        /// </summary>
        public void ApplyVisuals(Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
            SetColor(color);
        }

        /// <summary>
        /// Performs the AnimateTo operation.
        /// </summary>
        public void AnimateTo(Vector3 position, Vector3 scale, Color color, float duration)
        {
            KillTweens();

            if (transform != null)
            {
                transform.DOMove(position, duration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(transform)
                    .SetAutoKill(true);
                
                transform.DOScale(scale, duration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(transform)
                    .SetAutoKill(true);
            }

            if (Renderer != null)
            {
                Renderer.DOColor(color, duration)
                    .SetEase(Ease.OutQuad)
                    .SetTarget(Renderer)
                    .SetAutoKill(true);
            }
        }

        /// <summary>
        /// Performs the KillTweens operation.
        /// </summary>
        public void KillTweens()
        {
            TweenKiller.Kill(gameObject);
        }

        /// <summary>
        /// Performs the SetColliders operation.
        /// </summary>
        public void SetColliders(bool enabled)
        {
            SetCollidersEnabled(enabled);
        }
    }
}
