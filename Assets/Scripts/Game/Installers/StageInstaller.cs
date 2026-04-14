using Common.Ads.Data;
using Common.Ads.Events;
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
using Game.Missions.Services;
using Game.Settings;
using Game.UI.Common;
using Game.UI.Controllers;
using Game.UI.Data;
using Game.UI.Popups;
using Common.Rewards.Events;
using Common.Rewards.Interfaces;
using Game.Persistence.Services;
using Game.Pooling;
using Game.Rewards.Services;
using Game.Store.Data;
using Game.Store.Events;
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
        [SerializeField]
        [Tooltip("Throw timing, arcs, and related tuning for the knife thrower.")]
        private KnifeThrowConfigAsset _KnifeThrowConfig;

        [SerializeField]
        [Tooltip("Master list of stages and levels used by the stage flow and level catalog.")]
        private StagesCollection _StagesConfig;

        [SerializeField]
        [Tooltip("Shared prefabs for targets, knives, VFX, and pooling.")]
        private PrefabsConfiguration _PrefabsConfiguration;

        [SerializeField]
        [Tooltip("Definitions for power-ups available during the stage.")]
        private PowerUpCatalog _PowerUpCatalog;

        [SerializeField]
        [Tooltip("Policy text asset for the privacy popup from settings.")]
        private PrivacyPolicyContent _PrivacyPolicyContent;

        [Header("Scene References")]
        [SerializeField]
        [Tooltip("Scene knife thrower used for input and throws.")]
        private KnifeThrowerView _KnifeThrowerView;

        [SerializeField]
        [Tooltip("Power-up thrower UI / behavior anchor in the stage scene.")]
        private Game.PowerUps.Views.PowerUpThrowerView _PowerUpThrowerView;

        [SerializeField]
        [Tooltip("Parent transform where level content is spawned.")]
        private Transform _LevelRoot;

        [SerializeField]
        [Tooltip("Fail overlay with retry, restart, continue, and optional power-up store cards.")]
        private StageFailPopup _StageFailPopup;

        [SerializeField]
        [Tooltip("In-game settings overlay (audio, privacy, report).")]
        private SettingsPopup _SettingsPopup;

        [SerializeField]
        [Tooltip("Full-screen privacy policy view.")]
        private PrivacyPolicyPopup _PrivacyPopup;

        [SerializeField]
        [Tooltip("Bug / feedback report flow popup.")]
        private ReportErrorPopup _ReportErrorPopup;

        [SerializeField]
        [Tooltip("Stage HUD controller: fail popup, level flow hooks, etc.")]
        private UIController _UIController;

        [Header("Rewards")]
        [SerializeField]
        [Tooltip("Raised after rewards are granted; subscribe for analytics or HUD.")]
        private RewardGrantedEventChannel _RewardGrantedEventChannel;

        [SerializeField]
        [Tooltip("Popup that lists granted rewards and waits for Collect (optional; null skips UI).")]
        private RewardPopup _RewardPopup;

        [Header("Event Channels (Must match GameInstaller)")]
        [SerializeField]
        [Tooltip("Physics collision events routed to gameplay handlers.")]
        private CollisionEventChannel _CollisionEventChannel;

        [SerializeField]
        [Tooltip("Knife hit target events (stuck, bounce, etc.).")]
        private KnifeImpactEventChannel _KnifeImpactEventChannel;

        [SerializeField]
        [Tooltip("Fired when a knife is thrown.")]
        private KnifeThrownEventChannel _KnifeThrownEventChannel;

        [SerializeField]
        [Tooltip("Collectible hit for currency and persistence.")]
        private CollectibleHitEventChannel _CollectibleHitEventChannel;

        [SerializeField]
        [Tooltip("Current level completed successfully.")]
        private LevelCompletedEventChannel _LevelCompletedEventChannel;

        [SerializeField]
        [Tooltip("Current level failed (knife miss, etc.).")]
        private LevelFailedEventChannel _LevelFailedEventChannel;

        [SerializeField]
        [Tooltip("All levels in the current stage finished.")]
        private StageCompletedEventChannel _StageCompletedEventChannel;

        [SerializeField]
        [Tooltip("Player chose to leave the stage to the main menu.")]
        private ReturnToMenuEventChannel _ReturnToMenuEventChannel;

        [SerializeField]
        [Tooltip("A level has loaded and play is about to start.")]
        private LevelStartedEventChannel _LevelStartedEventChannel;

        [SerializeField]
        [Tooltip("Fail flow UI or logic has started (e.g. before fail popup settles).")]
        private VoidEventChannel _LevelFailFlowStartedEventChannel;

        [SerializeField]
        [Tooltip("Retry button on fail: reload current level.")]
        private VoidEventChannel _RetryLevelClickedChannel;

        [SerializeField]
        [Tooltip("Restart button on fail: reload whole stage from level 1.")]
        private VoidEventChannel _RestartStageClickedChannel;

        [SerializeField]
        [Tooltip("A power-up was activated.")]
        private PowerUpActivatedEventChannel _PowerUpActivatedEventChannel;

        [SerializeField]
        [Tooltip("A power-up was consumed / used.")]
        private PowerUpUsedEventChannel _PowerUpUsedEventChannel;

        [SerializeField]
        [Tooltip("A power-up activation was cancelled.")]
        private PowerUpCancelledEventChannel _PowerUpCancelledEventChannel;

        [Header("Economy (optional; use when stage runs standalone or parent has none)")]
        [SerializeField]
        [Tooltip("Currency type metadata and display defaults.")]
        private CurrencyTypeCatalog _CurrencyTypeCatalog;

        [SerializeField]
        [Tooltip("Raised when a currency balance changes (HUD, accumulation FX).")]
        private CurrencyBalanceChangedEventChannel _CurrencyBalanceChangedEventChannel;

        [SerializeField]
        [Tooltip("Store items for fail-screen power-up cards and in-stage purchases.")]
        private StoreCatalog _StoreCatalog;

        [SerializeField]
        [Tooltip("Tuning for coin fly / accumulation animations toward the HUD.")]
        private CurrencyAccumulationConfig _CurrencyAccumulationConfig;

        [SerializeField]
        [Tooltip("Purchase flow started (UI lock, analytics).")]
        private PurchaseStartedEventChannel _PurchaseStartedEventChannel;

        [SerializeField]
        [Tooltip("Purchase completed successfully.")]
        private PurchaseCompletedEventChannel _PurchaseCompletedEventChannel;

        [SerializeField]
        [Tooltip("Purchase failed or was rejected.")]
        private PurchaseFailedEventChannel _PurchaseFailedEventChannel;

        [SerializeField]
        [Tooltip("Rewarded ad completed successfully.")]
        private AdCompletedEventChannel _AdCompletedEventChannel;

        [Header("Ads (optional)")]
        [SerializeField]
        [Tooltip("Continue after fail: watch rewarded ad to retry the same level.")]
        private VoidEventChannel _ContinueRequestedChannel;

        [SerializeField]
        [Tooltip("Rewarded placement for continue (reward optional; may be null for continue-only).")]
        private RewardedPlacement _ContinuePlacement;

        [SerializeField]
        [Tooltip("Interstitial placement after fail or stage complete (policy in AdSettings).")]
        private InterstitialPlacement _InterstitialPlacement;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<VContainerRewardGrantContext>(Lifetime.Singleton).As<IRewardGrantContext>();

            var hasStorePurchaseFlow =
                _StoreCatalog != null &&
                _PurchaseStartedEventChannel != null &&
                _PurchaseCompletedEventChannel != null &&
                _PurchaseFailedEventChannel != null;

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

            builder.RegisterEntryPoint<MissionProgressRouter>()
                .WithParameter(_LevelCompletedEventChannel)
                .WithParameter(_LevelFailedEventChannel)
                .WithParameter(_StageCompletedEventChannel)
                .WithParameter(_CollectibleHitEventChannel)
                .WithParameter(_PowerUpUsedEventChannel)
                .WithParameter(_PurchaseCompletedEventChannel)
                .WithParameter(_AdCompletedEventChannel);

            Install(builder, new RewardInstaller(_RewardGrantedEventChannel, _RewardPopup));

            builder.RegisterEntryPoint<StageRewardConnector>();

            if (_PrefabsConfiguration != null)
            {
                builder.RegisterInstance(_PrefabsConfiguration).AsSelf();
            }

            if (hasStorePurchaseFlow)
            {
                Install(builder, new StoreInstaller(
                    _StoreCatalog,
                    _PurchaseStartedEventChannel,
                    _PurchaseCompletedEventChannel,
                    _PurchaseFailedEventChannel));
            }

            if (_CurrencyAccumulationConfig == null)
            {
                _CurrencyAccumulationConfig = Resources.Load<CurrencyAccumulationConfig>("Configurations/Systems/UI/CurrencyAccumulationConfig");
            }

            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            if (!hasStorePurchaseFlow)
            {
                builder.Register<KnifeSkinService>(Lifetime.Scoped).AsSelf();
            }

            if (_CurrencyTypeCatalog != null && _CurrencyBalanceChangedEventChannel != null)
            {
                Install(builder, new EconomyInstaller(_CurrencyTypeCatalog, _CurrencyBalanceChangedEventChannel));
            }

            if (_ContinuePlacement != null && _ContinueRequestedChannel != null)
            {
                builder.Register(resolver => new AdRewardController(
                    _ContinueRequestedChannel,
                    _RetryLevelClickedChannel,
                    _RestartStageClickedChannel,
                    resolver.Resolve<LevelFailedEventChannel>(),
                    _CurrencyBalanceChangedEventChannel,
                    resolver.Resolve<Game.Economy.Interfaces.ICurrencyService>(),
                    resolver.Resolve<Game.Level.Interfaces.ILevelCatalog>(),
                    resolver.Resolve<Game.Flow.Stage.StageData>(),
                    _ContinuePlacement,
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
