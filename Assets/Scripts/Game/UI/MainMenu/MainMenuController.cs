using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.MainMenu
{
    /// <summary>
    /// Controller for the main menu scene. Handles navigation to gameplay and stages menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _PlayButton;
        [SerializeField] private Button _StagesButton;

        private PlayerDataService _PlayerDataService;
        private SceneLoaderService _SceneLoaderService;

        /// <summary>
        /// Injects required dependencies from the DI container.
        /// </summary>
        public void Construct(PlayerDataService playerDataService, SceneLoaderService sceneLoaderService)
        {
            _PlayerDataService = playerDataService;
            _SceneLoaderService = sceneLoaderService;
        }

        private void Start()
        {
            _PlayButton.onClick.AddListener(OnPlayClicked);
            _StagesButton.onClick.AddListener(OnStagesClicked);
        }

        private void OnDestroy()
        {
            _PlayButton.onClick.RemoveListener(OnPlayClicked);
            _StagesButton.onClick.RemoveListener(OnStagesClicked);
        }

        private void OnPlayClicked()
        {
            int highestStage = _PlayerDataService.GetHighestStageIndex();
            PlayerPrefs.SetInt("SelectedStageIndex", highestStage);
            PlayerPrefs.Save();

            _SceneLoaderService.LoadGame().Forget();
        }

        private void OnStagesClicked()
        {
            _SceneLoaderService.LoadStagesMenu().Forget();
        }
    }
}
