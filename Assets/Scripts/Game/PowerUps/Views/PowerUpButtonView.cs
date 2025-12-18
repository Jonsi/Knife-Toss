using System;
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
        [SerializeField] private Color _SelectedColor = Color.white;
        [SerializeField] private Color _UnselectedColor = Color.gray;
        [SerializeField] private Color _UnavailableColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        private PowerUpConfigBase _Config;
        private Action<PowerUpConfigBase> _ClickHandler;
        private int _CurrentCount;
        private bool _IsSelected;

        /// <summary>
        /// Initializes the button with the given configuration.
        /// </summary>
        public void Initialize(PowerUpConfigBase config, Action<PowerUpConfigBase> clickHandler)
        {
            _Config = config;
            _ClickHandler = clickHandler;
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

            if (_Button != null)
            {
                _Button.interactable = count > 0;
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
            _ClickHandler?.Invoke(_Config);
        }

        private void OnDestroy()
        {
            if (_Button != null)
            {
                _Button.onClick.RemoveListener(HandleClick);
            }
        }

        private void UpdateVisualState()
        {
            var targetGraphic = _Button != null ? _Button.targetGraphic : null;
            if (targetGraphic == null)
                return;

            if (_CurrentCount <= 0)
            {
                targetGraphic.color = _UnavailableColor;
                return;
            }

            targetGraphic.color = _IsSelected ? _SelectedColor : _UnselectedColor;
        }
    }
}
