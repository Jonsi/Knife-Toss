using KnifeToss.Core.Events;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    public class Knife : MonoBehaviour
    {
        private KnifeThrower thrower;
        private float speed;
        private bool isResolved;
        private bool isStuck;
        private Rigidbody2D rb2d;

        [Header("Stick Settings")]
        [SerializeField] private float embedOffset = 0.02f;
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip hitTargetSound;
        [SerializeField] private AudioClip hitKnifeSound;
        [SerializeField] private float soundVolume = 1f;

        public void Initialize(KnifeThrower knifeThrower, float throwSpeed)
        {
            thrower = knifeThrower;
            speed = throwSpeed;
            isResolved = false;
            isStuck = false;

            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.gravityScale = 0f;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        private void Update()
        {
            if (isResolved || isStuck)
            {
                return;
            }

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.up * speed;
            }
            else
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision.transform, collision.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.transform, collision.gameObject);
        }

        private void HandleCollision(Transform collisionTransform, GameObject collisionObject)
        {
            if (isResolved || isStuck)
            {
                return;
            }

            if (collisionObject.CompareTag("Target"))
            {
                StickToTarget(collisionTransform);
                return;
            }

            if (collisionObject.CompareTag("Knife"))
            {
                Knife otherKnife = collisionObject.GetComponent<Knife>();
                if (otherKnife != null)
                {
                    TriggerFail();
                }
            }
        }

        private void StickToTarget(Transform targetTransform)
        {
            isStuck = true;
            isResolved = true;
            speed = 0f;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
            }

            Vector2 targetCenter = targetTransform.position;
            Vector2 knifePosition = transform.position;
            Vector2 direction = (knifePosition - targetCenter).normalized;

            CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
            float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;

            Vector2 embeddedPosition = targetCenter + direction * (targetRadius + embedOffset);
            transform.position = embeddedPosition;
            transform.SetParent(targetTransform);

            thrower?.OnKnifeResolved();

            if (hitTargetSound != null)
            {
                AudioSource.PlayClipAtPoint(hitTargetSound, transform.position, soundVolume);
            }

            impactEventChannel?.RaiseKnifeStuck();
        }

        private void TriggerFail()
        {
            if (isResolved)
            {
                return;
            }

            isResolved = true;
            speed = 0f;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
            }

            thrower?.OnKnifeResolved();

            if (hitKnifeSound != null)
            {
                AudioSource.PlayClipAtPoint(hitKnifeSound, transform.position, soundVolume);
            }

            impactEventChannel?.RaiseKnifeFailed();
        }

        public void Resolve()
        {
            if (isResolved)
            {
                return;
            }

            isResolved = true;
            thrower?.OnKnifeResolved();
        }

        public void MarkAsPreStuck()
        {
            isStuck = true;
            isResolved = true;
            speed = 0f;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
            }
        }

        public bool IsStuck => isStuck;
        public float EmbedOffset => embedOffset;
    }
}
