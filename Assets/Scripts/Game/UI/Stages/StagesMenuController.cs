using System.Collections.Generic;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Stages
{
    /// <summary>
    /// Controller for the stages selection menu.
    /// </summary>
    public class StagesMenuController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Collection of all stages.")]
        [SerializeField] private StagesCollection _StagesCollection;
        
        [Header("UI References")]
        [Tooltip("Prefab for the stage item.")]
        [SerializeField] private UiStageItem _StageItemPrefab;

        [Tooltip("Root container for stage items.")]
        [SerializeField] private Transform _ContentRoot;

        [Header("Scene Management")]
        [Tooltip("Name of the game scene to load.")]
        [SerializeField] private string _GameSceneName = "MainScene";

        private void Start()
        {
            if (_StagesCollection == null)
            {
                Debug.LogError("StagesCollection is not assigned in StagesMenuController.");
                return;
            }

            InitializeList();
        }

        private void InitializeList()
        {
            // Clear existing items if any (optional, useful if re-initializing)
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
                
                // Get icon from LayoutSettings
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
            Debug.Log($"Selected Stage Index: {stageIndex}");
            
            // Save selected stage index so the game scene can read it
            PlayerPrefs.SetInt("SelectedStageIndex", stageIndex);
            PlayerPrefs.Save();

            // Load the game scene
            SceneManager.LoadScene(_GameSceneName);
        }
    }
}
