using System;
using Core.Abstractions;
using Core.Data;
using Infrastructure.Persistence;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Player data inspector/editor backed by the file-based save system.
    /// </summary>
    public class PlayerDataEditorWindow : EditorWindow
    {
        private const string Title = "Player Data";

        private IPlayerDataStore dataStore;
        private PlayerData workingCopy;
        private PlayerDataContainer container;
        private SerializedObject serializedData;
        private Vector2 scrollPosition;
        private readonly System.Collections.Generic.List<StarEntry> starEntries = new System.Collections.Generic.List<StarEntry>();

        [MenuItem("Knife Toss/Player Data")]
        public static void ShowWindow()
        {
            GetWindow<PlayerDataEditorWindow>(Title);
        }

        private void OnEnable()
        {
            dataStore = new FilePlayerDataStore();
            ReloadFromDisk();
        }

        private void OnDisable()
        {
            if (container != null)
            {
                DestroyImmediate(container);
                container = null;
            }
        }

        private void ReloadFromDisk()
        {
            workingCopy = Clone(dataStore.LoadOrCreate());
            BuildSerializedObject();
            RebuildStarEntries();
            Repaint();
        }

        private void BuildSerializedObject()
        {
            if (container != null)
            {
                DestroyImmediate(container);
            }

            container = ScriptableObject.CreateInstance<PlayerDataContainer>();
            container.hideFlags = HideFlags.HideAndDontSave;
            container.data = workingCopy;
            serializedData = new SerializedObject(container);
        }

        private void OnGUI()
        {
            if (serializedData == null)
            {
                ReloadFromDisk();
            }

            bool prevEnabled = GUI.enabled;
            GUI.enabled = true;

            serializedData.UpdateIfRequiredOrScript();

            EditorGUILayout.LabelField("Player Data (persistent)", EditorStyles.boldLabel);
            string filePath = (dataStore as FilePlayerDataStore)?.FilePath ?? "<unknown>";
            EditorGUILayout.HelpBox($"Save file: {filePath}", MessageType.None);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawPlayerDataFields();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawActions();

            GUI.enabled = prevEnabled;
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload"))
            {
                ReloadFromDisk();
            }

            if (GUILayout.Button("Save"))
            {
                SaveToDisk();
            }

            if (GUILayout.Button("Reset"))
            {
                if (EditorUtility.DisplayDialog("Reset Player Data", "Reset all player data to defaults?", "Reset", "Cancel"))
                {
                    ResetData();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SaveToDisk()
        {
            PushStarEntriesIntoData();
            dataStore.Save(workingCopy);

            if (PlayerDataLocator.Runtime != null)
            {
                PlayerDataLocator.Runtime.Replace(Clone(workingCopy));
            }
        }

        private void ResetData()
        {
            workingCopy = PlayerData.CreateDefault();
            BuildSerializedObject();
            RebuildStarEntries();
            SaveToDisk();
        }

        private void DrawMigrationSection()
        {
        }

        private static PlayerData Clone(PlayerData source)
        {
            string json = JsonUtility.ToJson(source ?? PlayerData.CreateDefault());
            return JsonUtility.FromJson<PlayerData>(json);
        }

        private void DrawPlayerDataFields()
        {
            if (workingCopy == null)
            {
                EditorGUILayout.HelpBox("PlayerData is null.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (workingCopy.settings != null)
            {
                workingCopy.settings.audio.isMuted = EditorGUILayout.Toggle("Audio Muted", workingCopy.settings.audio.isMuted);
                workingCopy.settings.language = EditorGUILayout.TextField("Language", workingCopy.settings.language);
                workingCopy.settings.vibrationEnabled = EditorGUILayout.Toggle("Vibration Enabled", workingCopy.settings.vibrationEnabled);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (workingCopy.progress != null)
            {
                workingCopy.progress.currentStageIndex = EditorGUILayout.IntField("Current Stage Index", workingCopy.progress.currentStageIndex);
                workingCopy.progress.currentLevelIndex = EditorGUILayout.IntField("Current Level Index", workingCopy.progress.currentLevelIndex);
                workingCopy.progress.totalFruitsHit = EditorGUILayout.IntField("Total Fruits Hit", workingCopy.progress.totalFruitsHit);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Level Progress (Stars per Level)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                DrawStarEntries();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private void DrawStarEntries()
        {
            if (starEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("No levels tracked yet. Add entries below.", MessageType.None);
            }

            for (int i = 0; i < starEntries.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                starEntries[i].levelIndex = EditorGUILayout.IntField("Level", starEntries[i].levelIndex);
                starEntries[i].stars = EditorGUILayout.IntField("Stars", starEntries[i].stars);
                if (GUILayout.Button("X", GUILayout.Width(24f)))
                {
                    starEntries.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Level Entry"))
            {
                starEntries.Add(new StarEntry { levelIndex = 1, stars = 0 });
            }
        }

        private void RebuildStarEntries()
        {
            starEntries.Clear();
            if (workingCopy?.progress?.levelProgress?.starsPerLevel == null)
            {
                return;
            }

            foreach (var kvp in workingCopy.progress.levelProgress.starsPerLevel)
            {
                starEntries.Add(new StarEntry { levelIndex = kvp.Key, stars = kvp.Value });
            }
        }

        private void PushStarEntriesIntoData()
        {
            var levelProgress = workingCopy?.progress?.levelProgress;
            if (levelProgress == null)
            {
                return;
            }

            levelProgress.starsPerLevel.Clear();
            foreach (var entry in starEntries)
            {
                if (entry.levelIndex < 1 || entry.stars < 0)
                {
                    continue;
                }
                levelProgress.starsPerLevel[entry.levelIndex] = entry.stars;
            }
        }

        [Serializable]
        private class PlayerDataContainer : ScriptableObject
        {
            public PlayerData data;
        }

        [Serializable]
        private class StarEntry
        {
            public int levelIndex;
            public int stars;
        }
    }
}
