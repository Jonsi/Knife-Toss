using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Events;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;
#if TEST_ENABLED
using Game.Debugging;
#endif

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

        // Editor state for inputs
        [SerializeField] private int _JumpToStageIndex = 1;
        [SerializeField] private int _AddCoinsAmount = 100;
        private readonly System.Collections.Generic.Dictionary<string, int> _PowerUpCountsCache = new();

        private KnifeImpactEventChannel _SubscribedImpactChannel;

        private static class Styles
        {
            public static readonly GUIContent EventChannelsHeader = new GUIContent("Event Channels");
            public static readonly GUIContent RetryLevelChannel = new GUIContent("Retry Level Channel");
            public static readonly GUIContent RestartStageChannel = new GUIContent("Restart Stage Channel");
            public static readonly GUIContent KnifeImpactChannel = new GUIContent("Knife Impact Channel");
            
            public static readonly GUIContent GeneralHeader = new GUIContent("General Cheats");
            public static readonly GUIContent StageLevelHeader = new GUIContent("Stage & Level Cheats");
            public static readonly GUIContent EconomyHeader = new GUIContent("Economy Cheats");
            public static readonly GUIContent PowerUpsHeader = new GUIContent("Power-Up Cheats");
            public static readonly GUIContent CollisionHeader = new GUIContent("Collision Cheats");

            public static readonly GUIContent RestartLevelButton = new GUIContent("Restart Level");
            public static readonly GUIContent RestartStageButton = new GUIContent("Restart Stage");
            public static readonly GUIContent NextLevelButton = new GUIContent("Next Level");
            public static readonly GUIContent PrevLevelButton = new GUIContent("Prev Level");
            public static readonly GUIContent JumpToStageButton = new GUIContent("Jump to Stage");
            public static readonly GUIContent ClearProgressButton = new GUIContent("Clear All Progress");
            public static readonly GUIContent AddCoinsButton = new GUIContent("Add Coins");
            public static readonly GUIContent SetButton = new GUIContent("Set");
            public static readonly GUIContent RefreshButton = new GUIContent("Refresh");

            public static readonly GUIContent PauseOnCollision = new GUIContent("Pause On Knife Collision");
        }

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
            RefreshPowerUpCache();
        }

        private void OnDisable()
        {
            UnsubscribeFromImpactChannel();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 160;

            DrawChannelAssignments();
            EditorGUILayout.Space(10f);
            
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to access most cheats.", MessageType.Info);
            }

            DrawGeneralCheats();
            EditorGUILayout.Space(10f);
            DrawStageLevelCheats();
            EditorGUILayout.Space(10f);
            DrawEconomyCheats();
            EditorGUILayout.Space(10f);
            DrawPowerUpCheats();
            EditorGUILayout.Space(10f);
            DrawPauseOnCollisionToggle();
        }

        private void DrawPauseOnCollisionToggle()
        {
            EditorGUILayout.LabelField(Styles.CollisionHeader, EditorStyles.boldLabel);
            bool pauseOnCollision = EditorGUILayout.Toggle(Styles.PauseOnCollision, _PauseOnCollision);
            if (pauseOnCollision != _PauseOnCollision)
            {
                _PauseOnCollision = pauseOnCollision;
                UpdateImpactSubscription();
            }
        }

        private void RefreshPowerUpCache()
        {
#if TEST_ENABLED
            var powerUpCheats = GetPowerUpCheats();
            var catalog = powerUpCheats?.GetCatalog();
            if (catalog == null) return;

            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.InventoryKey)) continue;
                _PowerUpCountsCache[entry.InventoryKey] = powerUpCheats.GetPowerUpCount(entry.InventoryKey);
            }
