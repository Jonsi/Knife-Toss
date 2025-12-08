using System;
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

        private FilePlayerDataStore dataStore;
        private PlayerData workingCopy;
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

        private void ReloadFromDisk()
        {
            PlayerData loaded = dataStore.LoadOrCreate();
            workingCopy = DeepClone(loaded);
            EnsureDefaults();
            RebuildStarEntries();
            
            Debug.Log($"Editor Reloaded: CurrentStage={workingCopy.progress.currentStageIndex}, HighestStage={workingCopy.progress.highestStageIndex}");
            Repaint();
        }

        private void EnsureDefaults()
        {
            if (workingCopy.settings == null)
            {
                workingCopy.settings = new PlayerSettingsData();
            }

            if (workingCopy.progress == null)
            {
                workingCopy.progress = new PlayerProgressData();
            }

            if (workingCopy.progress.levelProgress == null)
            {
                workingCopy.progress.levelProgress = new LevelProgressData();
                workingCopy.progress.levelProgress.OnAfterDeserialize();
            }

            if (workingCopy.settings.audio == null)
            {
                workingCopy.settings.audio = new AudioSettingsData { isMuted = false };
            }
        }

        private void OnGUI()
        {
            if (workingCopy == null)
            {
                ReloadFromDisk();
                return;
            }

            EditorGUILayout.LabelField("Player Data (persistent)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Save file: {dataStore.FilePath}", MessageType.None);

            EditorGUI.BeginChangeCheck();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawPlayerDataFields();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawActions();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
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
            EnsureLevelProgressSerialization();
            
            Debug.Log($"Saving: CurrentStage={workingCopy.progress.currentStageIndex}, HighestStage={workingCopy.progress.highestStageIndex}");
            
            dataStore.Save(workingCopy);

            if (PlayerDataLocator.Runtime != null)
            {
                PlayerDataLocator.Runtime.Replace(DeepClone(workingCopy));
            }

            Debug.Log("Player data saved successfully.");
        }

        private void EnsureLevelProgressSerialization()
        {
            if (workingCopy?.progress?.levelProgress != null)
            {
                workingCopy.progress.levelProgress.OnBeforeSerialize();
            }
        }

        private void ResetData()
        {
            workingCopy = PlayerData.CreateDefault();
            EnsureDefaults();
            RebuildStarEntries();
            SaveToDisk();
        }

        private static PlayerData DeepClone(PlayerData source)
        {
            if (source == null)
            {
                return PlayerData.CreateDefault();
            }

            string json = JsonUtility.ToJson(source);
            PlayerData cloned = JsonUtility.FromJson<PlayerData>(json);
            
            if (cloned.progress?.levelProgress != null)
            {
                cloned.progress.levelProgress.OnAfterDeserialize();
            }

            return cloned;
        }

        private void DrawPlayerDataFields()
        {
            DrawSettings();
            EditorGUILayout.Space(8f);
            DrawProgress();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var settings = workingCopy.settings;
            if (settings != null)
            {
                settings.audio.isMuted = EditorGUILayout.Toggle("Audio Muted", settings.audio.isMuted);
                settings.language = EditorGUILayout.TextField("Language", settings.language);
                settings.vibrationEnabled = EditorGUILayout.Toggle("Vibration Enabled", settings.vibrationEnabled);
            }
            EditorGUI.indentLevel--;
        }

        private void DrawProgress()
        {
            EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var progress = workingCopy.progress;
            if (progress != null)
            {
                int newCurrentStage = EditorGUILayout.IntField("Current Stage Index", progress.currentStageIndex);
                if (newCurrentStage != progress.currentStageIndex)
                {
                    progress.currentStageIndex = Mathf.Max(1, newCurrentStage);
                }

                int newHighestStage = EditorGUILayout.IntField("Highest Stage Index", progress.highestStageIndex);
                if (newHighestStage != progress.highestStageIndex)
                {
                    progress.highestStageIndex = Mathf.Max(1, newHighestStage);
                }

                progress.totalFruitsHit = EditorGUILayout.IntField("Total Fruits Hit", progress.totalFruitsHit);

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
                if (entry.levelIndex >= 1 && entry.stars >= 0)
                {
                    levelProgress.starsPerLevel[entry.levelIndex] = entry.stars;
                }
            }
        }

        [Serializable]
        private class StarEntry
        {
            public int levelIndex;
            public int stars;
        }
    }
}
