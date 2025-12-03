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
        private Rigidbody2D rb2d;
        [SerializeField] private Transform snapPosition;
        private Sequence failSequence;
        private Collider2D[] cachedColliders;
        private IKnifeTracker knifeTracker;
        private IAudioPlayer audioPlayer;

        [Header("Stick Settings")]
        [SerializeField] private float embedOffset = 0.02f;
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        [Header("Fail Animation")]
        [SerializeField] private KnifeFailAnimationConfig failAnimationConfig;

        [SerializeField] private float soundVolume = 1f;
        private SoundSettings soundSettings;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            cachedColliders = GetComponents<Collider2D>();
            ResolveSnapPosition();
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
            KillFailSequence();
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            knifeTracker?.Unregister(this);
            KillFailSequence();
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

            transform.up = launchDirection;

            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
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
                    TriggerFail(collisionTransform != null ? collisionTransform.position : transform.position);
                }
                return;
            }

            HitableObjectView hitableView = collisionObject.GetComponentInParent<HitableObjectView>();
            if (hitableView != null)
            {
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
            AlignToSurface(anchor, surfacePoint);
            thrower?.OnKnifeResolved();
            PlaySound(soundSettings != null ? soundSettings.HitTargetSound : null);
            impactEventChannel?.RaiseKnifeStuck(impactPoint);
        }

        /// <summary>
        /// Performs the TriggerFail operation.
        /// </summary>
        private void TriggerFail(Vector3 failurePoint)
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
            PlaySound(soundSettings != null ? soundSettings.HitKnifeSound : null);
            impactEventChannel?.RaiseKnifeFailed(failurePoint);
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
            return !isResolved && !isStuck;
        }

        /// <summary>
        /// Performs the ResolveAsStuck operation.
        /// </summary>
        private void ResolveAsStuck()
        {
            isStuck = true;
            isResolved = true;
            HaltMotion();
        }

        /// <summary>
        /// Performs the AlignToCircleEdge operation.
        /// </summary>
        private void AlignToSurface(Transform anchorParent, Vector3 surfacePoint)
        {
            Quaternion rotation = transform.rotation;

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
        /// Performs the PlayFailAnimation operation.
        /// </summary>
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

        /// <summary>
        /// Performs the KillFailSequence operation.
        /// </summary>
        private void KillFailSequence()
        {
            if (failSequence != null)
            {
                failSequence.Kill();
                failSequence = null;
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
            if (failAnimationConfig == null)
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
