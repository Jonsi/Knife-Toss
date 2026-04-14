using System;
using Common.Events;
using Game.Flow.Events;
using Game.PowerUps.Configs;
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
        [SerializeField] private Color _SelectedColor = Color.white;
        [SerializeField] private Color _UnselectedColor = Color.gray;
        [SerializeField] private Color _UnavailableColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        private PowerUpConfigBase _Config;
        private Action<PowerUpConfigBase> _ClickHandler;
        private Action<PowerUpConfigBase> _AddHandler;
        private int _CurrentCount;
        private bool _IsSelected;
        private bool _IsGameplayActive = true;
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
                _Icon.sprite = config.Icon;
            }

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

            if (_CountRoot != null)
            {
                _CountRoot.SetActive(count > 0);
            }

            if (_PlusRoot != null)
            {
                _PlusRoot.SetActive(count <= 0);
            }

            if (_Button != null)
            {
                _Button.interactable = count > 0 || _AddHandler != null;
            }

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

        private void UpdateVisualState()
        {
            var targetGraphic = _Button != null ? _Button.targetGraphic : null;
            if (targetGraphic == null)
                return;

            if (_CurrentCount <= 0)
            {
                targetGraphic.color = _AddHandler != null ? _UnselectedColor : _UnavailableColor;
                return;
            }

            targetGraphic.color = _IsSelected ? _SelectedColor : _UnselectedColor;
        }
    }
}
