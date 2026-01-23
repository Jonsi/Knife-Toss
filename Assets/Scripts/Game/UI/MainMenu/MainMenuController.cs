using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Persistence.Services;
using Game.UI.PowerUps;
using TMPro;
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
        [SerializeField] private Button _PvpButton;
        [SerializeField] private Button _PowerUpsButton;
        [SerializeField] private TMP_Text _CurrentStageText;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;

        private void Start()
        {
            _PlayButton.onClick.AddListener(OnPlayClicked);
            _StagesButton.onClick.AddListener(OnStagesClicked);
            
            if (_PvpButton != null)
                _PvpButton.onClick.AddListener(OnPvpClicked);

            if (_PowerUpsButton != null)
                _PowerUpsButton.onClick.AddListener(OnPowerUpsClicked);

            UpdateCurrentStageText();
        }

        private void UpdateCurrentStageText()
        {
            if (_CurrentStageText != null && _GameData != null)
            {
                int stageNumber = _GameData.CurrentStageIndex + 1;
                _CurrentStageText.text = $"Stage {stageNumber}";
            }
        }

        private void OnDestroy()
        {
            _PlayButton.onClick.RemoveListener(OnPlayClicked);
            _StagesButton.onClick.RemoveListener(OnStagesClicked);
            
            if (_PvpButton != null)
                _PvpButton.onClick.RemoveListener(OnPvpClicked);

            if (_PowerUpsButton != null)
                _PowerUpsButton.onClick.RemoveListener(OnPowerUpsClicked);
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

        private void OnPvpClicked()
        {
            _SceneLoaderService.LoadPVP().Forget();
        }

        private void OnPowerUpsClicked()
        {
            _SceneLoaderService.LoadPowerUps().Forget();
        }
    }
}
