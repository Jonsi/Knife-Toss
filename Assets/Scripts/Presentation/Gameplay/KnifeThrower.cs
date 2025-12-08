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

        [Header("Knife Load Animation")]
        [SerializeField] private float loadBottomOffset = 0.5f;
        [SerializeField] private float loadAnimationDuration = 0.3f;
        [SerializeField] private Ease loadAnimationEase = Ease.OutQuad;

        [Header("Audio")]
        [SerializeField] private float throwSoundVolume = 1f;

        private IThrowInput throwInput;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int knivesThrown;
        private bool canThrow = true;
        private Transform knivesParent;

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

        private void OnThrowPressed(Vector2 screenPosition)
        {
        }

        private void OnThrowReleased(Vector2 screenPosition)
        {
            if (!IsReadyToThrow())
            {
                return;
            }

            ThrowKnife(Vector2.up);
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
        private void ThrowKnife(Vector2 direction)
        {
            EnsureInitialized();

            if (!magazine.TryTakeNext(out GameObject knifeObject, out Knife knife) || knifeObject == null || knife == null)
            {
                return;
            }

            isKnifeInFlight = true;

            PrepareKnifeForThrow(knifeObject);
            ActivateKnife(knifeObject);
            LaunchKnife(knife, direction);
            PlayThrowSound();
        }

        private void LaunchKnife(Knife knife, Vector2 direction)
        {
            if (knife == null)
            {
                return;
            }

            currentKnife = knife;
            currentKnife.Initialize(this, throwSpeed, direction);

            knivesThrown++;
            KnifeThrown?.Invoke();
            magazine.LoadNextKnife();
        }

        private void PlayThrowSound()
        {
            PlaySound(soundSettings != null ? soundSettings.KnifeThrowSound : null, throwSoundVolume);
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
        private void PrepareKnifeForThrow(GameObject knifeObject)
        {
            Transform parent = knivesParent != null ? knivesParent : null;
            Transform knifeTransform = knifeObject.transform;

            knifeTransform.SetParent(parent);
            if (launcherTransform != null)
            {
                knifeTransform.position = launcherTransform.position;
                knifeTransform.rotation = Quaternion.identity;
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
                loadBottomOffset,
                loadAnimationDuration,
                loadAnimationEase);
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

    }
}
