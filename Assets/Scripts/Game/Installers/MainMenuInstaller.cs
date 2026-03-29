using Game.Persistence.Services;
using Game.UI.Common;
using Game.UI.HUD;
using Game.UI.Interfaces;
using Game.UI.MainMenu;
using Game.UI.Missions;
using Game.UI.Services;
using Game.Missions.Events;
using Game.UI.Stages;
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
        [SerializeField] private CurrencyAccumulationConfig _CurrencyAccumulationConfig;
        [SerializeField] private DailyMissionPopup _DailyMissionPopup;
        [SerializeField] private MissionProgressChangedEventChannel _MissionProgressChangedEventChannel;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            builder.RegisterComponent(_MainMenuController);
            if (_DailyMissionPopup != null)
            {
                builder.RegisterComponent(_DailyMissionPopup);
            }

            builder.Register<PopupGamePauseController>(Lifetime.Singleton).As<IGamePauseController>();
            builder.Register(resolver => new DailyMissionPopupController(
                _DailyMissionPopup,
                resolver.Resolve<Game.Missions.Daily.Services.DailyMissionProvider>(),
                _MissionProgressChangedEventChannel), Lifetime.Scoped).AsSelf();
            builder.RegisterComponentInHierarchy<StageListController>();
            builder.RegisterComponentInHierarchy<CoinsUiDisplay>();
        }
    }
}
