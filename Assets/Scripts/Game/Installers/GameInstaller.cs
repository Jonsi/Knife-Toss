using Game.Ads.Installers;
using Game.Economy.Data;
using Common.Events;
using Game.Economy.Events;
using Game.Feedback.Audio.Data;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Loading;
using Game.Input;
using Game.KnifeThrower.Interfaces;
using Game.Missions.Configs;
using Game.Missions.Events;
using Game.Settings;
using Game.UI.Services;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Systems;
using Common.Rewards.Interfaces;
using Game.Rewards.Services;
using Game.Store.Data;
using Game.Store.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Common.Analytics;
using Common.Services.RemoteConfig;

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
        [SerializeField] private SoundSettings _SoundSettings;
        [SerializeField] private AudioPlayer _AudioPlayer;

        [Header("Services")]
        [SerializeField] private Common.Services.RemoteConfig.RemoteConfigSettings _RemoteConfigSettings;
        [SerializeField] private AnalyticsSettings _AnalyticsSettings;
        [SerializeField] private StoreCatalog _StoreCatalog;

        [Header("Missions")]
        [SerializeField] private MissionCatalog _DailyMissionCatalog;
        [SerializeField] private MissionProgressChangedEventChannel _MissionProgressChangedEventChannel;
        [SerializeField] private MissionCompletedEventChannel _MissionCompletedEventChannel;

        [Header("Ads")]
        [SerializeField] private AdInstaller _AdInstaller;

        protected override void Awake()
        {
            base.Awake();
#if UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }

        protected override void Configure(IContainerBuilder builder)
        {
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
            Install(builder, CreateRemoteConfigInstaller());
            Install(builder, CreateAnalyticsInstaller());

            if (_AdInstaller != null)
                Install(builder, _AdInstaller);

            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton);
            builder.Register<KnifeSkinService>(Lifetime.Singleton).AsSelf();

            builder.RegisterEntryPoint<SettingsService>().AsSelf();

            builder.RegisterInstance(_SoundSettings);
            builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();

            builder.RegisterInstance(_ReturnToMenuEventChannel);
            if (_StoreCatalog != null)
            {
                builder.RegisterInstance(_StoreCatalog).AsSelf();
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
