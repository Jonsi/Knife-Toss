using Cysharp.Threading.Tasks;
using Game.Flow.Game;
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

        private void Awake()
        {
            if (_Button == null)
            {
                _Button = GetComponent<Button>();
            }
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
            _SceneLoaderService.LoadMainMenu().Forget();
        }
    }
}
