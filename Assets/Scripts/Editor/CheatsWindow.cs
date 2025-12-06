#if TEST_ENABLED
using Core.Events;
using Installers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Editor window for cheats and debug tools. Only available when TEST_ENABLED is defined.
    /// </summary>
    public class CheatsWindow : EditorWindow
    {
        private const string Title = "Cheats";
        private const string CommandChannelPrefKey = "CheatsWindow.CommandChannel";
        
        [SerializeField] private LevelCommandEventChannel commandChannel;
        private Vector2 scrollPosition;

        [MenuItem("Knife Toss/Cheats")]
        public static void ShowWindow()
        {
            GetWindow<CheatsWindow>(Title);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            LoadCommandChannelFromPrefs();
            FindCommandChannel();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                FindCommandChannel();
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Cheats & Debug Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This window is only available when TEST_ENABLED is defined.", MessageType.Info);
            
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use cheats.", MessageType.Warning);
                return;
            }

            if (commandChannel == null)
            {
                EditorGUILayout.HelpBox("LevelCommandEventChannel not found. Make sure the game is running and the scene is loaded.", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh Channel"))
                {
                    FindCommandChannel();
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Channel Status:", GUILayout.Width(100));
            EditorGUILayout.LabelField("Connected", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                FindCommandChannel();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Channel Assignment", EditorStyles.boldLabel);
            LevelCommandEventChannel newChannel = EditorGUILayout.ObjectField(
                "Level Command Event Channel", 
                commandChannel, 
                typeof(LevelCommandEventChannel), 
                true) as LevelCommandEventChannel;
            
            if (newChannel != commandChannel)
            {
                commandChannel = newChannel;
                SaveCommandChannelToPrefs();
            }
            
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawLevelCheats();

            EditorGUILayout.EndScrollView();
        }

        private void DrawLevelCheats()
        {
            EditorGUILayout.LabelField("Level Controls", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart Current Level", GUILayout.Height(30)))
            {
                RestartCurrentLevel();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void RestartCurrentLevel()
        {
            if (commandChannel != null)
            {
                commandChannel.RaiseRetry();
                Debug.Log("[CheatsWindow] Restarting current level...");
            }
            else
            {
                Debug.LogWarning("[CheatsWindow] Cannot restart level - command channel not found.");
            }
        }

        private void FindCommandChannel()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // If channel is already assigned, use it
            if (commandChannel != null)
            {
                return;
            }

            // Try to find the channel through GameplayInstaller in the scene
            var allInstallers = Resources.FindObjectsOfTypeAll<GameplayInstaller>();
            if (allInstallers != null && allInstallers.Length > 0)
            {
                // Prefer one that's not an asset (runtime instance)
                GameplayInstaller installer = null;
                foreach (var inst in allInstallers)
                {
                    if (inst.hideFlags == HideFlags.None || inst.hideFlags == HideFlags.DontSave)
                    {
                        installer = inst;
                        break;
                    }
                }

                // If no runtime instance found, use the first one
                if (installer == null && allInstallers.Length > 0)
                {
                    installer = allInstallers[0];
                }

                if (installer != null)
                {
                    // Use SerializedObject to access the private serialized field
                    SerializedObject serializedInstaller = new SerializedObject(installer);
                    SerializedProperty channelProperty = serializedInstaller.FindProperty("levelCommandEventChannel");
                    
                    if (channelProperty != null && channelProperty.objectReferenceValue != null)
                    {
                        commandChannel = channelProperty.objectReferenceValue as LevelCommandEventChannel;
                        if (commandChannel != null)
                        {
                            SaveCommandChannelToPrefs();
                            return;
                        }
                    }
                }
            }

            // Fallback: Find all LevelCommandEventChannel instances
            var allChannels = Resources.FindObjectsOfTypeAll<LevelCommandEventChannel>();
            if (allChannels != null && allChannels.Length > 0)
            {
                // Prefer one that's not an asset (runtime instance)
                foreach (var channel in allChannels)
                {
                    if (channel.hideFlags == HideFlags.None || channel.hideFlags == HideFlags.DontSave)
                    {
                        commandChannel = channel;
                        SaveCommandChannelToPrefs();
                        return;
                    }
                }

                // If no runtime instance found, use the first one
                if (commandChannel == null)
                {
                    commandChannel = allChannels[0];
                    SaveCommandChannelToPrefs();
                }
            }
        }

        private void LoadCommandChannelFromPrefs()
        {
            string guid = EditorPrefs.GetString(CommandChannelPrefKey, string.Empty);
            if (!string.IsNullOrEmpty(guid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    commandChannel = AssetDatabase.LoadAssetAtPath<LevelCommandEventChannel>(assetPath);
                }
            }
        }

        private void SaveCommandChannelToPrefs()
        {
            if (commandChannel != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(commandChannel);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    EditorPrefs.SetString(CommandChannelPrefKey, guid);
                }
            }
            else
            {
                EditorPrefs.DeleteKey(CommandChannelPrefKey);
            }
        }
    }
}
#endif

