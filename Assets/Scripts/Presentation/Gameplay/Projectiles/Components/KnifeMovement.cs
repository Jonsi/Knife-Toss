using UnityEngine;
using KnifeToss.Core.Abstractions.Pooling;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Handles linear motion of a knife projectile.
    /// Requires Rigidbody2D (cached in inspector) and is agnostic of collision/animation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class KnifeMovement : MonoBehaviour, IPoolable
    {
        [SerializeField] private Rigidbody2D _rigidbody; // Cache in inspector!

        private Vector2 _direction = Vector2.up;
        private float _speed;
        private bool _isActive;

        public bool IsActive => _isActive;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        public void Launch(Vector2 direction, float speed)
        {
            _direction = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector2.up;
            _speed = speed;
            _isActive = true;

            if (_rigidbody != null)
            {
                _rigidbody.simulated = true;
                _rigidbody.gravityScale = 0f;
                _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                transform.up = _direction;
            }
        }

        public void Stop()
        {
            _isActive = false;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
            }
        }

        public void Halt()
        {
            Stop();
            if (_rigidbody != null)
            {
                _rigidbody.simulated = false;
            }
        }

        private void FixedUpdate()
        {
            if (!_isActive || _rigidbody == null)
            {
                return;
            }

            _rigidbody.linearVelocity = _direction * _speed;
        }

        public void OnSpawn()
        {
            // Reset state when spawned from pool
            _isActive = false;
            _speed = 0f;
            _direction = Vector2.up;
        }

        public void OnDespawn()
        {
            Stop();
            if (_rigidbody != null)
            {
                _rigidbody.simulated = false;
            }
        }
    }
}

