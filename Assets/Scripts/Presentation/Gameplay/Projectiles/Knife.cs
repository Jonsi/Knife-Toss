using DG.Tweening;
using KnifeToss.Core.Events;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;
using VContainer;

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
        private Core.Abstractions.IKnifeTracker knifeTracker;
        private Core.Abstractions.IAudioPlayer audioPlayer;

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

        private void OnEnable()
        {
            knifeTracker?.Register(this);
        }

        private void OnDisable()
        {
            knifeTracker?.Unregister(this);
            KillFailSequence();
        }

        private void OnDestroy()
        {
            knifeTracker?.Unregister(this);
            KillFailSequence();
        }

        [VContainer.Inject]
        public void Construct(Core.Abstractions.IKnifeTracker tracker, Core.Abstractions.IAudioPlayer audioPlayer)
        {
            knifeTracker = tracker;
            this.audioPlayer = audioPlayer;
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
            if (!CanResolveImpact())
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
            ResolveAsStuck();
            AlignToTarget(targetTransform);
            thrower?.OnKnifeResolved();
            PlaySound(hitTargetSound);
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
            PlaySound(hitKnifeSound);
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

        private bool CanResolveImpact()
        {
            return !isResolved && !isStuck;
        }

        private void ResolveAsStuck()
        {
            isStuck = true;
            isResolved = true;
            HaltMotion();
        }

        private void AlignToTarget(Transform targetTransform)
        {
            Vector2 targetCenter = targetTransform.position;
            Vector2 direction = ((Vector2)transform.position - targetCenter).normalized;
            CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
            float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;
            Vector2 embeddedPosition = targetCenter + direction * (targetRadius + embedOffset);
            transform.position = embeddedPosition;
            transform.SetParent(targetTransform);
        }

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

            float fallDistance = ResolveOrDefault(() => failAnimationConfig.FallDistance, DefaultFallDistance);
            float fallDuration = ResolveOrDefault(() => failAnimationConfig.FallDuration, DefaultFallDuration);
            float drift = ResolveOrDefault(() => failAnimationConfig.HorizontalDrift, DefaultHorizontalDrift);
            float rotationDegrees = ResolveOrDefault(() => failAnimationConfig.RotationDegrees, DefaultRotationDegrees);
            float rotationDuration = ResolveOrDefault(() => failAnimationConfig.RotationDuration, fallDuration);
            Ease moveEase = ResolveOrDefault(() => failAnimationConfig.MovementEase, Ease.InQuad);
            Ease rotationEase = ResolveOrDefault(() => failAnimationConfig.RotationEase, Ease.OutQuad);

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

        private void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            (audioPlayer ?? NullAudioPlayer.Instance).Play(clip, transform.position, soundVolume);
        }

        private T ResolveOrDefault<T>(System.Func<T> resolver, T fallback)
        {
            if (failAnimationConfig == null)
            {
                return fallback;
            }

            return resolver();
        }

        private sealed class NullAudioPlayer : Core.Abstractions.IAudioPlayer
        {
            public static readonly NullAudioPlayer Instance = new NullAudioPlayer();

            public void Play(AudioClip clip, Vector3 position, float volume)
            {
            }
        }
    }
}
