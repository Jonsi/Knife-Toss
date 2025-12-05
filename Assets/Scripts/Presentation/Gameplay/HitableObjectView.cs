using Core.Data;
using Core.Events;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Handles knife collisions for hitable objects and plays configured feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitableObjectView : MonoBehaviour
    {
        [SerializeField] private bool destroyOnHit = true;

        private bool hasBeenHit;
        private Collider2D[] cachedColliders;
        private HitableObjectConfig config;
        private HitableObjectHitEventChannel hitEventChannel;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            cachedColliders = GetComponents<Collider2D>();
        }

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        public void Configure(HitableObjectConfig hitableConfig, HitableObjectHitEventChannel channel)
        {
            config = hitableConfig;
            hitEventChannel = channel;
        }

        /// <summary>
        /// Performs the OnTriggerEnter2D operation.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            TryHandleHit(other.gameObject, ResolveImpactPoint(other));
        }

        /// <summary>
        /// Performs the OnCollisionEnter2D operation.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null)
            {
                return;
            }

            TryHandleHit(collision.gameObject, ResolveImpactPoint(collision));
        }

        /// <summary>
        /// Performs the TryHandleHit operation.
        /// </summary>
        private void TryHandleHit(GameObject otherObject, Vector3 impactPoint)
        {
            if (otherObject == null || !otherObject.CompareTag("Knife"))
            {
                return;
            }

            ProcessHit(impactPoint);
        }

        public void ProcessHit(Vector3 impactPoint)
        {
            if (hasBeenHit)
            {
                return;
            }

            hasBeenHit = true;
            DisableColliders();
            SpawnParticles(impactPoint);
            hitEventChannel?.RaiseHit(impactPoint);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Performs the SpawnParticles operation.
        /// </summary>
        private void SpawnParticles(Vector3 position)
        {
            if (config == null || config.hitParticles == null)
            {
                return;
            }

            ParticleSystem instance = Instantiate(
                config.hitParticles,
                position,
                Quaternion.identity,
                transform.parent);

            ParticleSystem.MainModule main = instance.main;
            if (!main.loop)
            {
                float life = main.startLifetime.constantMax;
                Destroy(instance.gameObject, main.duration + life);
            }
        }

        /// <summary>
        /// Performs the DisableColliders operation.
        /// </summary>
        private void DisableColliders()
        {
            if (cachedColliders == null || cachedColliders.Length == 0)
            {
                cachedColliders = GetComponents<Collider2D>();
            }

            for (int i = 0; i < cachedColliders.Length; i++)
            {
                Collider2D col = cachedColliders[i];
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        /// <summary>
        /// Performs the ResolveImpactPoint operation.
        /// </summary>
        private Vector3 ResolveImpactPoint(Collision2D collision)
        {
            if (collision == null)
            {
                return transform.position;
            }

            if (collision.contactCount > 0)
            {
                return collision.GetContact(0).point;
            }

            return ResolveImpactPoint(collision.collider);
        }

        /// <summary>
        /// Performs the ResolveImpactPoint operation.
        /// </summary>
        private Vector3 ResolveImpactPoint(Collider2D collider)
        {
            if (collider == null)
            {
                return transform.position;
            }

            return collider.ClosestPoint(transform.position);
        }

    }
}
