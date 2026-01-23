using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.PowerUps.Configs;

namespace Game.UI.PowerUps
{
    public class PowerUpListItem : MonoBehaviour
    {
        [SerializeField] private Image _Icon;
        [SerializeField] private TMP_Text _NameText;
        [SerializeField] private TMP_Text _DescriptionText;
        [SerializeField] private Button _Button;

        private PowerUpConfigBase _Config;
        private Action<PowerUpConfigBase> _OnClick;

        public void Setup(PowerUpConfigBase config, Action<PowerUpConfigBase> onClick)
        {
            _Config = config;
            _OnClick = onClick;

            if (_Icon != null) _Icon.sprite = config.Icon;
            if (_NameText != null) _NameText.text = config.DisplayName;
            if (_DescriptionText != null) _DescriptionText.text = config.Description;

            _Button.onClick.RemoveAllListeners();
            _Button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _OnClick?.Invoke(_Config);
        }
    }
}
