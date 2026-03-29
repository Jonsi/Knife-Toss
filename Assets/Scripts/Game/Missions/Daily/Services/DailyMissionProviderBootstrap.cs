using System;
using VContainer.Unity;

namespace Game.Missions.Daily.Services
{
    /// <summary>
    /// Eagerly initializes the daily mission provider from the container root.
    /// </summary>
    public class DailyMissionProviderBootstrap : IInitializable, IDisposable
    {
        private readonly DailyMissionProvider _Provider;

        public DailyMissionProviderBootstrap(DailyMissionProvider provider)
        {
            _Provider = provider;
        }

        public void Initialize()
        {
            _Provider.Initialize();
        }

        public void Dispose()
        {
            _Provider.Dispose();
        }
    }
}
