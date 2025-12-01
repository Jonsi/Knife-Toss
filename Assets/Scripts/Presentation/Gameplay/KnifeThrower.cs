using System;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{
    public class KnifeThrower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject knifePrefab;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("Throwing Settings")]
        [SerializeField] private float throwSpeed = 10f;

        [Header("Magazine Settings")]
        [SerializeField] private int poolWarmupCount = 10;
        [SerializeField] private int visibleQueueCount = 5;
        [SerializeField] private float queueSpacing = 0.5f;
        [SerializeField] private float slideAnimationDuration = 0.3f;
        [SerializeField] private float queueScaleEnd = 0.5f;
        [SerializeField] private float queueOpacityEnd = 0.3f;

        private InputAction attackAction;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int maxKnives;
        private int knivesThrown;
        private bool canThrow = true;
        private Transform knivesParent;

        private KnifePool knifePool;
        private KnifeMagazine magazine;

        public event Action KnifeThrown;

        private void Awake()
        {
            attackAction = inputActionAsset != null ? inputActionAsset.FindAction("Player/Attack") : null;
            launcherTransform ??= transform;

            knifePool = new KnifePool(knifePrefab, transform, poolWarmupCount);
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

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.Enable();
                attackAction.performed += OnThrowInput;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnThrowInput;
                attackAction.Disable();
            }
        }

        private void OnDestroy()
        {
            KillAllKnifeTweens();
            magazine.Reset();
            knifePool.Clear();
        }

        public void KillAllKnifeTweens()
        {
            KillTweensOnKnivesInScene();
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

        private void OnThrowInput(InputAction.CallbackContext _)
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            ThrowKnife();
        }

        private bool IsReadyToThrow()
        {
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
            knifeTransform.position = launcherTransform.position;
            knifeTransform.rotation = launcherTransform.rotation;
            knifeTransform.localScale = knifePool.PrefabOriginalScale;

            SpriteRenderer renderer = knifeObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = knifePool.PrefabOriginalColor;
            }

            EnableColliders(knifeObject);
        }

        private static void EnableColliders(GameObject knifeObject)
        {
            Collider2D[] colliders = knifeObject.GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
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

        private static void KillTweensOnKnivesInScene()
        {
            Knife[] allKnives = FindObjectsByType<Knife>(FindObjectsSortMode.None);
            foreach (Knife knife in allKnives)
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
    }
}

