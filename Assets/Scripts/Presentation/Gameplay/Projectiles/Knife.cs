using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Abstractions.Pooling;
using KnifeToss.Core.Abstractions.Services;
using KnifeToss.Core.Data;
using KnifeToss.Core.Constants;
using KnifeToss.Infrastructure.Services;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Facade for knife projectile. Coordinates movement, collision, sticking, animation, audio, and visibility components.
    /// Implements IPoolable for pool integration.
    /// </summary>
    [RequireComponent(typeof(KnifeMovement))]
    [RequireComponent(typeof(KnifeCollision))]
    [RequireComponent(typeof(KnifeSticking))]
    [RequireComponent(typeof(KnifeBounceAnimation))]
    [RequireComponent(typeof(KnifeAudio))]
    [RequireComponent(typeof(KnifeVisibility))]
    public sealed class Knife : MonoBehaviour, IPoolable
    {
        [SerializeField] private KnifeMovement _movement;
        [SerializeField] private KnifeCollision _collision;
        [SerializeField] private KnifeSticking _sticking;
        [SerializeField] private KnifeBounceAnimation _bounceAnimation;
        [SerializeField] private KnifeAudio _audio;
        [SerializeField] private KnifeVisibility _visibility;

        private KnifeThrower _thrower;
        private IKnifeTracker _knifeTracker;
        private IAudioService _audioService;
        private SoundSettings _soundSettings;
        private RuntimeCacheService _cacheService;
        private bool _isResolved;
        private bool _isReturnedToPool;

        private void Awake()
        {
            // Auto-assign components if not set in inspector
            _movement ??= GetComponent<KnifeMovement>();
            _collision ??= GetComponent<KnifeCollision>();
            _sticking ??= GetComponent<KnifeSticking>();
            _bounceAnimation ??= GetComponent<KnifeBounceAnimation>();
            _audio ??= GetComponent<KnifeAudio>();
            _visibility ??= GetComponent<KnifeVisibility>();
        }

        private void OnEnable()
        {
            _knifeTracker?.Register(this);
            if (_collision != null)
            {
                _collision.OnCollided += HandleCollision;
            }
        }

        private void OnDisable()
        {
            _knifeTracker?.Unregister(this);
            if (_collision != null)
            {
                _collision.OnCollided -= HandleCollision;
            }
            _bounceAnimation?.KillBounceSequence();
        }

        private void Update()
        {
            if (_isResolved || _sticking.IsStuck)
            {
                return;
            }

            // Check if knife is out of bounds
            if (_visibility != null && _visibility.IsOutOfBounds())
            {
                Debug.Log($"Knife '{name}' out of gameplay camera view, returning to pool");
                ReturnToPool();
            }
        }

        [VContainer.Inject]
        public void Construct(
            IKnifeTracker tracker,
            IAudioService audioService,
            SoundSettings soundSettings,
            RuntimeCacheService cacheService)
        {
            _knifeTracker = tracker;
            _audioService = audioService;
            _soundSettings = soundSettings;
            _cacheService = cacheService;

            _audio?.Initialize(_audioService, _soundSettings);
            _visibility?.Initialize(_cacheService);
        }

        /// <summary>
        /// Initializes the knife for throwing.
        /// </summary>
        public void Initialize(KnifeThrower knifeThrower, float throwSpeed, Vector2 direction)
        {
            _thrower = knifeThrower;
            _isResolved = false;
            _isReturnedToPool = false;

            _movement?.Launch(direction, throwSpeed);
            _collision?.Enable();
            _sticking?.ResetState();
        }

        /// <summary>
        /// Marks the knife as pre-stuck (spawned already stuck to target).
        /// </summary>
        public void MarkAsPreStuck()
        {
            _movement?.Halt();
            _collision?.Enable();
            _sticking?.MarkAsPreStuck();
            _isResolved = true;
        }

        public bool IsStuck => _sticking != null && _sticking.IsStuck;
        public float EmbedOffset => _sticking != null ? _sticking.EmbedOffset : 0.02f;

        private void HandleCollision(CollisionData data)
        {
            if (_isResolved || _sticking.IsStuck || _isReturnedToPool || data.Other == null)
            {
                return;
            }

            // Handle knife hitting another knife
            if (data.Other.CompareTag(Tags.Knife))
            {
                Knife otherKnife = data.Other.GetComponent<Knife>();
                if (otherKnife != null)
                {
                    HandleKnifeHitKnife(data.ImpactPoint);
                }
                return;
            }

            // Handle hitable objects
            HitableObjectView hitableView = data.Other.GetComponentInParent<HitableObjectView>();
            if (hitableView != null)
            {
                hitableView.ProcessHit(data.ImpactPoint);
                Collider2D hitableCollider = hitableView.GetComponent<Collider2D>()
                    ?? data.Other.GetComponent<Collider2D>();
                Transform anchor = hitableCollider != null ? hitableCollider.transform : hitableView.transform;
                StickToTarget(anchor, hitableCollider, data.ImpactPoint);
                return;
            }

            // Handle target
            if (data.Other.CompareTag(Tags.Target))
            {
                Collider2D targetCollider = data.Other.GetComponent<Collider2D>()
                    ?? data.Other.GetComponentInParent<Collider2D>();
                Transform anchor = targetCollider != null ? targetCollider.transform : data.OtherTransform;
                StickToTarget(anchor, targetCollider, data.ImpactPoint);
            }
        }

        private void StickToTarget(Transform anchor, Collider2D collider, Vector3 impactPoint)
        {
            _movement?.Halt();
            _collision?.Disable();
            _sticking?.StickToCollider(anchor, collider, impactPoint);
            _audio?.PlayHitTargetSound();
            Resolve();
        }

        private void HandleKnifeHitKnife(Vector3 impactPoint)
        {
            _isResolved = true;
            _movement?.Halt();
            transform.position = impactPoint;
            transform.SetParent(null);
            _collision?.Disable();
            _audio?.PlayHitKnifeSound();
            _bounceAnimation?.PlayBounceOff(impactPoint, ReturnToPool);
            Resolve();
        }

        private void ReturnToPool()
        {
            if (_isReturnedToPool)
            {
                return;
            }

            _isReturnedToPool = true;
            _isResolved = true;
            _movement?.Halt();
            transform.SetParent(null);
            _collision?.Disable();
            _bounceAnimation?.KillBounceSequence();
            Resolve();

            if (_thrower != null)
            {
                _thrower.ReturnToPool(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Resolve()
        {
            if (_isResolved)
            {
                return;
            }

            _isResolved = true;
            _thrower?.OnKnifeResolved();
        }

        public void OnSpawn()
        {
            ResetState();
        }

        public void OnDespawn()
        {
            ResetState();
            _bounceAnimation?.KillBounceSequence();
        }

        private void ResetState()
        {
            _isResolved = false;
            _isReturnedToPool = false;
            _thrower = null;
            _movement?.OnDespawn();
            _collision?.Disable();
            _sticking?.ResetState();
        }
    }
}

