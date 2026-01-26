using Game.UI.Controllers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.HUD
{
    public class SettingsButtonView : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        
        private UIController _UIController;

        [Inject]
        public void Construct(UIController uiController)
        {
            _UIController = uiController;
        }

        private void OnEnable()
        {
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
            if (_UIController != null)
            {
                _UIController.ToggleSettings();
            }
        }
    }
}
