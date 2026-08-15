using Game.Ads.Installers;
using Game.Ads.Services;
using Game.Economy.Data;
using Common.IAP.Installers;
using Common.IAP.Interfaces;
using Common.Events;
using Game.Economy.Events;
using Game.Feedback.Audio.Data;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Loading;
using Game.Input;
using Game.KnifeThrower.Interfaces;
using Game.Missions.Configs;
using Game.Missions.Events;
using Game.Pooling;
using Game.Settings;
using Game.UI.Services;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Systems;
using Game.Feedback.Audio.Services;
using Game.Feedback.Haptics;
using Game.Feedback.Presentation;
using Game.Feedback.VFX.Services;
using Common.Rewards.Interfaces;
using Game.Rewards.Services;
using Game.Store.Data;
using Game.Store.Services;
using Game.IAP.Services;
#if TEST_ENABLED
using Common.IAP.Data;
using Game.IAP.Mock.Installers;
using Game.IAP.UI;
#endif
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Level.Configurations.Stage;
using Game.Level.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Common.AssetLoading;
using Common.AssetLoading.UnityAddressables;
using Common.Analytics;
using Common.Services.RemoteConfig;
using System;

namespace Game.Installers
{
    /// <summary>
    /// Main composition root that configures dependency injection for general game systems.
    /// Persists across scenes.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private GameSettings _GameSettings;

        [Header("Event Channels")]
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;

        [Header("Economy")]
        [SerializeField] private CurrencyTypeCatalog _CurrencyTypeCatalog;
        [SerializeField] private CurrencyBalanceChangedEventChannel _CurrencyBalanceChangedEventChannel;

        [Header("Feedback")]
        [SerializeField] private FeedbackEventChannel _FeedbackEventChannel;
        [SerializeField] private FeedbackOutputCatalog _FeedbackOutputCatalog;
        [SerializeField] private SoundSettings _SoundSettings;
        [SerializeField] private AudioPlayer _AudioPlayer;

        [Header("Services")]
        [SerializeField] private Common.Services.RemoteConfig.RemoteConfigSettings _RemoteConfigSettings;
        [SerializeField] private AnalyticsSettings _AnalyticsSettings;
        [SerializeField] private StoreCatalog _StoreCatalog;
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;

        [Header("Missions")]
        [SerializeField] private MissionCatalog _DailyMissionCatalog;
        [SerializeField] private MissionProgressChangedEventChannel _MissionProgressChangedEventChannel;
        [SerializeField] private MissionCompletedEventChannel _MissionCompletedEventChannel;

        [Header("Ads")]
        [SerializeField] private AdInstaller _AdInstaller;

        [Header("Stages")]
        [SerializeField] private StagesCollectionComposite _StagesCollectionComposite;
        [SerializeField] private AssetReferenceStagesCollectionComposite _RemoteStagesCollectionCompositeReference;

        [Header("IAP")]
        [SerializeField] private IapInstaller _IapInstaller;
#if TEST_ENABLED
        [SerializeField] private MockIapSettings _MockIapSettings;
        [SerializeField] private MockIapPopupBridge _MockIapPopupBridge;
#endif

        protected override void Awake()
        {
            base.Awake();
#if UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateRequiredSetup();

            builder.Register<VContainerRewardGrantContext>(Lifetime.Singleton).As<IRewardGrantContext>();

            Install(builder, CreateGameFlowInstaller());
            Install(builder, CreatePersistenceInstaller());
            if (_CurrencyTypeCatalog != null && _CurrencyBalanceChangedEventChannel != null)
            {
                Install(builder, CreateEconomyInstaller());
            }
            Install(builder, CreateMissionInstaller());
            Install(builder, CreateDailyMissionInstaller());
            Install(builder, new RewardInstaller(null, null));
            builder.Register<StageRewardClaimService>(Lifetime.Singleton);
            Install(builder, CreateRemoteConfigInstaller());
            Install(builder, CreateAnalyticsInstaller());

            if (_IapInstaller != null)
            {
                builder.Register<PlayerDataIapEntitlementStore>(Lifetime.Singleton).As<IIapEntitlementStore>().AsSelf();
                Install(builder, _IapInstaller);
                builder.Register<IapAnalyticsTelemetryListener>(Lifetime.Singleton).As<Common.IAP.Interfaces.IIapTelemetryListener>();
                builder.Register<IapAdEntitlementGate>(Lifetime.Singleton).As<Common.Ads.Interfaces.IAdEntitlementGate>();
                builder.Register<MainMenuRemoveAdsFlow>(Lifetime.Singleton).AsSelf();
            }

            if (_AdInstaller != null)
                Install(builder, _AdInstaller);

            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton);

