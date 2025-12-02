using System;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay
{
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

        public event Action KnifeThrown;

        private void Awake()
        {
            launcherTransform ??= transform;
            throwInput = throwInputSource as IThrowInput ?? new NullThrowInput();
        }

        private void OnEnable()
        {
            throwInput.Performed += OnThrowInput;
            throwInput.Enable();
        }

        private void OnDisable()
        {
            throwInput.Performed -= OnThrowInput;
            throwInput.Disable();
        }

        private void OnDestroy()
        {
            KillAllKnifeTweens();
            magazine?.Reset();
            knifePool?.Clear();
        }

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

        private void OnThrowInput()
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            ThrowKnife();
        }

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

        private void PrepareKnifeForThrow(GameObject knifeObject)
        {
            Transform parent = knivesParent != null ? knivesParent : null;
            Transform knifeTransform = knifeObject.transform;

            knifeTransform.SetParent(parent);
            KnifeSetupUtility.PositionAtLauncher(knifeObject, launcherTransform);
            KnifeSetupUtility.ApplyVisuals(knifeObject, knifePool.PrefabOriginalScale, knifePool.PrefabOriginalColor);
            KnifeSetupUtility.EnableColliders(knifeObject, true);
        }

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

        public void OnKnifeResolved()
        {
            isKnifeInFlight = false;
            currentKnife = null;
        }

        public void ResetState()
        {
            EnsureInitialized();
            KillAllKnifeTweens();
            magazine.Reset();
            isKnifeInFlight = false;
            knivesThrown = 0;
            canThrow = true;
        }

        public void SetMaxKnives(int max)
        {
            maxKnives = max;
            knivesThrown = 0;
        }

        public void SetTotalKnives(int total)
        {
            EnsureInitialized();
            magazine.Configure(total);
        }

        public void SetCanThrow(bool allow)
        {
            canThrow = allow;
        }

        public void SetKnivesParent(Transform parent)
        {
            knivesParent = parent;
        }

        private static void ActivateKnife(GameObject knifeObject)
        {
            knifeObject.SetActive(true);
        }

        [VContainer.Inject]
        public void Construct(KnifeToss.Core.Abstractions.IKnifeTracker tracker, IObjectResolver resolver)
        {
            knifeTracker = tracker;
            objectResolver = resolver;
            InitializePoolAndMagazine();
        }

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
        }

        private void EnsureInitialized()
        {
            if (knifePool == null || magazine == null)
            {
                InitializePoolAndMagazine();
            }
        }

        private sealed class NullThrowInput : IThrowInput
        {
            public event Action Performed
            {
                add { }
                remove { }
            }
            public void Enable() { }
            public void Disable() { }
        }
    }
}

