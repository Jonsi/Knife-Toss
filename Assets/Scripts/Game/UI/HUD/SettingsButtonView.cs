using Game.UI.Controllers;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.HUD
{
    public class SettingsButtonView : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        
        private ISettingsPopupPresenter _SettingsPresenter;

        [Inject]
        public void Construct(ISettingsPopupPresenter settingsPresenter)
        {
            _SettingsPresenter = settingsPresenter;
        }

        private void OnEnable()
        {
            ActionButtonPresenter.GetOrAdd(_Button);
            if (_Button != null)
                _Button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            if (_Button != null)
                _Button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            _SettingsPresenter?.ToggleSettings();
        }
    }
}
