using System;
using Game.PowerUps.Configs;
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
        [SerializeField] private Text _CountText;
        [SerializeField] private GameObject _SelectedMarker;

        private PowerUpConfigBase _Config;
        private Action<PowerUpConfigBase> _ClickHandler;

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
            if (_CountText != null)
            {
                _CountText.text = count.ToString();
            }

            if (_Button != null)
            {
                _Button.interactable = count > 0;
            }
        }

        /// <summary>
        /// Updates the selected marker state.
        /// </summary>
        public void UpdateSelected(bool isSelected)
        {
            if (_SelectedMarker != null)
            {
                _SelectedMarker.SetActive(isSelected);
            }
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
    }
}
