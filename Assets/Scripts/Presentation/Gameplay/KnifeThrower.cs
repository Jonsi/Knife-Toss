using System;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Represents the KnifeThrower class.
    /// </summary>
    public class KnifeThrower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject knifePrefab;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private MonoBehaviour throwInputSource;

        [Header("Throwing Settings")]
        [SerializeField] private float throwSpeed = 10f;

        [Header("Magazine Settings")]
        [SerializeField] private int poolWarmupCount = 10;
        [SerializeField] private int visibleQueueCount = 5;
        [SerializeField] private float queueSpacing = 0.5f;
        [SerializeField] private float slideAnimationDuration = 0.3f;
        [SerializeField] private float queueScaleEnd = 0.5f;
        [SerializeField] private float queueOpacityEnd = 0.3f;

        private IThrowInput throwInput;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int maxKnives;
        private int knivesThrown;
        private bool canThrow = true;
        private Transform knivesParent;

        private KnifePool knifePool;
        private KnifeMagazine magazine;
        private KnifeToss.Core.Abstractions.IKnifeTracker knifeTracker;
        private IObjectResolver objectResolver;
        private ITargetScaleProvider targetScaleProvider;

        public event Action KnifeThrown;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            launcherTransform ??= transform;
            throwInput = throwInputSource as IThrowInput ?? new NullThrowInput();
        }

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            throwInput.Performed += OnThrowInput;
            throwInput.Enable();
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            throwInput.Performed -= OnThrowInput;
            throwInput.Disable();
        }

        /// <summary>
        /// Performs the OnDestroy operation.
        /// </summary>
        private void OnDestroy()
        {
            KillAllKnifeTweens();
            magazine?.Reset();
            knifePool?.Clear();
        }

        /// <summary>
        /// Performs the KillAllKnifeTweens operation.
        /// </summary>
        public void KillAllKnifeTweens()
        {
            EnsureInitialized();
            KillTweensFromRegistry();
            magazine.KillTweens();
            knifePool.KillTweens();
            KillTweensOnChildren(transform);
            if (knivesParent != null)
            {
                KillTweensOnChildren(knivesParent);
            }

            if (currentKnife != null)
            {
                TweenKiller.Kill(currentKnife.gameObject);
            }
        }

        /// <summary>
        /// Performs the OnThrowInput operation.
        /// </summary>
        private void OnThrowInput()
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            ThrowKnife();
        }

        /// <summary>
        /// Performs the IsReadyToThrow operation.
        /// </summary>
        private bool IsReadyToThrow()
        {
            EnsureInitialized();

            if (!canThrow || isKnifeInFlight || knifePrefab == null || launcherTransform == null)
            {
                return false;
            }

            if (maxKnives > 0 && knivesThrown >= maxKnives)
            {
                return false;
            }

            return magazine.KnivesRemaining > 0;
        }

        /// <summary>
        /// Performs the ThrowKnife operation.
        /// </summary>
        private void ThrowKnife()
        {
            EnsureInitialized();

            if (!magazine.TryTakeNext(out GameObject knifeObject))
            {
                return;
            }

            isKnifeInFlight = true;

            PrepareKnifeForThrow(knifeObject);
            ActivateKnife(knifeObject);

            currentKnife = knifeObject.GetComponent<Knife>();
            if (currentKnife != null)
            {
                currentKnife.Initialize(this, throwSpeed);
            }

            knivesThrown++;
            KnifeThrown?.Invoke();
            magazine.SlideQueueForward();
        }

        /// <summary>
        /// Performs the PrepareKnifeForThrow operation.
        /// </summary>
        private void PrepareKnifeForThrow(GameObject knifeObject)
        {
            Transform parent = knivesParent != null ? knivesParent : null;
            Transform knifeTransform = knifeObject.transform;

            knifeTransform.SetParent(parent);
            KnifeSetupUtility.PositionAtLauncher(knifeObject, launcherTransform);
            KnifeSetupUtility.ApplyVisuals(knifeObject, ResolveKnifeScale(), knifePool.PrefabOriginalColor);
            KnifeSetupUtility.EnableColliders(knifeObject, true);
        }

        /// <summary>
        /// Performs the KillTweensOnChildren operation.
        /// </summary>
        private static void KillTweensOnChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null)
                {
                    TweenKiller.Kill(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Performs the KillTweensFromRegistry operation.
        /// </summary>
        private void KillTweensFromRegistry()
        {
            foreach (Knife knife in knifeTracker?.Snapshot() ?? Array.Empty<Knife>())
            {
                if (knife != null)
                {
                    TweenKiller.Kill(knife.gameObject);
                }
            }
        }

        /// <summary>
        /// Performs the OnKnifeResolved operation.
        /// </summary>
        public void OnKnifeResolved()
        {
            isKnifeInFlight = false;
            currentKnife = null;
        }

        /// <summary>
        /// Performs the ResetState operation.
        /// </summary>
        public void ResetState()
        {
            EnsureInitialized();
            KillAllKnifeTweens();
            magazine.Reset();
            isKnifeInFlight = false;
            knivesThrown = 0;
            canThrow = true;
        }

        /// <summary>
        /// Performs the SetMaxKnives operation.
        /// </summary>
        public void SetMaxKnives(int max)
        {
            maxKnives = max;
            knivesThrown = 0;
        }

        /// <summary>
        /// Performs the SetTotalKnives operation.
        /// </summary>
        public void SetTotalKnives(int total)
        {
            EnsureInitialized();
            magazine.Configure(total);
        }

        /// <summary>
        /// Performs the SetCanThrow operation.
        /// </summary>
        public void SetCanThrow(bool allow)
        {
            canThrow = allow;
        }

        /// <summary>
        /// Performs the SetKnivesParent operation.
        /// </summary>
        public void SetKnivesParent(Transform parent)
        {
            knivesParent = parent;
        }

        /// <summary>
        /// Performs the ActivateKnife operation.
        /// </summary>
        private static void ActivateKnife(GameObject knifeObject)
        {
            knifeObject.SetActive(true);
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [VContainer.Inject]
        public void Construct(KnifeToss.Core.Abstractions.IKnifeTracker tracker, IObjectResolver resolver, ITargetScaleProvider scaleProvider = null)
        {
            knifeTracker = tracker;
            objectResolver = resolver;
            targetScaleProvider = scaleProvider;
            InitializePoolAndMagazine();
            UpdateMagazineScale();
        }

        /// <summary>
        /// Performs the SetScaleProvider operation.
        /// </summary>
        public void SetScaleProvider(ITargetScaleProvider scaleProvider)
        {
            targetScaleProvider = scaleProvider;
            UpdateMagazineScale();
        }

        /// <summary>
        /// Performs the InitializePoolAndMagazine operation.
        /// </summary>
        private void InitializePoolAndMagazine()
        {
            if (knifePool != null && magazine != null)
            {
                return;
            }

            knifePool = new KnifePool(knifePrefab, transform, poolWarmupCount, objectResolver);
            magazine = new KnifeMagazine(
                knifePool,
                transform,
                launcherTransform,
                visibleQueueCount,
                queueSpacing,
                slideAnimationDuration,
                queueScaleEnd,
                queueOpacityEnd);
            UpdateMagazineScale();
        }

        /// <summary>
        /// Performs the EnsureInitialized operation.
        /// </summary>
        private void EnsureInitialized()
        {
            if (knifePool == null || magazine == null)
            {
                InitializePoolAndMagazine();
            }
        }

        /// <summary>
        /// Performs the ResolveKnifeScale operation.
        /// </summary>
        private Vector3 ResolveKnifeScale()
        {
            float scaleFactor = ResolveScaleFactor();
            return knifePool != null ? knifePool.PrefabOriginalScale * scaleFactor : Vector3.one * scaleFactor;
        }

        /// <summary>
        /// Performs the ResolveScaleFactor operation.
        /// </summary>
        private float ResolveScaleFactor()
        {
            return targetScaleProvider != null ? targetScaleProvider.PrimaryScale : 1f;
        }

        /// <summary>
        /// Performs the UpdateMagazineScale operation.
        /// </summary>
        private void UpdateMagazineScale()
        {
            if (magazine == null)
            {
                return;
            }

            magazine.SetScaleMultiplier(ResolveScaleFactor());
        }

        private sealed class NullThrowInput : IThrowInput
        {
            public event Action Performed
            {
                add { }
                remove { }
            }
            /// <summary>
            /// Performs the Enable operation.
            /// </summary>
            public void Enable() { }
            /// <summary>
            /// Performs the Disable operation.
            /// </summary>
            public void Disable() { }
        }

        [System.Serializable]
        public sealed class MagazineConfig
        {
            [SerializeField] private int poolWarmupCount = 10;
            [SerializeField] private int visibleQueueCount = 5;
            [SerializeField] private float queueSpacing = 0.5f;
            [SerializeField] private float slideAnimationDuration = 0.3f;
            [SerializeField] private float queueScaleEnd = 0.5f;
            [SerializeField] private float queueOpacityEnd = 0.3f;

            public int PoolWarmupCount => poolWarmupCount;
            public int VisibleQueueCount => visibleQueueCount;
            public float QueueSpacing => queueSpacing;
            public float SlideAnimationDuration => slideAnimationDuration;
            public float QueueScaleEnd => queueScaleEnd;
            public float QueueOpacityEnd => queueOpacityEnd;
        }
    }
}
