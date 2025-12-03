using DG.Tweening;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{



    [DisallowMultipleComponent]
    public sealed class KnifeView : MonoBehaviour
    {
        [field: SerializeField] public SpriteRenderer Renderer { get; private set; }
        [field: SerializeField] public Collider2D[] Colliders { get; private set; }

        private void Awake()
        {
            CacheIfNeeded();
        }

        public void CacheIfNeeded()
        {
            Renderer ??= GetComponent<SpriteRenderer>();
            if (Colliders == null || Colliders.Length == 0)
            {
                Colliders = GetComponents<Collider2D>();
            }
        }

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

        public void SetColor(Color color)
        {
            if (Renderer != null)
            {
                Renderer.color = color;
            }
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }

        public void ResetTransform(Transform parent, Vector3 targetScale)
        {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = targetScale;
        }

        public void ApplyVisuals(Vector3 position, Vector3 scale, Color color, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
            SetColor(color);
        }

        public void AnimateTo(Vector3 position, Vector3 scale, Color color, float duration)
        {
            KillTweens();

            transform.DOMove(position, duration).SetEase(Ease.OutQuad);
            transform.DOScale(scale, duration).SetEase(Ease.OutQuad);

            if (Renderer != null)
            {
                Renderer.DOColor(color, duration).SetEase(Ease.OutQuad);
            }
        }

        public void KillTweens()
        {
            TweenKiller.Kill(gameObject);
        }

        public void SetColliders(bool enabled)
        {
            SetCollidersEnabled(enabled);
        }
    }
}
