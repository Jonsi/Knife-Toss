using DG.Tweening;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Events;
using KnifeToss.Core.Data;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Represents the Knife class.
    /// </summary>
    public class Knife : MonoBehaviour
    {
        private const float DefaultFallDistance = 3f;
        private const float DefaultFallDuration = 0.5f;
        private const float DefaultHorizontalDrift = 0.2f;
        private const float DefaultRotationDegrees = 540f;

        private static readonly Vector2 DefaultLaunchDirection = Vector2.up;

        private KnifeThrower thrower;
        private float speed;
        private Vector2 launchDirection = DefaultLaunchDirection;
        private bool isResolved;
        private bool isStuck;
        private bool isReturnedToPool;
        private Rigidbody2D rb2d;
        [SerializeField] private Transform snapPosition;
        private Sequence bounceSequence;
        private Collider2D[] cachedColliders;
        private IKnifeTracker knifeTracker;
        private IAudioPlayer audioPlayer;
        [SerializeField] private Camera gameplayCamera;

        [Header("Stick Settings")]
        [SerializeField] private float embedOffset = 0.02f;
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("Bounce Animation")]
        [SerializeField] private KnifeBounceOffAnimationConfig bounceAnimationConfig;

        [SerializeField] private float soundVolume = 1f;
        private SoundSettings soundSettings;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            cachedColliders = GetComponents<Collider2D>();
            ResolveSnapPosition();
            gameplayCamera ??= Camera.main;
        }

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            knifeTracker?.Register(this);
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            knifeTracker?.Unregister(this);
            KillBounceSequence();
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            knifeTracker?.Unregister(this);
            KillBounceSequence();
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [VContainer.Inject]
        public void Construct(IKnifeTracker tracker, IAudioPlayer audioPlayer, SoundSettings soundSettings)
        {
            knifeTracker = tracker;
            this.audioPlayer = audioPlayer;
            this.soundSettings = soundSettings;
        }

        /// <summary>
        /// Performs the Initialize operation.
        /// </summary>
        public void Initialize(KnifeThrower knifeThrower, float throwSpeed, Vector2 direction)
        {
            thrower = knifeThrower;
            speed = throwSpeed;
            launchDirection = direction.sqrMagnitude > Mathf.Epsilon
                ? direction.normalized
                : DefaultLaunchDirection;
            isResolved = false;
            isStuck = false;
            isReturnedToPool = false;

            transform.up = launchDirection;

            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.simulated = true;
                rb2d.gravityScale = 0f;
                rb2d.bodyType = RigidbodyType2D.Dynamic;
            }

            SetCollidersEnabled(true);
        }

        /// <summary>
        /// Performs the Update operation.
        /// </summary>
        private void Update()
        {
            if (isResolved || isStuck)
            {
                return;
            }

            if (rb2d != null)
            {
                rb2d.linearVelocity = launchDirection * speed;
            }
            else
            {
                transform.position += (Vector3)(launchDirection * speed * Time.deltaTime);
            }

            if (ShouldReturnBasedOnGameplayCamera())
            {
                Debug.Log($"Knife '{name}' out of gameplay camera view, returning to pool");
                ReturnToPool();
            }
        }

        /// <summary>
        /// Performs the OnTriggerEnter2D operation.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision.transform, collision.gameObject, ResolveImpactPoint(collision));
        }

        /// <summary>
        /// Performs the OnCollisionEnter2D operation.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.transform, collision.gameObject, ResolveImpactPoint(collision));
        }

        /// <summary>
        /// Performs the OnBecameInvisible operation.
        /// </summary>
        private void OnBecameInvisible() { }

        /// <summary>
        /// Performs the HandleCollision operation.
        /// </summary>
        private void HandleCollision(Transform collisionTransform, GameObject collisionObject, Vector3 impactPoint)
        {
            if (!CanResolveImpact() || collisionObject == null)
            {
                return;
            }

            if (collisionObject.CompareTag("Knife"))
            {
                Knife otherKnife = collisionObject.GetComponent<Knife>();
                if (otherKnife != null)
                {
                    HandleKnifeHitKnife(collisionTransform != null ? collisionTransform.position : transform.position);
                }
                return;
            }

            HitableObjectView hitableView = collisionObject.GetComponentInParent<HitableObjectView>();
            if (hitableView != null)
            {
                hitableView.ProcessHit(impactPoint);
                Collider2D hitableCollider = hitableView.GetComponent<Collider2D>()
                    ?? collisionObject.GetComponent<Collider2D>();
                Transform anchor = hitableCollider != null ? hitableCollider.transform : hitableView.transform;
                StickToCollider(anchor, hitableCollider, impactPoint);
                return;
            }

            if (collisionObject.CompareTag("Target"))
            {
                Collider2D targetCollider = collisionObject.GetComponent<Collider2D>()
                    ?? collisionObject.GetComponentInParent<Collider2D>();
                Transform anchor = targetCollider != null ? targetCollider.transform : collisionTransform;
                StickToCollider(anchor, targetCollider, impactPoint);
            }
        }

        /// <summary>
        /// Performs the StickToCircle operation using collider data.
        /// </summary>
        private void StickToCollider(Transform anchor, Collider2D collider, Vector3 impactPoint)
        {
            if (anchor == null)
            {
                return;
            }

            ResolveCircleSnap(anchor, collider, impactPoint, out Vector3 center, out Vector3 surfacePoint, out Vector2 surfaceDirection);

            ResolveAsStuck();
            AlignToSurface(anchor, center, surfacePoint, surfaceDirection);
            PlaySound(soundSettings != null ? soundSettings.HitTargetSound : null);
            impactEventChannel?.RaiseKnifeStuck(impactPoint);
            thrower?.OnKnifeResolved();
        }

        /// <summary>
        /// Performs the HandleKnifeHitKnife operation.
        /// </summary>
        private void HandleKnifeHitKnife(Vector3 impactPoint)
        {
            if (isResolved || isReturnedToPool)
            {
                return;
            }

            isResolved = true;
            HaltMotion();
            if (rb2d != null)
            {
                rb2d.simulated = false;
            }

            transform.position = impactPoint;
            transform.SetParent(null);
            SetCollidersEnabled(false);

            thrower?.OnKnifeResolved();
            PlaySound(soundSettings != null ? soundSettings.HitKnifeSound : null);
            PlayBounceOffAnimation();
        }

        /// <summary>
        /// Performs the ReturnToPool operation.
        /// </summary>
        private void ReturnToPool()
        {
            if (isReturnedToPool)
            {
                return;
            }

            isReturnedToPool = true;
            isResolved = true;
            HaltMotion();
            if (rb2d != null)
            {
                rb2d.simulated = false;
            }

            transform.SetParent(null);
            SetCollidersEnabled(false);
            KillBounceSequence();
            thrower?.OnKnifeResolved();

            if (thrower != null)
            {
                thrower.ReturnToPool(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Performs the Resolve operation.
        /// </summary>
        public void Resolve()
        {
            if (isResolved)
            {
                return;
            }

            isResolved = true;
            thrower?.OnKnifeResolved();
        }

        /// <summary>
        /// Performs the MarkAsPreStuck operation.
        /// </summary>
        public void MarkAsPreStuck()
        {
            isStuck = true;
            isResolved = true;
            isReturnedToPool = false;
            HaltMotion();
            SetCollidersEnabled(true);
        }

        public bool IsStuck => isStuck;
        public float EmbedOffset => embedOffset;

        /// <summary>
        /// Performs the CanResolveImpact operation.
        /// </summary>
        private bool CanResolveImpact()
        {
            return !isResolved && !isStuck && !isReturnedToPool;
        }

        /// <summary>
        /// Performs the ResolveAsStuck operation.
        /// </summary>
        private void ResolveAsStuck()
        {
            isStuck = true;
            isResolved = true;
            isReturnedToPool = false;
            HaltMotion();
        }

        /// <summary>
        /// Performs the AlignToCircleEdge operation.
        /// </summary>
        private void AlignToSurface(Transform anchorParent, Vector3 circleCenter, Vector3 surfacePoint, Vector2 surfaceDirection)
        {
            Vector2 direction = surfaceDirection.sqrMagnitude > Mathf.Epsilon
                ? surfaceDirection.normalized
                : ResolveImpactDirection(circleCenter, surfacePoint);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -direction);

            Vector3 snapTarget = surfacePoint;
            if (TryResolveSnapOffset(rotation, out Vector3 snapOffset))
            {
                transform.SetPositionAndRotation(snapTarget - snapOffset, rotation);
            }
            else
            {
                transform.SetPositionAndRotation(snapTarget, rotation);
            }

            transform.SetParent(anchorParent, true);
        }

        /// <summary>
        /// Performs the HaltMotion operation.
        /// </summary>
        private void HaltMotion()
        {
            speed = 0f;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
        }

        /// <summary>
        /// Performs the SetCollidersEnabled operation.
        /// </summary>
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

        /// <summary>
        /// Performs the PlayBounceOffAnimation operation.
        /// </summary>
        private void PlayBounceOffAnimation()
        {
            KillBounceSequence();

            float fallDistance = ResolveOrDefault(() => bounceAnimationConfig.FallDistance, DefaultFallDistance);
            float fallDuration = ResolveOrDefault(() => bounceAnimationConfig.FallDuration, DefaultFallDuration);
            float drift = ResolveOrDefault(() => bounceAnimationConfig.HorizontalDrift, DefaultHorizontalDrift);
            float rotationDegrees = ResolveOrDefault(() => bounceAnimationConfig.RotationDegrees, DefaultRotationDegrees);
            float rotationDuration = ResolveOrDefault(() => bounceAnimationConfig.RotationDuration, fallDuration);
            Ease moveEase = ResolveOrDefault(() => bounceAnimationConfig.MovementEase, Ease.InQuad);
            Ease rotationEase = ResolveOrDefault(() => bounceAnimationConfig.RotationEase, Ease.OutQuad);

            Vector3 horizontalOffset = Vector3.right * Random.Range(-drift, drift);
            Vector3 targetPosition = transform.position + horizontalOffset + Vector3.down * fallDistance;

            bounceSequence = DOTween.Sequence().SetTarget(this);
            bounceSequence.Join(transform.DOMove(targetPosition, fallDuration).SetEase(moveEase));
            bounceSequence.Join(
                transform
                    .DORotate(Vector3.forward * rotationDegrees, rotationDuration, RotateMode.FastBeyond360)
                    .SetEase(rotationEase)
                    .SetRelative());
            bounceSequence.OnComplete(ReturnToPool);
            bounceSequence.OnKill(() => bounceSequence = null);
        }

        /// <summary>
        /// Performs the KillBounceSequence operation.
        /// </summary>
        private void KillBounceSequence()
        {
            if (bounceSequence != null)
            {
                bounceSequence.Kill();
                bounceSequence = null;
            }
        }

        /// <summary>
        /// Performs the PlaySound operation.
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            IAudioPlayer player = audioPlayer ?? NullAudioPlayer.Instance;

            player.Play2D(clip, soundVolume);
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

        /// <summary>
        /// Performs the ResolveImpactDirection operation.
        /// </summary>
        private Vector2 ResolveImpactDirection(Vector3 circleCenter, Vector3 impactPoint)
        {
            Vector2 fromCenter = (Vector2)(impactPoint - circleCenter);
            if (fromCenter.sqrMagnitude < Mathf.Epsilon)
            {
                fromCenter = (Vector2)(transform.position - circleCenter);
            }

            return fromCenter.sqrMagnitude > Mathf.Epsilon ? fromCenter.normalized : Vector2.up;
        }

        /// <summary>
        /// Performs the TryResolveSnapOffset operation.
        /// </summary>
        private bool TryResolveSnapOffset(Quaternion rotation, out Vector3 snapOffset)
        {
            Transform resolvedSnap = ResolveSnapPosition();
            if (resolvedSnap == null)
            {
                snapOffset = Vector3.zero;
                return false;
            }

            Vector3 scaledLocal = Vector3.Scale(resolvedSnap.localPosition, transform.lossyScale);
            snapOffset = rotation * scaledLocal;
            return true;
        }

        private bool ShouldReturnBasedOnGameplayCamera()
        {
            Camera cam = gameplayCamera != null ? gameplayCamera : Camera.main;
            if (cam == null)
            {
                return false;
            }

            Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);
            const float margin = 0.05f;

            if (viewportPosition.z < 0f)
            {
                return true;
            }

            return viewportPosition.x < -margin
                   || viewportPosition.x > 1f + margin
                   || viewportPosition.y < -margin
                   || viewportPosition.y > 1f + margin;
        }

        /// <summary>
        /// Performs the ResolveSnapPosition operation.
        /// </summary>
        private Transform ResolveSnapPosition()
        {
            if (snapPosition != null)
            {
                return snapPosition;
            }

            snapPosition = transform.Find("SnapPosition");
            return snapPosition;
        }

        /// <summary>
        /// Resolves circle collider center and surface point for snapping.
        /// </summary>
        private void ResolveCircleSnap(Transform anchor, Collider2D collider, Vector3 impactPoint, out Vector3 center, out Vector3 surfacePoint, out Vector2 surfaceDirection)
        {
            CircleCollider2D circle = collider as CircleCollider2D;
            if (circle == null && anchor != null)
            {
                circle = anchor.GetComponent<CircleCollider2D>();
            }

            center = anchor != null ? anchor.position : impactPoint;
            surfaceDirection = ResolveImpactDirection(center, impactPoint);
            surfacePoint = center;

            if (circle == null)
            {
                return;
            }

            center = circle.transform.TransformPoint(circle.offset);
            float uniformScale = Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
            float radius = circle.radius * uniformScale;

            surfaceDirection = ResolveImpactDirection(center, impactPoint);
            surfaceDirection = surfaceDirection.sqrMagnitude > Mathf.Epsilon ? surfaceDirection.normalized : Vector2.up;
            surfacePoint = center + (Vector3)(surfaceDirection * radius);
        }

        /// <summary>
        /// Performs the ResolveOrDefault<T> operation.
        /// </summary>
        private T ResolveOrDefault<T>(System.Func<T> resolver, T fallback)
        {
            if (bounceAnimationConfig == null)
            {
                return fallback;
            }

            return resolver();
        }

        private sealed class NullAudioPlayer : IAudioPlayer
        {
            public static readonly NullAudioPlayer Instance = new NullAudioPlayer();

            /// <summary>
            /// Performs the Play operation.
            /// </summary>
            public void Play(AudioClip clip, Vector3 position, float volume)
            {
            }

            /// <summary>
            /// Performs the Play2D operation.
            /// </summary>
            public void Play2D(AudioClip clip)
            {
            }

            /// <summary>
            /// Performs the Play2D operation.
            /// </summary>
            public void Play2D(AudioClip clip, float volume)
            {
            }

            /// <summary>
            /// Performs the SetMuted operation.
            /// </summary>
            public void SetMuted(bool isMuted)
            {
            }
        }
    }
}
