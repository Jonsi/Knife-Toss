using Common.Ads.Data;
using Common.Ads.Interfaces;
using Common.Collision.Events;
using Game.Ads.Controllers;
using Game.Ads.Data;
using Game.Economy.Data;
using Common.Events;
using Game.Economy.Events;
using Game.Entities.Collectible.Events;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Flow.Events;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Events;
using Game.Level.Catalogs;
using Game.Settings;
using Game.UI.Common;
using Game.UI.Controllers;
using Game.UI.Data;
using Game.UI.Popups;
using Game.Persistence.Services;
using Game.Pooling;
using Game.Store.Data;
using Game.Store.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Game.PowerUps.Configs;
using Game.PowerUps.Events;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the StagePlay scene.
    /// Installs stage-specific services and inherits general services from GameInstaller.
    /// </summary>
    public class StageInstaller : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private KnifeThrowConfigAsset _KnifeThrowConfig;
        [SerializeField] private StagesCollection _StagesConfig;
        [SerializeField] private PrefabsConfiguration _PrefabsConfiguration;
        [SerializeField] private PowerUpCatalog _PowerUpCatalog;
        [SerializeField] private PrivacyPolicyContent _PrivacyPolicyContent;
        
        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;
        [SerializeField] private Game.PowerUps.Views.PowerUpThrowerView _PowerUpThrowerView;
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private StageFailPopup _StageFailPopup;
        [SerializeField] private SettingsPopup _SettingsPopup;
        [SerializeField] private PrivacyPolicyPopup _PrivacyPopup;
        [SerializeField] private ReportErrorPopup _ReportErrorPopup;
        [SerializeField] private UIController _UIController;

        [Header("Event Channels (Must match GameInstaller)")]
        [SerializeField] private CollisionEventChannel _CollisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel _KnifeImpactEventChannel;
        [SerializeField] private KnifeThrownEventChannel _KnifeThrownEventChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitEventChannel;
        
        [SerializeField] private LevelCompletedEventChannel _LevelCompletedEventChannel;
        [SerializeField] private LevelFailedEventChannel _LevelFailedEventChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedEventChannel;
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedEventChannel;
        [SerializeField] private VoidEventChannel _LevelFailFlowStartedEventChannel;

        [SerializeField] private VoidEventChannel _RetryLevelClickedChannel;
        [SerializeField] private VoidEventChannel _RestartStageClickedChannel;
        
        [SerializeField] private PowerUpActivatedEventChannel _PowerUpActivatedEventChannel;
        [SerializeField] private PowerUpUsedEventChannel _PowerUpUsedEventChannel;
        [SerializeField] private PowerUpCancelledEventChannel _PowerUpCancelledEventChannel;

        [Header("Economy (optional; use when stage runs standalone or parent has none)")]
        [SerializeField] private CurrencyTypeCatalog _CurrencyTypeCatalog;
        [SerializeField] private CurrencyBalanceChangedEventChannel _CurrencyBalanceChangedEventChannel;
        [SerializeField] private StoreCatalog _StoreCatalog;
        [SerializeField] private CurrencyAccumulationConfig _CurrencyAccumulationConfig;

        [Header("Ads (optional)")]
        [SerializeField] private VoidEventChannel _ContinueRequestedChannel;
        [SerializeField] private RewardedPlacement _ContinuePlacement;
        [SerializeField] private InterstitialPlacement _InterstitialPlacement;

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateTargetInstaller());
            Install(builder, CreateKnifeInstaller());
            Install(builder, CreateCollectibleInstaller());
            Install(builder, CreatePowerUpInstaller());
            Install(builder, CreateLevelInstaller());
            Install(builder, CreateStageFlowInstaller());
            Install(builder, CreateCollisionInstaller());
            Install(builder, CreateVFXInstaller());
            Install(builder, CreateUIInstaller());
            Install(builder, CreateStageAudioInstaller());
            
            // Connect Stage Events to Global Persistence
            builder.RegisterEntryPoint<StagePersistenceConnector>()
                .WithParameter(_LevelCompletedEventChannel)
                .WithParameter(_StageCompletedEventChannel)
                .WithParameter(_CollectibleHitEventChannel);

            if (_PrefabsConfiguration != null)
            {
                builder.RegisterInstance(_PrefabsConfiguration).AsSelf();
            }

            if (_StoreCatalog != null)
            {
                builder.RegisterInstance(_StoreCatalog).AsSelf();
            }

            if (_CurrencyAccumulationConfig == null)
            {
                _CurrencyAccumulationConfig = Resources.Load<CurrencyAccumulationConfig>("Configurations/Systems/UI/CurrencyAccumulationConfig");
            }

            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            builder.Register<KnifeSkinService>(Lifetime.Scoped).AsSelf();

            if (_CurrencyTypeCatalog != null && _CurrencyBalanceChangedEventChannel != null)
            {
                Install(builder, new EconomyInstaller(_CurrencyTypeCatalog, _CurrencyBalanceChangedEventChannel));
            }

            if (_ContinuePlacement != null && _ContinueRequestedChannel != null)
            {
                builder.Register(resolver => new AdRewardController(
                    resolver.Resolve<IAdService>(),
                    resolver.Resolve<AdSettings>(),
                    _ContinuePlacement,
                    resolver,
                    _ContinueRequestedChannel,
                    _RetryLevelClickedChannel,
                    _RestartStageClickedChannel,
                    resolver.Resolve<LevelFailedEventChannel>(),
                    resolver.Resolve<StageFailPopup>()
                ), Lifetime.Scoped).AsImplementedInterfaces();
            }

            if (_InterstitialPlacement != null)
            {
                builder.Register(resolver => new InterstitialAdTrigger(
                    resolver.Resolve<IAdService>(),
                    _InterstitialPlacement,
                    resolver.Resolve<LevelFailedEventChannel>(),
                    resolver.Resolve<StageCompletedEventChannel>(),
                    _RetryLevelClickedChannel,
                    _RestartStageClickedChannel
                ), Lifetime.Scoped).AsImplementedInterfaces();
            }
        }

        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }

        private TargetInstaller CreateTargetInstaller()
        {
            return new TargetInstaller(_PrefabsConfiguration);
        }

        private KnifeInstaller CreateKnifeInstaller()
        {
            return new KnifeInstaller(
                _KnifeThrowConfig,
                _KnifeThrowerView,
                _PowerUpThrowerView,
                _PrefabsConfiguration,
                _KnifeImpactEventChannel,
                _KnifeThrownEventChannel);
        }

        private CollectibleInstaller CreateCollectibleInstaller()
        {
            return new CollectibleInstaller(_CollectibleHitEventChannel);
        }

        private PowerUpInstaller CreatePowerUpInstaller()
        {
            return new PowerUpInstaller(
                _PowerUpCatalog,
                _PowerUpActivatedEventChannel,
                _PowerUpUsedEventChannel,
                _PowerUpCancelledEventChannel,
                _LevelStartedEventChannel);
        }

        private LevelInstaller CreateLevelInstaller()
        {
            return new LevelInstaller(
                _LevelRoot,
                _LevelCompletedEventChannel,
                _LevelFailedEventChannel,
                _LevelFailFlowStartedEventChannel);
        }

        private StageFlowInstaller CreateStageFlowInstaller()
        {
            return new StageFlowInstaller(
                _StagesConfig,
                _StageCompletedEventChannel,
                _ReturnToMenuEventChannel,
                _RetryLevelClickedChannel,
                _RestartStageClickedChannel,
                _LevelStartedEventChannel);
        }

        private CollisionInstaller CreateCollisionInstaller()
        {
            return new CollisionInstaller(_CollisionEventChannel);
        }

        private VFXInstaller CreateVFXInstaller()
        {
            return new VFXInstaller(_PrefabsConfiguration, _KnifeImpactEventChannel);
        }

        private UIInstaller CreateUIInstaller()
        {
            return new UIInstaller(
                _UIController,
                _StageFailPopup,
                _SettingsPopup,
                _PrivacyPopup,
                _ReportErrorPopup,
                _PrivacyPolicyContent);
        }

        private StageAudioInstaller CreateStageAudioInstaller()
        {
            return new StageAudioInstaller(
                _KnifeThrownEventChannel,
                _KnifeImpactEventChannel,
                _CollectibleHitEventChannel,
                _LevelCompletedEventChannel,
                _LevelFailedEventChannel);
        }
    }
}
