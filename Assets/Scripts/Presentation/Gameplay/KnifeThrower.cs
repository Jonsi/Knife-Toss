using System;
using System.Collections.Generic;
using Core.Abstractions;
using Core.Data;
using DG.Tweening;
using Presentation.Gameplay.Input;
using Presentation.Gameplay.Projectiles;
using Presentation.Gameplay.Utilities;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay
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

        [Header("Load Animation")]
        [SerializeField] private float loadPullbackDistance = 0.15f;
        [SerializeField] private float loadAnimationDuration = 0.12f;
        [SerializeField] private Ease loadAnimationEase = Ease.OutQuad;
        [SerializeField] private float loadSoundVolume = 1f;

        [Header("Throw Animation")]
        [SerializeField] private float throwPushDistance = 0.25f;
        [SerializeField] private float throwAnimationDuration = 0.08f;
        [SerializeField] private Ease throwAnimationEase = Ease.OutQuad;
        [SerializeField] private float throwSoundVolume = 1f;

        private IThrowInput throwInput;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int knivesThrown;
        private bool canThrow = true;
        private bool isAiming;
        private Transform knivesParent;
        private Tween loadTween;
        private GameObject loadingKnife;
        private Vector3 loadingKnifeStartPosition;
        private bool hasLoadingKnifeStart;
        private Sequence throwSequence;
        private AudioSource loadAudioSource;

        private KnifePool knifePool;
        private KnifeMagazine magazine;
        private IKnifeTracker knifeTracker;
        private IObjectResolver objectResolver;
        private ITargetScaleProvider targetScaleProvider;
        private IAudioPlayer audioPlayer;
        private SoundSettings soundSettings;

        public event Action KnifeThrown;
        public event Action KnifeResolved;

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
            throwInput.Pressed += OnThrowPressed;
            throwInput.Released += OnThrowReleased;
            throwInput.Enable();
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            throwInput.Pressed -= OnThrowPressed;
            throwInput.Released -= OnThrowReleased;
            throwInput.Disable();
        }

        private void Update()
        {
            if (!isAiming)
            {
                return;
            }

            Vector2 aimDirection = ResolveAimDirection(throwInput.CurrentScreenPosition);
            ApplyAimDirection(aimDirection);
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
            KillLoadTween();
            ClearLoadAnimationState();
            StopLoadSound();
            KillThrowSequence();
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

        private void OnThrowPressed(Vector2 screenPosition)
        {
            isAiming = true;
            Vector2 aimDirection = ResolveAimDirection(screenPosition);
            ApplyAimDirection(aimDirection);
            PlayLoadAnimation();
        }

        private void OnThrowReleased(Vector2 screenPosition)
        {
            isAiming = false;
            StopLoadSound();
            Vector2 aimDirection = ResolveAimDirection(screenPosition);
            ApplyAimDirection(aimDirection);

            if (!IsReadyToThrow())
            {
                ResetLoadAnimation();
                return;
            }

            CompleteLoadAnimation();
            ThrowKnife(aimDirection);
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

            return magazine.KnivesRemaining > 0;
        }

        /// <summary>
        /// Performs the ThrowKnife operation.
        /// </summary>
        private void ThrowKnife(Vector2 aimDirection)
        {
            EnsureInitialized();
            KillLoadTween();
            KillThrowSequence();
            StopLoadSound();

            if (!magazine.TryTakeNext(out GameObject knifeObject))
            {
                ClearLoadAnimationState();
                return;
            }

            bool shouldKeepPosition = knifeObject == loadingKnife;
            ClearLoadAnimationState();

            isKnifeInFlight = true;

            PrepareKnifeForThrow(knifeObject, shouldKeepPosition);
            ActivateKnife(knifeObject);

            ApplyAimDirection(aimDirection);

            PlayThrowAnimation(knifeObject, () =>
            {
                LaunchKnife(knifeObject, aimDirection);
                PlayThrowSound();
            });
        }

        private void LaunchKnife(GameObject knifeObject, Vector2 aimDirection)
        {
            currentKnife = knifeObject != null ? knifeObject.GetComponent<Knife>() : null;
            if (currentKnife != null)
            {
                currentKnife.Initialize(this, throwSpeed, aimDirection);
            }

            knivesThrown++;
            KnifeThrown?.Invoke();
            magazine.SlideQueueForward();
        }

        private void ApplyAimDirection(Vector2 direction)
        {
            if (launcherTransform == null)
            {
                return;
            }

            Vector2 resolved = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector2.up;
            AlignQueuedHead(resolved);
        }

        private Vector2 ResolveAimDirection(Vector2 screenPosition)
        {
            if (launcherTransform == null)
            {
                return Vector2.up;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return launcherTransform.up;
            }

            float zDistance = Mathf.Abs(launcherTransform.position.z - cam.transform.position.z);
            zDistance = Mathf.Approximately(zDistance, 0f) ? 1f : zDistance;
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistance));
            worldPoint.z = launcherTransform.position.z;

            Vector2 direction = (Vector2)(worldPoint - launcherTransform.position);
            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                return launcherTransform.up;
            }

            return direction.normalized;
        }

        private void AlignQueuedHead(Vector2 direction)
        {
            if (magazine == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            magazine.AlignFirstKnife(direction.normalized);
        }

        private void PlayLoadAnimation()
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            GameObject knifeObject = ResolveQueuedKnifeHead();
            if (knifeObject == null)
            {
                return;
            }

            loadingKnife = knifeObject;
            hasLoadingKnifeStart = true;
            loadingKnifeStartPosition = knifeObject.transform.position;

            KillLoadTween();

            Vector3 backDirection = launcherTransform != null ? -launcherTransform.up : Vector3.down;
            Vector3 targetPosition = loadingKnifeStartPosition + backDirection * loadPullbackDistance;

            loadTween = knifeObject.transform.DOMove(targetPosition, loadAnimationDuration)
                .SetEase(loadAnimationEase);
            PlayLoadSound();
        }

        private void ResetLoadAnimation()
        {
            if (loadingKnife == null || !hasLoadingKnifeStart)
            {
                KillLoadTween();
                ClearLoadAnimationState();
                return;
            }

            KillLoadTween();
            loadTween = loadingKnife.transform.DOMove(loadingKnifeStartPosition, loadAnimationDuration)
                .SetEase(loadAnimationEase);
            ClearLoadAnimationState();
        }

        private void PlayThrowAnimation(GameObject knifeObject, Action onComplete)
        {
            if (knifeObject == null)
            {
                onComplete?.Invoke();
                return;
            }

            KillThrowSequence();

            Vector3 forward = launcherTransform != null ? launcherTransform.up : Vector3.up;
            Vector3 targetPosition = knifeObject.transform.position + forward * throwPushDistance;
            float duration = Mathf.Max(0f, throwAnimationDuration);

            if (duration <= Mathf.Epsilon)
            {
                knifeObject.transform.position = targetPosition;
                onComplete?.Invoke();
                return;
            }

            throwSequence = DOTween.Sequence().SetTarget(knifeObject);
            throwSequence.Append(knifeObject.transform.DOMove(targetPosition, duration).SetEase(throwAnimationEase));
            throwSequence.OnComplete(() => onComplete?.Invoke());
            throwSequence.OnKill(() => throwSequence = null);
        }

        private void CompleteLoadAnimation()
        {
            KillLoadTween(true);
            ClearLoadAnimationState();
        }

        private void KillLoadTween(bool complete = false)
        {
            if (loadTween == null)
            {
                return;
            }

            loadTween.Kill(complete);
            loadTween = null;
        }

        private void KillThrowSequence()
        {
            if (throwSequence == null)
            {
                return;
            }

            throwSequence.Kill();
            throwSequence = null;
        }

        private void ClearLoadAnimationState()
        {
            loadingKnife = null;
            hasLoadingKnifeStart = false;
            loadingKnifeStartPosition = Vector3.zero;
        }

        private GameObject ResolveQueuedKnifeHead()
        {
            IReadOnlyList<GameObject> queuedKnives = magazine?.QueuedKnives;
            if (queuedKnives == null || queuedKnives.Count == 0)
            {
                return null;
            }

            return queuedKnives[0];
        }

        private void PlayLoadSound()
        {
            AudioClip clip = soundSettings != null ? soundSettings.KnifeLoadSound : null;
            if (clip == null)
            {
                return;
            }

            if (loadAudioSource == null)
            {
                loadAudioSource = gameObject.AddComponent<AudioSource>();
                loadAudioSource.playOnAwake = false;
                loadAudioSource.loop = false;
                loadAudioSource.spatialBlend = 0f;
            }

            loadAudioSource.clip = clip;
            loadAudioSource.volume = Mathf.Max(0f, loadSoundVolume);
            loadAudioSource.Stop();
            loadAudioSource.Play();
        }

        private void PlayThrowSound()
        {
            PlaySound(soundSettings != null ? soundSettings.KnifeThrowSound : null, throwSoundVolume);
        }

        private void StopLoadSound()
        {
            if (loadAudioSource != null && loadAudioSource.isPlaying)
            {
                loadAudioSource.Stop();
            }
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            IAudioPlayer player = audioPlayer ?? NullAudioPlayer.Instance;
            player.Play2D(clip, Mathf.Max(0f, volume));
        }

        /// <summary>
        /// Performs the PrepareKnifeForThrow operation.
        /// </summary>
        private void PrepareKnifeForThrow(GameObject knifeObject, bool keepPosition)
        {
            Transform parent = knivesParent != null ? knivesParent : null;
            Transform knifeTransform = knifeObject.transform;

            knifeTransform.SetParent(parent);
            if (launcherTransform != null)
            {
                if (!keepPosition)
                {
                    knifeTransform.position = launcherTransform.position;
                }

                knifeTransform.rotation = launcherTransform.rotation;
            }
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
            KnifeResolved?.Invoke();
        }

        /// <summary>
        /// Returns a knife to the pool.
        /// </summary>
        /// <param name="knife">Knife to return.</param>
        public void ReturnToPool(Knife knife)
        {
            if (knife == null)
            {
                return;
            }

            EnsureInitialized();
            knifePool?.Return(knife.gameObject);
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
        /// Performs the SetTotalKnives operation.
        /// </summary>
        public void SetTotalKnives(int total)
        {
            EnsureInitialized();
            magazine.Configure(total);
            knivesThrown = 0;
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
        public void Construct(
            IKnifeTracker tracker,
            IObjectResolver resolver,
            SoundSettings soundSettings = null,
            IAudioPlayer audioPlayer = null,
            ITargetScaleProvider scaleProvider = null)
        {
            knifeTracker = tracker;
            objectResolver = resolver;
            targetScaleProvider = scaleProvider;
            this.soundSettings = soundSettings;
            this.audioPlayer = audioPlayer;
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
            public event Action<Vector2> Pressed
            {
                add { }
                remove { }
            }
            public event Action<Vector2> Released
            {
                add { }
                remove { }
            }
            public Vector2 CurrentScreenPosition => Vector2.zero;
            /// <summary>
            /// Performs the Enable operation.
            /// </summary>
            public void Enable() { }
            /// <summary>
            /// Performs the Disable operation.
            /// </summary>
            public void Disable() { }
        }

        private sealed class NullAudioPlayer : IAudioPlayer
        {
            public static readonly IAudioPlayer Instance = new NullAudioPlayer();
            public void Play(AudioClip clip, Vector3 position, float volume) { }
            public void Play2D(AudioClip clip) { }
            public void Play2D(AudioClip clip, float volume) { }
            public void SetMuted(bool isMuted) { }
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
