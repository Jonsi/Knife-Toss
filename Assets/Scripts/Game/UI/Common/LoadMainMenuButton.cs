using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.UI.Controllers;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Common
{
    /// <summary>
    /// Loads the main menu scene when the assigned button is clicked.
    /// </summary>
    public class LoadMainMenuButton : MonoBehaviour
    {
        [SerializeField] private Button _Button;

        [Inject] private SceneLoaderService _SceneLoaderService;
        private UIController _UIController;
        private bool _IsLoading;

        private void Awake()
        {
            if (_Button == null)
            {
                _Button = GetComponent<Button>();
            }

            _UIController = GetComponentInParent<UIController>();
        }

        private void Start()
        {
            if (_Button != null)
            {
                _Button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDestroy()
        {
            if (_Button != null)
            {
                _Button.onClick.RemoveListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            LoadMainMenuAsync().Forget();
        }

        private async UniTaskVoid LoadMainMenuAsync()
        {
            if (_IsLoading)
            {
                return;
            }

            _IsLoading = true;
            if (_Button != null)
            {
                _Button.interactable = false;
            }

            if (_UIController != null)
            {
                await _UIController.PlayStageOutAsync();
            }

            if (_SceneLoaderService != null)
            {
                await _SceneLoaderService.LoadMainMenu();
            }
        }
    }
}
