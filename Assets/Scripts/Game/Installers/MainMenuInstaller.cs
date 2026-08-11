using Game.Persistence.Services;
using Game.UI.Common;
using Game.UI.HUD;
using Game.UI.Interfaces;
using Game.UI.MainMenu;
using Game.UI.Missions;
using Game.UI.Controllers;
using Game.UI.Data;
using Game.UI.Popups;
using Game.UI.Services;
using Game.Missions.Events;
using Game.Feedback.Events;
using Game.Store.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the MainMenu scene.
    /// Connects to the parent GameInstaller scope to access persistence and scene services.
    /// </summary>
    public class MainMenuInstaller : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private MainMenuController _MainMenuController;
        [SerializeField]
        [Tooltip("Optional. Applies themed menu particles at scene load; assign the root BackgroundEffectHolder instance.")]
        private MainMenuBackgroundEffect _MainMenuBackgroundEffect;
        [SerializeField] private CurrencyAccumulationConfig _CurrencyAccumulationConfig;
        [SerializeField] private DailyMissionPopup _DailyMissionPopup;
        [SerializeField] private MissionProgressChangedEventChannel _MissionProgressChangedEventChannel;

        [Header("Settings Presentation")]
        [SerializeField] private SettingsModalHost _SettingsModalHost;
        [SerializeField] private PrivacyPolicyContent _PrivacyPolicyContent;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ModalStackCoordinator>(Lifetime.Singleton)
                .As<IModalStackCoordinator>();

            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            builder.RegisterComponent(_MainMenuController);
            if (_MainMenuBackgroundEffect != null)
            {
                builder.RegisterComponent(_MainMenuBackgroundEffect);
            }

            builder.Register<KnifeSkinService>(Lifetime.Scoped).AsSelf();

            if (_DailyMissionPopup != null)
            {
                builder.RegisterComponent(_DailyMissionPopup);
            }

            builder.Register<PopupGamePauseController>(Lifetime.Singleton).As<IGamePauseController>();

            if (_SettingsModalHost != null && _SettingsModalHost.IsConfigured)
            {
                builder.RegisterComponent(_SettingsModalHost.SettingsPopup);
                builder.RegisterComponent(_SettingsModalHost.PrivacyPopup);
                builder.RegisterComponent(_SettingsModalHost.ReportErrorPopup);

                if (_PrivacyPolicyContent != null)
                {
                    builder.RegisterInstance(_PrivacyPolicyContent);
                }

                builder.Register<SettingsPopupController>(Lifetime.Singleton)
                    .As<ISettingsPopupPresenter>()
                    .AsSelf();
                builder.RegisterComponentInHierarchy<SettingsButtonView>();
            }

            builder.Register(resolver => new DailyMissionPopupController(
                _DailyMissionPopup,
                resolver.Resolve<Game.Missions.Daily.Services.DailyMissionProvider>(),
                _MissionProgressChangedEventChannel,
                resolver.Resolve<FeedbackEventChannel>()), Lifetime.Scoped).AsSelf();
            builder.RegisterComponentInHierarchy<CoinsUiDisplay>();
            builder.RegisterComponentInHierarchy<ModalInputBlockerPresenter>();
        }
    }
}
