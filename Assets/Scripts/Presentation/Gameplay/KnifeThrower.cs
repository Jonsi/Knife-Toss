using System;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Abstractions.Factories;
using KnifeToss.Infrastructure.Services;
using KnifeToss.Presentation.Gameplay.Projectiles;
using KnifeToss.Presentation.Gameplay.Throwing;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Simplified facade for knife throwing. Coordinates input, aiming, launching, and magazine.
    /// </summary>
    [RequireComponent(typeof(ThrowingInput))]
    [RequireComponent(typeof(AimCalculator))]
    [RequireComponent(typeof(KnifeLauncher))]
    public sealed class KnifeThrower : MonoBehaviour
    {
        [SerializeField] private ThrowingInput _input;
        [SerializeField] private AimCalculator _aim;
        [SerializeField] private KnifeLauncher _launcher;
        
        [Header("Magazine Settings")]
        [SerializeField] private int visibleQueueCount = 3;
        [SerializeField] private float queueSpacing = 0.2f;
        [SerializeField] private float slideAnimationDuration = 0.3f;
        [SerializeField] private float queueScaleEnd = 0.5f;
        [SerializeField] private float queueOpacityEnd = 0.3f;

        private KnifeMagazine _magazine;
        private IKnifeTracker _knifeTracker;
        private IKnifeFactory _knifeFactory;
        private ITargetScaleProvider _scaleProvider;
        private RuntimeCacheService _cacheService;
        private GameObject _knifePrefab;
        private IObjectResolver _resolver;
        private Transform _knivesParent;
        private Knife _currentKnife;
        private bool _isKnifeInFlight;
        private bool _canThrow = true;
        private int _knivesThrown;

        public event Action KnifeThrown;
        public event Action KnifeResolved;

        private void Awake()
        {
            _input ??= GetComponent<ThrowingInput>();
            _aim ??= GetComponent<AimCalculator>();
            _launcher ??= GetComponent<KnifeLauncher>();
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.ThrowPressed += HandleThrowPressed;
                _input.ThrowReleased += HandleThrowReleased;
                _input.AimUpdated += HandleAimUpdated;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.ThrowPressed -= HandleThrowPressed;
                _input.ThrowReleased -= HandleThrowReleased;
                _input.AimUpdated -= HandleAimUpdated;
            }
        }

        [VContainer.Inject]
        public void Construct(
            IKnifeTracker tracker,
            IKnifeFactory knifeFactory,
            ITargetScaleProvider scaleProvider,
            RuntimeCacheService cacheService,
            GameObject knifePrefab,
            IObjectResolver resolver)
        {
            _knifeTracker = tracker;
            _knifeFactory = knifeFactory;
            _scaleProvider = scaleProvider;
            _cacheService = cacheService;
            _knifePrefab = knifePrefab;
            _resolver = resolver;

            _aim?.Initialize(_cacheService);
        }

        public void Initialize(
            Transform knivesParent,
            Vector3 prefabScale,
            Color prefabColor,
            int totalKnives)
        {
            _knivesParent = knivesParent;
            
            if (_launcher != null)
            {
                _launcher.Initialize(transform, knivesParent, _knifeFactory, _scaleProvider, prefabScale, prefabColor);
            }

            // Create KnifeMagazine if not already created
            if (_magazine == null && _knifePrefab != null && _resolver != null && knivesParent != null)
            {
                // Create a queue parent for the magazine
                GameObject queueParentObj = new GameObject("KnifeQueue");
                queueParentObj.transform.SetParent(knivesParent);
                queueParentObj.transform.localPosition = Vector3.zero;
                Transform queueParent = queueParentObj.transform;

                // Create KnifePool
                KnifePool pool = new KnifePool(_knifePrefab, knivesParent, totalKnives, _resolver);

                // Create KnifeMagazine
                _magazine = new KnifeMagazine(
                    pool,
                    queueParent,
                    transform,
                    visibleQueueCount,
                    queueSpacing,
                    slideAnimationDuration,
                    queueScaleEnd,
                    queueOpacityEnd);
            }

            if (_magazine != null)
            {
                _magazine.Configure(totalKnives);
            }

            _canThrow = true;
            _knivesThrown = 0;
        }

        public void SetCanThrow(bool allow)
        {
            _canThrow = allow;
        }

        public void SetTotalKnives(int total)
        {
            _magazine?.Configure(total);
            _knivesThrown = 0;
        }

        public void SetKnivesParent(Transform parent)
        {
            _launcher?.SetKnivesParent(parent);
        }

        public void SetScaleProvider(ITargetScaleProvider scaleProvider)
        {
            _scaleProvider = scaleProvider;
            _launcher?.SetScaleProvider(scaleProvider);
        }

        public void KillAllKnifeTweens()
        {
            _magazine?.KillTweens();
        }

        public void ResetState()
        {
            _isKnifeInFlight = false;
            _knivesThrown = 0;
            _canThrow = true;
            _magazine?.Reset();
        }

        public void ReturnToPool(Knife knife)
        {
            if (knife == null || _knifeFactory == null)
            {
                return;
            }

            _knifeFactory.Recycle(knife);
        }

        public void OnKnifeResolved()
        {
            _isKnifeInFlight = false;
            _currentKnife = null;
            KnifeResolved?.Invoke();
        }

        private void HandleThrowPressed(Vector2 screenPosition)
        {
            Vector2 direction = _aim.CalculateDirection(screenPosition);
            _aim.UpdateAim(direction);
            _magazine?.AlignFirstKnife(direction);
        }

        private void HandleThrowReleased(Vector2 screenPosition)
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            Vector2 direction = _aim.CalculateDirection(screenPosition);
            ThrowKnife(direction);
        }

        private void HandleAimUpdated(Vector2 screenPosition)
        {
            Vector2 direction = _aim.CalculateDirection(screenPosition);
            _magazine?.AlignFirstKnife(direction);
        }

        private bool IsReadyToThrow()
        {
            if (!_canThrow || _isKnifeInFlight || _magazine == null)
            {
                return false;
            }

            return _magazine.KnivesRemaining > 0;
        }

        private void ThrowKnife(Vector2 direction)
        {
            if (_magazine == null || !_magazine.TryTakeNext(out GameObject knifeObject))
            {
                return;
            }

            _isKnifeInFlight = true;
            _currentKnife = knifeObject.GetComponent<Knife>();
            if (_currentKnife == null)
            {
                _currentKnife = _launcher.Launch(direction);
            }

            _knivesThrown++;
            KnifeThrown?.Invoke();
            _magazine.SlideQueueForward();
        }
    }
}

