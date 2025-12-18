using Game.Persistence.Data;
using Game.Persistence.Storage;
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
            LoadData();
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

            if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                LoadData();
            }

            using (new EditorGUI.DisabledScope(!_HasUnsavedChanges || _PlayerData == null))
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    SaveData();
                }
            }

            using (new EditorGUI.DisabledScope(_PlayerData == null))
            {
                if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
                {
                    ResetData();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgressSection()
        {
            EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
            var progress = EnsureProgressData();

            EditorGUI.BeginChangeCheck();
            int currentStage = EditorGUILayout.IntField("Current Stage", progress.CurrentStageIndex);
            int highestStage = EditorGUILayout.IntField("Highest Stage", progress.HighestStageIndex);
            int tomatoCount = EditorGUILayout.IntField("Tomatoes Collected", progress.TotalCollectiblesCollected);

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
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            var settings = EnsureSettingsData();

            EditorGUI.BeginChangeCheck();
            bool isMuted = EditorGUILayout.Toggle("Muted", settings.IsMuted);

            if (EditorGUI.EndChangeCheck())
            {
                settings.IsMuted = isMuted;
                MarkDirty();
            }
        }

        private void DrawPowerUpSection()
        {
            EditorGUILayout.LabelField("Power Ups", EditorStyles.boldLabel);
            var inventory = EnsurePowerUpInventory();

            EditorGUI.BeginChangeCheck();
            int superKnife = EditorGUILayout.IntField("Super Knife", inventory.SuperKnifeCount);
            int multipleKnives = EditorGUILayout.IntField("Multiple Knives", inventory.MultipleKnivesCount);
            int collectiblesBlade = EditorGUILayout.IntField("Collectibles Blade", inventory.CollectiblesBladeCount);

            if (EditorGUI.EndChangeCheck())
            {
                inventory.SuperKnifeCount = Mathf.Max(0, superKnife);
                inventory.MultipleKnivesCount = Mathf.Max(0, multipleKnives);
                inventory.CollectiblesBladeCount = Mathf.Max(0, collectiblesBlade);
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
