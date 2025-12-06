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
        private LevelCommandEventChannel commandChannel;
        private Vector2 scrollPosition;

        [MenuItem("Knife Toss/Cheats")]
        public static void ShowWindow()
        {
            GetWindow<CheatsWindow>(Title);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
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
            commandChannel = null;

            if (!Application.isPlaying)
            {
                return;
            }

            // Try to find the channel through GameplayInstaller in the scene
            GameplayInstaller installer = FindObjectOfType<GameplayInstaller>();
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
                        return;
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
                        break;
                    }
                }

                // If no runtime instance found, use the first one
                if (commandChannel == null)
                {
                    commandChannel = allChannels[0];
                }
            }
        }
    }
}
#endif

