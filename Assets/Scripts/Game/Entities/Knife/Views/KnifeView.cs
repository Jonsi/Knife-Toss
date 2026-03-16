using System;
using Common.Collision.Data;
using Common.Collision.Interfaces;
using Common.Events;
using Common.Interfaces;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Interfaces;
using Game.Entities.Knife.States;
using Game.Feedback.Animation;
using Game.Feedback.Animation.Data;
using Game.Entities.Target.Data;
using Game.Pooling;
using Game.PowerUps.Configs;
using Game.Store.Services;
using UnityEngine;
using VContainer;

namespace Game.Entities.Knife.Views
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior.
    /// Implements IHitable to participate in the collision system.
    /// </summary>
    public class KnifeView : MonoBehaviour, IKnifeViewContext, IStickable
    {
        private static readonly Type CachedType = typeof(KnifeView);

        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private Transform _VisualRoot;
        [SerializeField] private Collider2D _Collider;
        [SerializeField] private Collider2D _HitCollider;
        [SerializeField] private SpriteShatterController _ShatterController;
        [SerializeField] private KnifeFailAnimationConfig _FailAnimationConfig;

        private KnifeStateMachine _StateMachine;
        private Camera _MainCamera;
        private bool _HasReportedOutOfView;
        private bool _SuppressOutOfViewMonitoring;
        private bool _IsSuperKnife;
        private PowerUpConfigBase _SourcePowerUpConfig;
        private Sprite _DefaultSprite;
        private Coroutine _FailSequenceCoroutine;
        private GameObject _SuperKnifeVfxInstance;
        private ParticleSystem[] _SuperKnifeVfxParticleSystems;
        private Renderer[] _SuperKnifeVfxRenderers;

        private IEventChannel<CollisionData> _CollisionChannel;
        private IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private KnifePoolProvider _KnifePoolProvider;
        private KnifeSkinService _KnifeSkinService;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets whether the knife is currently in flight.
        /// </summary>
        public bool IsInFlight => _StateMachine?.IsInFlight ?? false;

        /// <summary>
        /// Exposes transform for state consumption.
        /// </summary>
        public Transform Transform => transform;

        public Transform VisualRoot => _VisualRoot != null ? _VisualRoot : transform;
        public SpriteRenderer SpriteRenderer => _SpriteRenderer;

        public float RadiusOffset => 0f;
        public float RotationOffset => 0f;

        /// <inheritdoc />
        public void ApplySorting(int sortingLayerId, int sortingOrder)
        {
            if (_SpriteRenderer == null)
                return;

            _SpriteRenderer.sortingLayerID = sortingLayerId;
            _SpriteRenderer.sortingOrder = sortingOrder;

            if (_SourcePowerUpConfig is SuperKnifeConfig config)
            {
                ApplySuperKnifeVfxSorting(config);
            }
        }

        public void OnAttachedToTarget()
        {
            // Optional: Logic when attached to target
        }

        /// <summary>
        /// Gets or sets whether this knife should behave as a super knife.
        /// </summary>
        public bool IsSuperKnife
        {
            get => _IsSuperKnife;
            set
            {
                if (_IsSuperKnife == value)
                    return;

                _IsSuperKnife = value;
                UpdateSuperKnifeVfxState();
            }
        }

        public PowerUpConfigBase SourcePowerUpConfig => _SourcePowerUpConfig;

        [Inject]
        private void Construct(
            IEventChannel<CollisionData> collisionChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            KnifeSkinService knifeSkinService)
        {
            _CollisionChannel = collisionChannel;
            _ImpactChannel = impactChannel;
            _KnifeSkinService = knifeSkinService;
        }

        private void Awake()
        {
            InitializeStateMachine();
            _MainCamera = Camera.main;
            if (_SpriteRenderer != null)
            {
                _DefaultSprite = _SpriteRenderer.sprite;
            }
        }

        private void Update()
        {
            _StateMachine?.Update(Time.deltaTime);
            CheckOutOfView();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collisionChannel = _CollisionChannel;
            if (collisionChannel == null)
                return;

            var hitable = other.GetComponent<IHitable>();
            if (hitable == null)
                return;

            // Calculate impact point as the closest point on the other collider
            Vector2 impactPoint = other.ClosestPoint(transform.position);
            collisionChannel.Raise(new CollisionData(this, hitable, impactPoint));
        }

        /// <summary>
        /// Launches the knife in the specified direction with the given speed.
        /// </summary>
        /// <param name="direction">The direction vector in which to launch the knife.</param>
        /// <param name="speed">The speed at which to launch the knife.</param>
        public void Launch(Vector2 direction, float speed)
        {
            _HasReportedOutOfView = false;
            _StateMachine.InFlight.Launch(direction, speed);
            _StateMachine.SetState(_StateMachine.InFlight);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            _StateMachine.SetState(_StateMachine.Stuck);
        }

        /// <summary>
        /// Transitions the knife to the bounced state.
        /// </summary>
        public void Bounce()
        {
            StopFailSequence(restoreVisuals: true, restoreTimeScale: true);
            _FailSequenceCoroutine = StartCoroutine(PlayFailSequence());
        }


        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            StopFailSequence(restoreVisuals: true, restoreTimeScale: true);
            _HasReportedOutOfView = false;
            _SuppressOutOfViewMonitoring = false;
            IsSuperKnife = false;
            _SourcePowerUpConfig = null;
            RefreshVisualSkin();
            SetSpriteAlpha(1f);
            _StateMachine.SetState(_StateMachine.Loaded);
        }

        public void RefreshVisualSkin()
        {
            if (_SpriteRenderer == null)
            {
                return;
            }

            var sprite = _KnifeSkinService?.GetEquippedConfig()?.GetKnifeSprite();
            _SpriteRenderer.sprite = sprite != null ? sprite : _DefaultSprite;
        }


        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void InitializeStateMachine()
        {
            _StateMachine = new KnifeStateMachine(
                new KnifeLoadedState(this),
                new KnifeInFlightState(this),
                new KnifeStuckState(this),
                new KnifeBouncedState(this),
                new KnifePooledState(this));
        }

        public void SetColliderEnabled(bool enabled)
        {
            // Determine if we are in the Stuck state
            bool isStuck = _StateMachine != null && _StateMachine.Current == _StateMachine.Stuck;

            if (isStuck)
            {
                // In stuck state, we use the HitCollider (if assigned)
                if (_HitCollider != null)
                {
                    _HitCollider.enabled = enabled;
                    // Ensure the main collider is disabled
                    if (_Collider != null) _Collider.enabled = false;
                }
                else
                {
                    // Fallback if HitCollider is missing: use main collider
                    if (_Collider != null) _Collider.enabled = enabled;
                }
            }
            else
            {
                // In other states (Loaded, InFlight, etc.), use the main collider
                if (_Collider != null) _Collider.enabled = enabled;

                // Ensure HitCollider is disabled
                if (_HitCollider != null) _HitCollider.enabled = false;
            }
        }

        private void CheckOutOfView()
        {
            if (_HasReportedOutOfView)
                return;

            var mainCamera = _MainCamera;
            if (mainCamera == null)
                return;

            if (_StateMachine == null || !_StateMachine.ShouldMonitorOutOfView)
                return;

            if (_SuppressOutOfViewMonitoring)
                return;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool isOutOfView = viewportPoint.x < 0f || viewportPoint.x > 1f || viewportPoint.y < 0f || viewportPoint.y > 1f;

            if (!isOutOfView)
                return;

            _HasReportedOutOfView = true;
            _ImpactChannel?.Raise(new KnifeImpactEventData(KnifeImpactType.OutOfView, transform.position));
            ReturnToPool();
        }

        /// <summary>
        /// Plays the configured shatter effect for this knife if available.
        /// </summary>
        public void PlayShatterEffect()
        {
            _ShatterController?.PlayShatter();
        }

        public void ReturnToPool()
        {
            StopFailSequence(restoreVisuals: true, restoreTimeScale: false);

            if (_KnifePoolProvider == null)
                return;

            _StateMachine?.SetState(_StateMachine.Pooled);
            _KnifePoolProvider.Release(this);
        }

        internal void AssignPoolProvider(KnifePoolProvider poolProvider)
        {
            _KnifePoolProvider = poolProvider;
        }

        internal void SetSourcePowerUpConfig(PowerUpConfigBase config)
        {
            _SourcePowerUpConfig = config;
            UpdateSuperKnifeVfxState();
        }

        private System.Collections.IEnumerator PlayFailSequence()
        {
            var config = GetFailAnimationConfig();
            _SuppressOutOfViewMonitoring = true;
            SetColliderEnabled(false);

            _StateMachine.Bounced.ConfigureBounce(config.InitialVelocity, config.AngularVelocity, config.Gravity);
            _StateMachine.SetState(_StateMachine.Bounced);

            float duration = Mathf.Max(0.01f, config.SequenceDuration);
            float flashDuration = Mathf.Max(0f, config.FlashDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                Time.timeScale = Mathf.Clamp01(config.TimeScaleOverSequence.Evaluate(normalizedTime));
                SetSpriteAlpha(Mathf.Clamp01(config.KnifeAlphaOverSequence.Evaluate(normalizedTime)));

                if (flashDuration > 0f && elapsed <= flashDuration)
                {
                    float flashNormalizedTime = Mathf.Clamp01(elapsed / flashDuration);
                    KnifeFailScreenFlash.Instance.SetAlpha(Mathf.Clamp01(config.FlashAlphaCurve.Evaluate(flashNormalizedTime)));
                }
                else
                {
                    KnifeFailScreenFlash.Instance.Hide();
                }

                yield return null;
            }

            SetSpriteAlpha(0f);
            KnifeFailScreenFlash.Instance.Hide();
            CompleteFailSequence();
        }

        private void CompleteFailSequence()
        {
            _FailSequenceCoroutine = null;
            _SuppressOutOfViewMonitoring = false;
            Time.timeScale = 1f;
            KnifeFailScreenFlash.Instance.Hide();
            _HasReportedOutOfView = true;
            _ImpactChannel?.Raise(new KnifeImpactEventData(KnifeImpactType.OutOfView, transform.position));
            ReturnToPool();
        }

        private void StopFailSequence(bool restoreVisuals, bool restoreTimeScale)
        {
            if (_FailSequenceCoroutine != null)
            {
                StopCoroutine(_FailSequenceCoroutine);
                _FailSequenceCoroutine = null;
            }

            _SuppressOutOfViewMonitoring = false;
            KnifeFailScreenFlash.Instance.Hide();

            if (restoreTimeScale)
            {
                Time.timeScale = 1f;
            }

            if (restoreVisuals)
            {
                SetSpriteAlpha(1f);
            }
        }

        private void SetSpriteAlpha(float alpha)
        {
            if (_SpriteRenderer == null)
                return;

            Color color = _SpriteRenderer.color;
            color.a = Mathf.Clamp01(alpha);
            _SpriteRenderer.color = color;
        }

        private void UpdateSuperKnifeVfxState()
        {
            if (!_IsSuperKnife)
            {
                StopSuperKnifeVfx();
                return;
            }

            if (_SourcePowerUpConfig is not SuperKnifeConfig config || config.VfxPrefab == null)
            {
                StopSuperKnifeVfx();
                return;
            }

            EnsureSuperKnifeVfxInstance(config);
            PlaySuperKnifeVfx(config);
        }

        private void EnsureSuperKnifeVfxInstance(SuperKnifeConfig config)
        {
            if (_SuperKnifeVfxInstance == null)
            {
                var parent = VisualRoot != null ? VisualRoot : transform;
                _SuperKnifeVfxInstance = Instantiate(config.VfxPrefab, parent);
                _SuperKnifeVfxInstance.name = $"{config.VfxPrefab.name}_KnifeVfx";
                _SuperKnifeVfxParticleSystems = _SuperKnifeVfxInstance.GetComponentsInChildren<ParticleSystem>(true);
                _SuperKnifeVfxRenderers = _SuperKnifeVfxInstance.GetComponentsInChildren<Renderer>(true);
            }

            Transform vfxTransform = _SuperKnifeVfxInstance.transform;
            vfxTransform.SetParent(VisualRoot != null ? VisualRoot : transform, false);
            vfxTransform.localPosition = config.VfxLocalOffset;
            vfxTransform.localRotation = Quaternion.identity;
            vfxTransform.localScale = config.VfxLocalScale;
            ApplySuperKnifeVfxSorting(config);
        }

        private void PlaySuperKnifeVfx(SuperKnifeConfig config)
        {
            if (_SuperKnifeVfxInstance == null)
                return;

            Transform vfxTransform = _SuperKnifeVfxInstance.transform;
            vfxTransform.localPosition = config.VfxLocalOffset;
            vfxTransform.localScale = config.VfxLocalScale;

            ApplySuperKnifeVfxSorting(config);
            _SuperKnifeVfxInstance.SetActive(true);

            if (_SuperKnifeVfxParticleSystems == null || _SuperKnifeVfxParticleSystems.Length == 0)
                return;

            for (int i = 0; i < _SuperKnifeVfxParticleSystems.Length; i++)
            {
                var system = _SuperKnifeVfxParticleSystems[i];
                if (system == null)
                    continue;

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(true);
            }
        }

        private void StopSuperKnifeVfx()
        {
            if (_SuperKnifeVfxInstance == null)
                return;

            if (_SuperKnifeVfxParticleSystems != null)
            {
                for (int i = 0; i < _SuperKnifeVfxParticleSystems.Length; i++)
                {
                    var system = _SuperKnifeVfxParticleSystems[i];
                    if (system == null)
                        continue;

                    system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            _SuperKnifeVfxInstance.SetActive(false);
        }

        private void ApplySuperKnifeVfxSorting(SuperKnifeConfig config)
        {
            if (_SpriteRenderer == null || _SuperKnifeVfxRenderers == null)
                return;

            int sortingLayerId = _SpriteRenderer.sortingLayerID;
            int sortingOrder = _SpriteRenderer.sortingOrder + config.VfxSortingOrderOffset;

            for (int i = 0; i < _SuperKnifeVfxRenderers.Length; i++)
            {
                var renderer = _SuperKnifeVfxRenderers[i];
                if (renderer == null)
                    continue;

                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = sortingOrder;
            }
        }

        private void OnDestroy()
        {
            if (_SuperKnifeVfxInstance != null)
            {
                Destroy(_SuperKnifeVfxInstance);
            }
        }

        private KnifeFailAnimationConfig GetFailAnimationConfig()
        {
            if (_FailAnimationConfig != null)
            {
                return _FailAnimationConfig;
            }

            _FailAnimationConfig = Resources.Load<KnifeFailAnimationConfig>(
                "Configurations/Systems/Animation/KnifeFailAnimationConfig");

            if (_FailAnimationConfig != null)
            {
                return _FailAnimationConfig;
            }

            _FailAnimationConfig = ScriptableObject.CreateInstance<KnifeFailAnimationConfig>();
            return _FailAnimationConfig;
        }
    }
}
