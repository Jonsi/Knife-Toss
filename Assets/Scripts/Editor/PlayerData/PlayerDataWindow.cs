using System.Collections.Generic;
using System.Linq;
using Game.Persistence.Data;
using Game.Persistence.Storage;
using Game.PowerUps.Configs;
using Game.PowerUps.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Simple editor window for inspecting and editing persisted player data.
    /// </summary>
    public class PlayerDataWindow : EditorWindow
    {
        private FilePlayerDataStore _Store;
        private PlayerData _PlayerData;
        private bool _HasUnsavedChanges;
        private PowerUpCatalog _Catalog;
        private List<PowerUpConfigBase> _PowerUpConfigs = new List<PowerUpConfigBase>();

        private static class Styles
        {
            public static readonly GUIContent LoadButton = new GUIContent("Load");
            public static readonly GUIContent SaveButton = new GUIContent("Save");
            public static readonly GUIContent ResetButton = new GUIContent("Reset");
            public static readonly GUIContent ProgressHeader = new GUIContent("Progress");
            public static readonly GUIContent CurrentStage = new GUIContent("Current Stage");
            public static readonly GUIContent HighestStage = new GUIContent("Highest Stage");
            public static readonly GUIContent TomatoCount = new GUIContent("Tomatoes Collected");
            public static readonly GUIContent SettingsHeader = new GUIContent("Settings");
            public static readonly GUIContent Muted = new GUIContent("Muted");
            public static readonly GUIContent PowerUpsHeader = new GUIContent("Power Ups");
        }

        [MenuItem("Tools/Player Data")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerDataWindow>("Player Data");
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Player Data");
            _Store ??= new FilePlayerDataStore();
            LoadCatalog();
            LoadData();
        }

        private void LoadCatalog()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(PowerUpCatalog).Name}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _Catalog = AssetDatabase.LoadAssetAtPath<PowerUpCatalog>(path);
                if (_Catalog != null)
                {
                    _PowerUpConfigs = _Catalog.Entries.Where(config => config != null && !string.IsNullOrEmpty(config.InventoryKey)).ToList();
                }
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_PlayerData == null)
            {
                EditorGUILayout.HelpBox("No player data could be loaded.", MessageType.Warning);
                return;
            }

            if (_HasUnsavedChanges)
            {
                EditorGUILayout.HelpBox("You have unsaved changes.", MessageType.Info);
            }

            EditorGUILayout.Space(5f);
            DrawProgressSection();
            EditorGUILayout.Space(10f);
            DrawSettingsSection();
            EditorGUILayout.Space(10f);
            DrawPowerUpSection();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(Styles.LoadButton, EditorStyles.toolbarButton))
            {
                LoadData();
            }

            using (new EditorGUI.DisabledScope(!_HasUnsavedChanges || _PlayerData == null))
            {
                if (GUILayout.Button(Styles.SaveButton, EditorStyles.toolbarButton))
                {
                    SaveData();
                }
            }

            using (new EditorGUI.DisabledScope(_PlayerData == null))
            {
                if (GUILayout.Button(Styles.ResetButton, EditorStyles.toolbarButton))
                {
                    ResetData();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgressSection()
        {
            EditorGUILayout.LabelField(Styles.ProgressHeader, EditorStyles.boldLabel);
            var progress = EnsureProgressData();

            EditorGUI.BeginChangeCheck();
            int currentStage = EditorGUILayout.IntField(Styles.CurrentStage, progress.CurrentStageIndex);
            int highestStage = EditorGUILayout.IntField(Styles.HighestStage, progress.HighestStageIndex);
            int tomatoCount = EditorGUILayout.IntField(Styles.TomatoCount, progress.TotalCollectiblesCollected);

            if (EditorGUI.EndChangeCheck())
            {
                progress.CurrentStageIndex = Mathf.Max(0, currentStage);
                progress.HighestStageIndex = Mathf.Max(0, highestStage);
                progress.TotalCollectiblesCollected = Mathf.Max(0, tomatoCount);
                MarkDirty();
            }
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField(Styles.SettingsHeader, EditorStyles.boldLabel);
            var settings = EnsureSettingsData();

            EditorGUI.BeginChangeCheck();
            bool isMuted = EditorGUILayout.Toggle(Styles.Muted, settings.IsMuted);

            if (EditorGUI.EndChangeCheck())
            {
                settings.IsMuted = isMuted;
                MarkDirty();
            }
        }

        private void DrawPowerUpSection()
        {
            EditorGUILayout.LabelField(Styles.PowerUpsHeader, EditorStyles.boldLabel);
            var inventory = EnsurePowerUpInventory();

            if (_Catalog == null || _PowerUpConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("No PowerUpCatalog found or no power-ups configured. Please ensure a PowerUpCatalog asset exists in the project.", MessageType.Warning);
                if (GUILayout.Button("Reload Catalog"))
                {
                    LoadCatalog();
                }
                return;
            }

            bool hasChanges = false;

            for (int i = 0; i < _PowerUpConfigs.Count; i++)
            {
                var config = _PowerUpConfigs[i];
                if (config == null || string.IsNullOrEmpty(config.InventoryKey))
                    continue;

                string displayName = string.IsNullOrEmpty(config.name) ? config.InventoryKey : config.name;
                GUIContent label = new GUIContent(displayName);
                int currentCount = inventory.GetCount(config.InventoryKey);
                
                EditorGUI.BeginChangeCheck();
                int newCount = EditorGUILayout.IntField(label, currentCount);
                
                if (EditorGUI.EndChangeCheck())
                {
                    inventory.SetCount(config.InventoryKey, Mathf.Max(0, newCount));
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                MarkDirty();
            }
        }

        private void LoadData()
        {
            if (_Store == null)
            {
                _Store = new FilePlayerDataStore();
            }

            _PlayerData = _Store.Load() ?? new PlayerData();
            EnsureProgressData();
            EnsureSettingsData();

            _HasUnsavedChanges = false;
            Repaint();
        }

        private void SaveData()
        {
            if (_PlayerData == null || _Store == null)
            {
                return;
            }

            _Store.Save(_PlayerData);
            _HasUnsavedChanges = false;
        }

        private void ResetData()
        {
            _PlayerData = new PlayerData();
            _HasUnsavedChanges = true;
        }

        private PlayerProgressData EnsureProgressData()
        {
            if (_PlayerData.Progress == null)
            {
                _PlayerData.Progress = new PlayerProgressData();
            }

            return _PlayerData.Progress;
        }

        private PlayerSettingsData EnsureSettingsData()
        {
            if (_PlayerData.Settings == null)
            {
                _PlayerData.Settings = new PlayerSettingsData();
            }

            return _PlayerData.Settings;
        }

        private void MarkDirty()
        {
            _HasUnsavedChanges = true;
        }

        private PowerUpInventoryData EnsurePowerUpInventory()
        {
            if (_PlayerData.PowerUpInventory == null)
            {
                _PlayerData.PowerUpInventory = new PowerUpInventoryData();
            }

            return _PlayerData.PowerUpInventory;
        }
    }
}
