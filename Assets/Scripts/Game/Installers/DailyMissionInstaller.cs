using Common.Rewards.Interfaces;
using Game.Missions.Configs;
using Game.Missions.Daily.Services;
using Game.Missions.Services;
using Game.Persistence.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Registers the daily mission feature on top of the shared mission infrastructure.
    /// </summary>
    public class DailyMissionInstaller : IInstaller
    {
        private readonly MissionCatalog _DailyCatalog;

        public DailyMissionInstaller(MissionCatalog dailyCatalog)
        {
            _DailyCatalog = dailyCatalog;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.Register(resolver => new DailyMissionProvider(
                _DailyCatalog,
                resolver.Resolve<MissionTracker>(),
                resolver.Resolve<PlayerDataService>(),
                resolver.Resolve<IRewardGrantService>()), Lifetime.Singleton).AsSelf();

            builder.RegisterEntryPoint<DailyMissionProviderBootstrap>();
        }
    }
}