#endif
        }

        private void DrawChannelAssignments()
        {
            EditorGUILayout.LabelField(Styles.EventChannelsHeader, EditorStyles.boldLabel);
            _RetryLevelChannel = (VoidEventChannel)EditorGUILayout.ObjectField(
                Styles.RetryLevelChannel,
                _RetryLevelChannel,
                typeof(VoidEventChannel),
                false);
            _RestartStageChannel = (VoidEventChannel)EditorGUILayout.ObjectField(
                Styles.RestartStageChannel,
                _RestartStageChannel,
                typeof(VoidEventChannel),
                false);

            var impactChannel = (KnifeImpactEventChannel)EditorGUILayout.ObjectField(
                Styles.KnifeImpactChannel,
                _KnifeImpactChannel,
                typeof(KnifeImpactEventChannel),
                false);
            if (!ReferenceEquals(impactChannel, _KnifeImpactChannel))
            {
                _KnifeImpactChannel = impactChannel;
                UpdateImpactSubscription();
            }
        }

        private void DrawGeneralCheats()
        {
            EditorGUILayout.LabelField(Styles.GeneralHeader, EditorStyles.boldLabel);
            if (GUILayout.Button(Styles.ClearProgressButton))
            {
#if TEST_ENABLED
                GetGeneralCheats()?.ClearProgress();
                RefreshPowerUpCache();
#endif
            }
        }

        private void DrawStageLevelCheats()
        {
            EditorGUILayout.LabelField(Styles.StageLevelHeader, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
#if TEST_ENABLED
                var stageCheats = GetStageLevelCheats();
#endif

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Styles.RestartLevelButton))
                    {
#if TEST_ENABLED
                        if (stageCheats != null) stageCheats.RestartLevel();
                        else RaiseChannel(_RetryLevelChannel, "Retry Level");
#else
                        RaiseChannel(_RetryLevelChannel, "Retry Level");
#endif
                    }

                    if (GUILayout.Button(Styles.RestartStageButton))
                    {
#if TEST_ENABLED
                        if (stageCheats != null) stageCheats.RestartStage();
                        else RaiseChannel(_RestartStageChannel, "Restart Stage");
#else
                        RaiseChannel(_RestartStageChannel, "Restart Stage");
#endif
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Styles.PrevLevelButton))
                    {
#if TEST_ENABLED
                        stageCheats?.PreviousLevel();
#endif
                    }

                    if (GUILayout.Button(Styles.NextLevelButton))
                    {
#if TEST_ENABLED
                        stageCheats?.NextLevel();
#endif
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _JumpToStageIndex = EditorGUILayout.IntField("Stage (1-based)", _JumpToStageIndex);
                    if (GUILayout.Button(Styles.JumpToStageButton))
                    {
#if TEST_ENABLED
                        stageCheats?.JumpToStage(_JumpToStageIndex - 1);
#endif
                    }
                }
            }
        }

        private void DrawEconomyCheats()
        {
            EditorGUILayout.LabelField(Styles.EconomyHeader, EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _AddCoinsAmount = EditorGUILayout.IntField("Amount", _AddCoinsAmount);
                if (GUILayout.Button(Styles.AddCoinsButton))
                {
#if TEST_ENABLED
                    var economyCheats = GetEconomyCheats();
                    
                    // Dynamically find the currency ID from the catalog
                    var catalog = Resources.Load<Game.Economy.Data.CurrencyTypeCatalog>("CurrencyTypeCatalog");
                    if (catalog == null)
                    {
                        // Try alternative path if not in Resources root
                        catalog = AssetDatabase.LoadAssetAtPath<Game.Economy.Data.CurrencyTypeCatalog>("Assets/Configurations/Currency/CurrencyTypeCatalog.asset");
                    }

                    var configs = catalog?.GetAllConfigs();
                    string currencyId = (configs != null && configs.Count > 0) ? configs[0].TypeId : "Coins";

                    economyCheats?.AddBalance(currencyId, _AddCoinsAmount);
#endif
                }
            }
        }

        private void DrawPowerUpCheats()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Styles.PowerUpsHeader, EditorStyles.boldLabel);
                if (GUILayout.Button(Styles.RefreshButton, GUILayout.Width(80)))
                {
                    RefreshPowerUpCache();
                }
            }

#if TEST_ENABLED
            var powerUpCheats = GetPowerUpCheats();
            var catalog = powerUpCheats?.GetCatalog();

            if (catalog != null)
            {
                foreach (var entry in catalog.Entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.InventoryKey)) continue;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (!_PowerUpCountsCache.TryGetValue(entry.InventoryKey, out int count))
                        {
                            count = powerUpCheats.GetPowerUpCount(entry.InventoryKey);
                            _PowerUpCountsCache[entry.InventoryKey] = count;
                        }

                        int newCount = EditorGUILayout.IntField(entry.DisplayName, count);
                        _PowerUpCountsCache[entry.InventoryKey] = newCount;

                        if (GUILayout.Button(Styles.SetButton, GUILayout.Width(50)))
                        {
                            powerUpCheats.SetPowerUpCount(entry.InventoryKey, newCount);
                            _PowerUpCountsCache[entry.InventoryKey] = newCount;
                        }
                    }
                }
            }
#endif
        }

#if TEST_ENABLED
        private GeneralCheats GetGeneralCheats() => ResolveService<GeneralCheats>();
        private StageLevelCheats GetStageLevelCheats() => ResolveService<StageLevelCheats>();
        private EconomyCheats GetEconomyCheats() => ResolveService<EconomyCheats>();
        private PowerUpCheats GetPowerUpCheats() => ResolveService<PowerUpCheats>();

        private T ResolveService<T>() where T : class
        {
            if (EditorApplication.isPlaying)
            {
                var scopes = Object.FindObjectsByType<VContainer.Unity.LifetimeScope>(FindObjectsSortMode.None);
                foreach (var scope in scopes)
                {
                    if (scope.Container == null) continue;
                    try
                    {
                        var service = scope.Container.Resolve<T>();
                        if (service != null) return service;
                    }
                    catch { /* Ignore */ }
                }
            }
            
            // Fallback for Edit Mode or when service not found in DI
            if (typeof(T) == typeof(GeneralCheats)) return new GeneralCheats(null, null) as T;
            if (typeof(T) == typeof(EconomyCheats)) return new EconomyCheats(null, null) as T;
            if (typeof(T) == typeof(PowerUpCheats)) return new PowerUpCheats(null) as T;
            
            return null;
        }
#endif

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
