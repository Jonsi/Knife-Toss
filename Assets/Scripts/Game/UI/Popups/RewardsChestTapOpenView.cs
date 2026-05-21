using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Handles chest tap input, DOTween punch feedback, and switching to the opened visual state.
    /// </summary>
    public class RewardsChestTapOpenView : MonoBehaviour
    {
        private static readonly Vector3 DefaultTapPunchScale = new Vector3(0.12f, 0.12f, 0f);
        private static readonly Vector3 DefaultSpawnedOpenEffectLocalScale = new Vector3(0.45f, 0.45f, 0.45f);

        [SerializeField]
        private Image _ChestImage;

        [SerializeField]
        private Sprite _ClosedChestSprite;

        [SerializeField]
        private Sprite _OpenedChestSprite;

        [SerializeField]
        private RectTransform _ChestRectTransform;

        [SerializeField]
        private Button _TapChestButton;

        [SerializeField]
        private Transform _EmbeddedOpenEffectRoot;

        [SerializeField]
        private Transform _EmbeddedBackgroundEffectRoot;

        [SerializeField]
        private GameObject _OpenEffectPrefab;

        [SerializeField]
        private Vector3 _SpawnedOpenEffectLocalScale = DefaultSpawnedOpenEffectLocalScale;

        [SerializeField]
        private string _SpawnedParticleSortingLayerName = "UI";

        [SerializeField]
        private int _SpawnedParticleSortingOrder = 200;

        [SerializeField]
        [Min(1)]
        private int _TapsToOpen = 3;

        [SerializeField]
        private Vector3 _TapPunchScale = DefaultTapPunchScale;

        [SerializeField]
        [Min(0f)]
        private float _TapPunchDurationSeconds = 0.18f;

        [SerializeField]
        [Min(1)]
        private int _TapPunchVibrato = 6;

        [SerializeField]
        [Range(0f, 1f)]
        private float _TapPunchElasticity = 0.4f;

        [SerializeField]
        private bool _UseUnscaledTime = true;

        private int _TapCount;
        private bool _IsOpened;
        private Transform _OpenEffectSpawnParent;
        private GameObject _SpawnedOpenEffectInstance;

        /// <summary>
        /// Raised once after the last required tap, when open visuals are applied.
        /// </summary>
        public event Action Opened;

        /// <summary>
        /// Optional world-space parent for spawned VFX (same pattern as main menu background effects).
        /// </summary>
        /// <param name="spawnParent">When null, the effect is spawned with no parent at the chest world position.</param>
        public void SetOpenEffectSpawnParent(Transform spawnParent)
        {
            _OpenEffectSpawnParent = spawnParent;
        }

        /// <summary>
        /// Updates chest sprites from code; non-null arguments override serialized defaults.
        /// </summary>
        public void ConfigureSprites(Sprite closedChest, Sprite openedChest)
        {
            if (closedChest != null)
            {
                _ClosedChestSprite = closedChest;
            }

            if (openedChest != null)
            {
                _OpenedChestSprite = openedChest;
            }

            ApplyClosedVisual();
        }

        /// <summary>
        /// Resets taps and closed visuals so the chest can be opened again.
        /// </summary>
        public void ResetState()
        {
            _TapCount = 0;
            _IsOpened = false;

            DespawnSpawnedOpenEffect();
            HideEmbeddedOpenEffect();
            HideEmbeddedBackgroundEffect();

            if (_TapChestButton != null)
            {
                _TapChestButton.interactable = true;
            }

            ApplyClosedVisual();
        }

        private void Awake()
        {
            if (_TapChestButton != null)
            {
                _TapChestButton.onClick.AddListener(OnChestTapped);
            }

            HideEmbeddedOpenEffect();
            HideEmbeddedBackgroundEffect();
        }

        private void Start()
        {
            ApplyClosedVisual();
        }

        private void OnDestroy()
        {
            if (_TapChestButton != null)
            {
                _TapChestButton.onClick.RemoveListener(OnChestTapped);
            }

            if (_ChestRectTransform != null)
            {
                DOTween.Kill(_ChestRectTransform);
            }

            DespawnSpawnedOpenEffect();
        }

        private void OnChestTapped()
        {
            if (_IsOpened)
            {
                return;
            }

            PlayTapPunch();
            _TapCount++;

            if (_TapCount < _TapsToOpen)
            {
                return;
            }

            OpenChest();
        }

        private void PlayTapPunch()
        {
            if (_ChestRectTransform == null)
            {
                return;
            }

            DOTween.Kill(_ChestRectTransform);
            _ChestRectTransform
                .DOPunchScale(_TapPunchScale, _TapPunchDurationSeconds, _TapPunchVibrato, _TapPunchElasticity)
                .SetUpdate(_UseUnscaledTime)
                .SetTarget(_ChestRectTransform);
        }

        private void OpenChest()
        {
            _IsOpened = true;

            if (_TapChestButton != null)
            {
                _TapChestButton.interactable = false;
            }

            if (_ChestImage != null && _OpenedChestSprite != null)
            {
                _ChestImage.sprite = _OpenedChestSprite;
            }

            SpawnOpenEffectWorldSpace();
            PlayEmbeddedBackgroundEffect();

            Opened?.Invoke();
        }

        private void HideEmbeddedOpenEffect()
        {
            if (_EmbeddedOpenEffectRoot != null)
            {
                _EmbeddedOpenEffectRoot.gameObject.SetActive(false);
            }
        }

        private void HideEmbeddedBackgroundEffect()
        {
            if (_EmbeddedBackgroundEffectRoot != null)
            {
                _EmbeddedBackgroundEffectRoot.gameObject.SetActive(false);
            }
        }

        private void PlayEmbeddedBackgroundEffect()
        {
            if (_EmbeddedBackgroundEffectRoot == null)
            {
                return;
            }

            _EmbeddedBackgroundEffectRoot.gameObject.SetActive(true);
            ConfigureAndRestartParticles(_EmbeddedBackgroundEffectRoot.gameObject);
        }

        private void SpawnOpenEffectWorldSpace()
        {
            DespawnSpawnedOpenEffect();

            if (_OpenEffectPrefab == null || _ChestRectTransform == null)
            {
                return;
            }

            var position = _ChestRectTransform.position;
            _SpawnedOpenEffectInstance = Instantiate(_OpenEffectPrefab, position, Quaternion.identity, _OpenEffectSpawnParent);
            ConfigureSpawnedOpenEffectForScreenSpaceUi();
        }

        private void ConfigureSpawnedOpenEffectForScreenSpaceUi()
        {
            if (_SpawnedOpenEffectInstance == null)
            {
                return;
            }

            _SpawnedOpenEffectInstance.transform.localScale = _SpawnedOpenEffectLocalScale;

            var layerId = SortingLayer.NameToID(_SpawnedParticleSortingLayerName);
            if (layerId < 0)
            {
                layerId = SortingLayer.NameToID("Default");
            }

            var renderers = _SpawnedOpenEffectInstance.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null)
                {
                    continue;
                }

                r.sortingLayerID = layerId;
                r.sortingOrder = _SpawnedParticleSortingOrder;
            }

            ConfigureAndRestartParticles(_SpawnedOpenEffectInstance);
        }

        private void ConfigureAndRestartParticles(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var systems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (var i = 0; i < systems.Length; i++)
            {
                var particleSystem = systems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                var main = particleSystem.main;
                main.useUnscaledTime = _UseUnscaledTime;

                particleSystem.Clear(true);
                particleSystem.Play(true);
            }
        }

        private void DespawnSpawnedOpenEffect()
        {
            if (_SpawnedOpenEffectInstance == null)
            {
                return;
            }

            Destroy(_SpawnedOpenEffectInstance);
            _SpawnedOpenEffectInstance = null;
        }

        private void ApplyClosedVisual()
        {
            if (_ChestImage != null && _ClosedChestSprite != null)
            {
                _ChestImage.sprite = _ClosedChestSprite;
            }
        }
    }
}
