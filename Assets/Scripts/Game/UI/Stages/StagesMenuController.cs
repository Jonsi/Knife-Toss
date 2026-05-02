using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Stages
{
    /// <summary>
    /// Stages Menu scene navigation. World rows are built by <see cref="StageListController"/>.
    /// </summary>
    public class StagesMenuController : MonoBehaviour
    {
        [SerializeField] private Button _BackButton;

        [Inject] private SceneLoaderService _SceneLoaderService;

        private void OnEnable()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDisable()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }
    }
}