            builder.Register<AddressableAssetLoader>(Lifetime.Singleton).As<IAssetLoader>();
            builder.Register<CatalogUpdateService>(Lifetime.Singleton);
            builder.Register<RemoteCompositeLoader>(Lifetime.Singleton).AsSelf()
                .WithParameter(_RemoteStagesCollectionCompositeReference);
            builder.Register<StagesCollectionCatalogProvider>(Lifetime.Singleton)
                .As<IStagesCollectionCatalog>()
                .WithParameter(_StagesCollectionComposite);
            builder.Register<WorldAssetService>(Lifetime.Singleton).AsSelf();
            builder.Register<StagesCollectionResolver>(Lifetime.Singleton).AsSelf();
            builder.Register<BackgroundEffectService>(Lifetime.Singleton).AsSelf();
#if UNITY_EDITOR
            builder.RegisterEntryPoint<EditorRemoteBootstrapStartable>();
#endif

            builder.RegisterEntryPoint<SettingsService>().AsSelf();

            builder.RegisterInstance(_SoundSettings);
            builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();

            if (_CurrencyBalanceChangedEventChannel != null)
            {
                builder.RegisterEntryPoint<CurrencyAudioFeedbackService>();
            }

            if (_FeedbackEventChannel != null)
            {
                builder.RegisterInstance(_FeedbackEventChannel)
                    .AsSelf()
                    .As<IEventChannel<FeedbackEventData>>();

                if (_FeedbackOutputCatalog != null)
                {
                    builder.RegisterInstance(_FeedbackOutputCatalog);
                    builder.Register<UnityFeedbackPresentationClock>(Lifetime.Singleton)
                        .As<IFeedbackPresentationClock>();
                    builder.Register<FeedbackVfxSignal>(Lifetime.Singleton)
                        .AsSelf()
                        .As<IFeedbackVfxPlayer>();
                    builder.Register<CandyCodedHapticFeedbackPlayer>(Lifetime.Singleton)
                        .As<IHapticFeedbackPlayer>();
                    builder.RegisterEntryPoint<FeedbackAudioPresenter>();
                    builder.RegisterEntryPoint<FeedbackVfxPresenter>();
                    builder.RegisterEntryPoint<FeedbackHapticPresenter>();
                }
            }

            builder.RegisterInstance(_ReturnToMenuEventChannel);
            builder.RegisterInstance(_StoreCatalog).AsSelf();

#if TEST_ENABLED
            if (_IapInstaller != null)
            {
                MockIapSettings mockSettings = _MockIapSettings != null
                    ? _MockIapSettings
                    : ScriptableObject.CreateInstance<MockIapSettings>();
                Install(builder, new IapMockInstaller(mockSettings, _MockIapPopupBridge));
            }
#endif

            if (_PrefabsConfiguration != null)
            {
                builder.RegisterInstance(_PrefabsConfiguration).AsSelf();
            }

#if TEST_ENABLED
            builder.Register<Game.Debugging.GeneralCheats>(Lifetime.Singleton).AsSelf();
            builder.Register<Game.Debugging.EconomyCheats>(Lifetime.Singleton).AsSelf();
            builder.Register<Game.Debugging.PowerUpCheats>(Lifetime.Singleton).AsSelf();
#endif
        }

        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }

        private void ValidateRequiredSetup()
        {
            if (_IapInstaller == null)
            {
                throw new InvalidOperationException("GameInstaller requires IapInstaller for production IAP flow.");
            }

            if (_StoreCatalog == null)
            {
                throw new InvalidOperationException("GameInstaller requires StoreCatalog for production IAP flow.");
            }

            if (_StagesCollectionComposite == null)
            {
                throw new InvalidOperationException("GameInstaller requires StagesCollectionComposite.");
            }

            if (_RemoteStagesCollectionCompositeReference == null ||
                !_RemoteStagesCollectionCompositeReference.RuntimeKeyIsValid())
            {
                throw new InvalidOperationException(
                    "GameInstaller requires a valid AssetReferenceStagesCollectionComposite for remote composite loads.");
            }
        }

        private GameFlowInstaller CreateGameFlowInstaller()
        {
            return new GameFlowInstaller(_GameSettings);
        }

        private PersistenceInstaller CreatePersistenceInstaller()
        {
            return new PersistenceInstaller();
        }

        private EconomyInstaller CreateEconomyInstaller()
        {
            return new EconomyInstaller(_CurrencyTypeCatalog, _CurrencyBalanceChangedEventChannel);
        }

        private MissionInstaller CreateMissionInstaller()
        {
            return new MissionInstaller(_MissionProgressChangedEventChannel, _MissionCompletedEventChannel);
        }

        private DailyMissionInstaller CreateDailyMissionInstaller()
        {
            return new DailyMissionInstaller(_DailyMissionCatalog);
        }

        private RemoteConfigInstaller CreateRemoteConfigInstaller()
        {
            return new RemoteConfigInstaller(_RemoteConfigSettings);
        }

        private AnalyticsInstaller CreateAnalyticsInstaller()
        {
            return new AnalyticsInstaller(_AnalyticsSettings);
        }
    }
}
