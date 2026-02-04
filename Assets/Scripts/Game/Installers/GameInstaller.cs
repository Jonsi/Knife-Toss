using Common.Economy.Data;
using Common.Events;
using Game.Economy.Events;
using Game.Feedback.Audio.Data;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Loading;
using Game.Input;
using Game.KnifeThrower.Interfaces;
using Game.Settings;
using Game.UI.Services;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;
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

        protected override void Awake()
        {
            base.Awake();
#if UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateGameFlowInstaller());
            Install(builder, CreatePersistenceInstaller());
            if (_CurrencyTypeCatalog != null && _CurrencyBalanceChangedEventChannel != null)
            {
                Install(builder, CreateEconomyInstaller());
            }
            Install(builder, CreateRemoteConfigInstaller());

            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton);
            
            // Settings & Feedback
            builder.RegisterEntryPoint<SettingsService>().AsSelf();

            // Audio Core (Feedback listeners are in StageInstaller)
            builder.RegisterInstance(_SoundSettings);
            builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();

            // Register Event Channels (Shared/Persistence)
            builder.RegisterInstance(_ReturnToMenuEventChannel);

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

        private RemoteConfigInstaller CreateRemoteConfigInstaller()
        {
            return new RemoteConfigInstaller(_RemoteConfigSettings);
        }
    }
}