using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Cached references for knives to avoid repeated GetComponent calls.
    /// </summary>
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
    }
}
