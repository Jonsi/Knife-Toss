using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

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

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;

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
            if (_GameData != null)
            {
                _GameData.CurrentStageIndex = highestStage;
            }

            _SceneLoaderService.LoadGame().Forget();
        }

        private void OnStagesClicked()
        {
            _SceneLoaderService.LoadStagesMenu().Forget();
        }
    }
}
