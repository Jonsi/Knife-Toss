using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Events;
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
        private const string KnifeImpactChannelPath = "Assets/Configurations/EventChannels/Knife/KnifeImpactEventChannel.asset";

        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;
        [SerializeField] private KnifeImpactEventChannel _KnifeImpactChannel;
        [SerializeField] private bool _PauseOnCollision;

        private KnifeImpactEventChannel _SubscribedImpactChannel;

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
            UpdateImpactSubscription();
        }

        private void OnDisable()
        {
            UnsubscribeFromImpactChannel();
        }

        private void OnGUI()
        {
            DrawChannelAssignments();
            EditorGUILayout.Space(10f);
            DrawCheatActions();
            EditorGUILayout.Space(10f);
            DrawPauseOnCollisionToggle();
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

            var impactChannel = (KnifeImpactEventChannel)EditorGUILayout.ObjectField(
                "Knife Impact Channel",
                _KnifeImpactChannel,
                typeof(KnifeImpactEventChannel),
                false);
            if (!ReferenceEquals(impactChannel, _KnifeImpactChannel))
            {
                _KnifeImpactChannel = impactChannel;
                UpdateImpactSubscription();
            }
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

        private void DrawPauseOnCollisionToggle()
        {
            bool pauseOnCollision = EditorGUILayout.Toggle("Pause On Knife Collision", _PauseOnCollision);
            if (pauseOnCollision != _PauseOnCollision)
            {
                _PauseOnCollision = pauseOnCollision;
                UpdateImpactSubscription();
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

            if (_KnifeImpactChannel == null)
            {
                _KnifeImpactChannel = AssetDatabase.LoadAssetAtPath<KnifeImpactEventChannel>(KnifeImpactChannelPath);
            }
        }

        private void UpdateImpactSubscription()
        {
            UnsubscribeFromImpactChannel();
            if (!_PauseOnCollision || _KnifeImpactChannel == null)
            {
                return;
            }

            _KnifeImpactChannel.Subscribe(OnKnifeImpact);
            _SubscribedImpactChannel = _KnifeImpactChannel;
        }

        private void UnsubscribeFromImpactChannel()
        {
            if (_SubscribedImpactChannel == null)
            {
                return;
            }

            _SubscribedImpactChannel.Unsubscribe(OnKnifeImpact);
            _SubscribedImpactChannel = null;
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            if (!EditorApplication.isPlaying || data.ImpactType != KnifeImpactType.Bounced)
            {
                return;
            }

            EditorApplication.isPaused = true;
            Debug.Log("Cheats Window: Paused Editor on knife collision.");
        }
    }
}
