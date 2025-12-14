using Common.Events;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Simple cheats window for quickly triggering common debug actions.
    /// </summary>
    public class CheatsWindow : EditorWindow
    {
        private const string RetryChannelPath = "Assets/Configurations/EventChannels/UI/LevelFailedPopup/RetryLevelClicked.asset";
        private const string RestartChannelPath = "Assets/Configurations/EventChannels/UI/LevelFailedPopup/RestartStageClicked.asset";

        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

        [MenuItem("Tools/Cheats")]
        public static void ShowWindow()
        {
            var window = GetWindow<CheatsWindow>("Cheats");
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Cheats");
            TryAutoAssignChannels();
        }

        private void OnGUI()
        {
            DrawChannelAssignments();
            EditorGUILayout.Space(10f);
            DrawCheatActions();
        }

        private void DrawChannelAssignments()
        {
            EditorGUILayout.LabelField("Event Channels", EditorStyles.boldLabel);
            _RetryLevelChannel = (VoidEventChannel)EditorGUILayout.ObjectField(
                "Retry Level Channel",
                _RetryLevelChannel,
                typeof(VoidEventChannel),
                false);
            _RestartStageChannel = (VoidEventChannel)EditorGUILayout.ObjectField(
                "Restart Stage Channel",
                _RestartStageChannel,
                typeof(VoidEventChannel),
                false);
        }

        private void DrawCheatActions()
        {
            EditorGUILayout.LabelField("Runtime Actions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play Mode to trigger cheats.", MessageType.Info);
                }

                if (GUILayout.Button("Restart Level"))
                {
                    RaiseChannel(_RetryLevelChannel, "Retry Level");
                }

                if (GUILayout.Button("Restart Stage"))
                {
                    RaiseChannel(_RestartStageChannel, "Restart Stage");
                }
            }
        }

        private void RaiseChannel(VoidEventChannel channel, string actionName)
        {
            if (channel == null)
            {
                Debug.LogWarning($"Cheats Window: {actionName} channel is not assigned.");
                return;
            }

            channel.Raise();
            Debug.Log($"Cheats Window: {actionName} triggered.");
        }

        private void TryAutoAssignChannels()
        {
            if (_RetryLevelChannel == null)
            {
                _RetryLevelChannel = AssetDatabase.LoadAssetAtPath<VoidEventChannel>(RetryChannelPath);
            }

            if (_RestartStageChannel == null)
            {
                _RestartStageChannel = AssetDatabase.LoadAssetAtPath<VoidEventChannel>(RestartChannelPath);
            }
        }
    }
}
