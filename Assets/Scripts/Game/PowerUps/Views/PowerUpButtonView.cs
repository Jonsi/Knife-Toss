using System;
using Common.Events;
using Game.Flow.Events;
using Game.PowerUps.Configs;
using Game.PowerUps.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.PowerUps.Views
{
    /// <summary>
    /// Displays a single power-up entry with icon and count.
    /// </summary>
    public class PowerUpButtonView : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        [SerializeField] private Image _Icon;
        [SerializeField] private TMP_Text _CountText;
        [SerializeField] private GameObject _CountRoot;
        [SerializeField] private GameObject _PlusRoot;
        [SerializeField] private Material _ActiveIconMaterial;
        [SerializeField] private Color _SelectedColor = Color.white;
        [SerializeField] private Color _UnselectedColor = Color.gray;
        [SerializeField] private Color _UnavailableColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _CooldownColor = new Color(0f, 0f, 0f, 0.6f);

        private PowerUpConfigBase _Config;
        private Action<PowerUpConfigBase> _ClickHandler;
        private Action<PowerUpConfigBase> _AddHandler;
        private int _CurrentCount;
        private bool _IsSelected;
        private bool _IsEffectActive;
        private bool _IsGameplayActive = true;
        private Image _CooldownFill;
        private Material _DefaultIconMaterial;
        private LevelStartedEventChannel _LevelStartedChannel;
        private VoidEventChannel _FailFlowStartedChannel;

        /// <summary>
        /// Initializes the button with the given configuration.
        /// </summary>
        public void Initialize(
            PowerUpConfigBase config,
            Action<PowerUpConfigBase> clickHandler,
            Action<PowerUpConfigBase> addHandler,
            LevelStartedEventChannel levelStartedChannel,
            VoidEventChannel failFlowStartedChannel)
        {
            _Config = config;
            _ClickHandler = clickHandler;
            _AddHandler = addHandler;
            _LevelStartedChannel = levelStartedChannel;
            _FailFlowStartedChannel = failFlowStartedChannel;
            if (_Icon != null)
            {
                _DefaultIconMaterial = _Icon.material;
                _Icon.sprite = config.Icon;
            }

            EnsureCooldownFill();

            UpdateSelected(false);
            SetCount(0);
            if (_Button != null)
            {
                _Button.onClick.AddListener(HandleClick);
            }

            _LevelStartedChannel?.Subscribe(OnLevelStarted);
            _FailFlowStartedChannel?.Subscribe(OnFailFlowStarted);

            UpdateVisualState();
        }

        /// <summary>
        /// Gets the configuration associated with this button.
        /// </summary>
        public PowerUpConfigBase Config => _Config;

        /// <summary>
        /// Sets the displayed count and interactable state.
        /// </summary>
        public void SetCount(int count)
        {
            _CurrentCount = count;
            if (_CountText != null)
            {
                _CountText.text = count.ToString();
            }

            UpdateAvailabilityState();
            UpdateVisualState();
        }

        /// <summary>
        /// Displays a clockwise radial overlay for an active timed effect.
        /// </summary>
        public void SetEffectProgress(bool isActive, float normalizedRemaining)
        {
            _IsEffectActive = isActive;
            if (_CooldownFill != null)
            {
                _CooldownFill.gameObject.SetActive(isActive);
                _CooldownFill.fillAmount = isActive
                    ? 1f - Mathf.Clamp01(normalizedRemaining)
                    : 0f;
            }

            UpdateAvailabilityState();
            UpdateVisualState();
        }

        /// <summary>
        /// Updates the selected marker state.
        /// </summary>
        public void UpdateSelected(bool isSelected)
        {
            _IsSelected = isSelected;
            UpdateVisualState();
        }

        private void HandleClick()
        {
            if (_IsEffectActive)
                return;

            if (_CurrentCount <= 0)
            {
                _AddHandler?.Invoke(_Config);
                return;
            }

            if (!_IsGameplayActive)
                return;

            _ClickHandler?.Invoke(_Config);
        }

        private void OnDestroy()
        {
            _LevelStartedChannel?.Unsubscribe(OnLevelStarted);
            _FailFlowStartedChannel?.Unsubscribe(OnFailFlowStarted);

            if (_Button != null)
            {
                _Button.onClick.RemoveListener(HandleClick);
            }
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            SetGameplayActive(true);
        }

        private void OnFailFlowStarted()
        {
            SetGameplayActive(false);
        }

        private void SetGameplayActive(bool isGameplayActive)
        {
            _IsGameplayActive = isGameplayActive;
        }

        private void EnsureCooldownFill()
        {
            if (_CooldownFill != null || _Config is not IWorldEffectPowerUp || _Icon == null)
                return;

            var cooldownObject = new GameObject(
                "CooldownFill",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            cooldownObject.layer = gameObject.layer;

            var rectTransform = cooldownObject.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.SetAsFirstSibling();

            _CooldownFill = cooldownObject.GetComponent<Image>();
            _CooldownFill.sprite = _Icon.sprite;
            _CooldownFill.color = _CooldownColor;
            _CooldownFill.raycastTarget = false;
            _CooldownFill.type = Image.Type.Filled;
            _CooldownFill.fillMethod = Image.FillMethod.Radial360;
            _CooldownFill.fillOrigin = (int)Image.Origin360.Top;
            _CooldownFill.fillClockwise = true;
            _CooldownFill.fillAmount = 0f;
            _CooldownFill.gameObject.SetActive(false);
        }

        private void UpdateAvailabilityState()
        {
            if (_CountRoot != null)
            {
                _CountRoot.SetActive(_CurrentCount > 0 || _IsEffectActive);
            }

            if (_PlusRoot != null)
            {
                _PlusRoot.SetActive(_CurrentCount <= 0 && !_IsEffectActive);
            }

            if (_Button != null)
            {
                _Button.interactable = !_IsEffectActive &&
                    (_CurrentCount > 0 || _AddHandler != null);
            }
        }

        private void UpdateVisualState()
        {
            var targetGraphic = _Button != null ? _Button.targetGraphic : null;
            if (targetGraphic == null)
                return;

            bool isActive = _IsSelected || _IsEffectActive;
            bool isUnavailable = _CurrentCount <= 0 && !_IsEffectActive;

            if (_Icon != null)
            {
                _Icon.material = isActive && !isUnavailable && _ActiveIconMaterial != null
                    ? _ActiveIconMaterial
                    : _DefaultIconMaterial;
            }

            if (isUnavailable)
            {
                targetGraphic.color = _UnavailableColor;
                return;
            }

            targetGraphic.color = isActive ? _SelectedColor : _UnselectedColor;
        }
    }
}
