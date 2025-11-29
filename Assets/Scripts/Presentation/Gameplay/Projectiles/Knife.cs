using DG.Tweening;
using KnifeToss.Core.Events;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    public class Knife : MonoBehaviour
    {
        private const float DefaultFallDistance = 3f;
        private const float DefaultFallDuration = 0.5f;
        private const float DefaultHorizontalDrift = 0.2f;
        private const float DefaultRotationDegrees = 540f;

        private KnifeThrower thrower;
        private float speed;
        private bool isResolved;
        private bool isStuck;
        private Rigidbody2D rb2d;
        private Sequence failSequence;
        private Collider2D[] cachedColliders;

        [Header("Stick Settings")]
        [SerializeField] private float embedOffset = 0.02f;
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("Fail Animation")]
        [SerializeField] private KnifeFailAnimationConfig failAnimationConfig;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip hitTargetSound;
        [SerializeField] private AudioClip hitKnifeSound;
        [SerializeField] private float soundVolume = 1f;

        private void Awake()
        {
            cachedColliders = GetComponents<Collider2D>();
        }

        private void OnDisable()
        {
            KillFailSequence();
        }

        private void OnDestroy()
        {
            KillFailSequence();
        }

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

            SetCollidersEnabled(true);
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
            HaltMotion();

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
            HaltMotion();
            if (rb2d != null)
            {
                rb2d.simulated = false;
            }

            transform.SetParent(null);
            SetCollidersEnabled(false);

            thrower?.OnKnifeResolved();
            PlayFailAnimation();

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
            HaltMotion();
        }

        public bool IsStuck => isStuck;
        public float EmbedOffset => embedOffset;

        private void HaltMotion()
        {
            speed = 0f;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            if (cachedColliders == null || cachedColliders.Length == 0)
            {
                cachedColliders = GetComponents<Collider2D>();
            }

            for (int i = 0; i < cachedColliders.Length; i++)
            {
                Collider2D collider = cachedColliders[i];
                if (collider != null)
                {
                    collider.enabled = enabled;
                }
            }
        }

        private void PlayFailAnimation()
        {
            KillFailSequence();

            float fallDistance = failAnimationConfig != null
                ? failAnimationConfig.FallDistance
                : DefaultFallDistance;
            float fallDuration = failAnimationConfig != null
                ? failAnimationConfig.FallDuration
                : DefaultFallDuration;
            float drift = failAnimationConfig != null
                ? failAnimationConfig.HorizontalDrift
                : DefaultHorizontalDrift;
            float rotationDegrees = failAnimationConfig != null
                ? failAnimationConfig.RotationDegrees
                : DefaultRotationDegrees;
            float rotationDuration = failAnimationConfig != null
                ? failAnimationConfig.RotationDuration
                : fallDuration;
            Ease moveEase = failAnimationConfig != null
                ? failAnimationConfig.MovementEase
                : Ease.InQuad;
            Ease rotationEase = failAnimationConfig != null
                ? failAnimationConfig.RotationEase
                : Ease.OutQuad;

            Vector3 horizontalOffset = Vector3.right * Random.Range(-drift, drift);
            Vector3 targetPosition = transform.position + horizontalOffset + Vector3.down * fallDistance;

            failSequence = DOTween.Sequence().SetTarget(this);
            failSequence.Join(transform.DOMove(targetPosition, fallDuration).SetEase(moveEase));
            failSequence.Join(
                transform
                    .DORotate(Vector3.forward * rotationDegrees, rotationDuration, RotateMode.FastBeyond360)
                    .SetEase(rotationEase)
                    .SetRelative());
            failSequence.OnKill(() => failSequence = null);
        }

        private void KillFailSequence()
        {
            if (failSequence != null)
            {
                failSequence.Kill();
                failSequence = null;
            }
        }
    }
}
