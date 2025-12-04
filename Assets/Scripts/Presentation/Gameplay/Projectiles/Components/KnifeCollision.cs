using System;
using UnityEngine;
using KnifeToss.Core.Constants;
using KnifeToss.Core.Utilities;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Handles collision detection for knife projectiles.
    /// Publishes collision events for other components to handle.
    /// </summary>
    public sealed class KnifeCollision : MonoBehaviour
    {
        [SerializeField] private Collider2D[] _colliders; // Cache in inspector!

        public event Action<CollisionData> OnCollided;

        private bool _isEnabled = true;

        private void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                _colliders = GetComponents<Collider2D>();
            }
        }

        public void Enable()
        {
            _isEnabled = true;
            SetCollidersEnabled(true);
        }

        public void Disable()
        {
            _isEnabled = false;
            SetCollidersEnabled(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isEnabled) return;
            HandleCollision(other.transform, other.gameObject, CollisionUtility.ResolveImpactPoint(other, transform.position));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isEnabled) return;
            HandleCollision(collision.transform, collision.gameObject, CollisionUtility.ResolveImpactPoint(collision, transform.position));
        }

        private void HandleCollision(Transform collisionTransform, GameObject collisionObject, Vector3 impactPoint)
        {
            if (collisionObject == null)
            {
                return;
            }

            CollisionData data = new CollisionData(impactPoint, collisionObject, collisionTransform, CollisionType.Trigger);
            OnCollided?.Invoke(data);
        }

        private void SetCollidersEnabled(bool enabled)
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                _colliders = GetComponents<Collider2D>();
            }

            for (int i = 0; i < _colliders.Length; i++)
            {
                Collider2D col = _colliders[i];
                if (col != null)
                {
                    col.enabled = enabled;
                }
            }
        }
    }
}

