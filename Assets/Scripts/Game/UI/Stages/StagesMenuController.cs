using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Stages
{
    /// <summary>
    /// Controller for the stages selection menu.
    /// </summary>
    public class StagesMenuController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private StagesCollection _StagesCollection;
        
        [Header("UI References")]
        [SerializeField] private UiStageItem _StageItemPrefab;
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private Button _BackButton;

        private SceneLoaderService _SceneLoaderService;
        private GameData _GameData;

        /// <summary>
        /// Injects required dependencies from the DI container.
        /// </summary>
        public void Construct(SceneLoaderService sceneLoaderService, GameData gameData)
        {
            _SceneLoaderService = sceneLoaderService;
            _GameData = gameData;
        }

        private void Start()
        {
            if (_StagesCollection == null)
            {
                Debug.LogError("StagesCollection is not assigned in StagesMenuController.");
                return;
            }

            InitializeList();

            if (_BackButton != null)
            {
                _BackButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDestroy()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void InitializeList()
        {
            foreach (Transform child in _ContentRoot)
            {
                Destroy(child.gameObject);
            }

            var stages = _StagesCollection.Stages;
            for (int i = 0; i < stages.Count; i++)
            {
                var stageConfig = stages[i];
                if (stageConfig == null) continue;

                var item = Instantiate(_StageItemPrefab, _ContentRoot);
                
                Sprite icon = null;
                if (stageConfig.LayoutSettings != null)
                {
                    icon = stageConfig.LayoutSettings.StageIcon;
                }

                item.Initialize(icon, i + 1, stageConfig.StageName, i, OnStageSelected);
            }
        }

        private void OnStageSelected(int stageIndex)
        {
            if (_GameData != null)
            {
                _GameData.CurrentStageIndex = stageIndex;
            }

            _SceneLoaderService.LoadGame().Forget();
        }

        private void OnBackClicked()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }
    }
}
