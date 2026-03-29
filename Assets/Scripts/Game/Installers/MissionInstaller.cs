using Game.Missions.Events;
using Game.Missions.Services;
using Game.Persistence.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Registers shared mission infrastructure.
    /// </summary>
    public class MissionInstaller : IInstaller
    {
        private readonly MissionProgressChangedEventChannel _ProgressChannel;
        private readonly MissionCompletedEventChannel _CompletedChannel;

        public MissionInstaller(
            MissionProgressChangedEventChannel progressChannel,
            MissionCompletedEventChannel completedChannel)
        {
            _ProgressChannel = progressChannel;
            _CompletedChannel = completedChannel;
        }

        public void Install(IContainerBuilder builder)
        {
            if (_ProgressChannel != null)
            {
                builder.RegisterInstance(_ProgressChannel);
            }

            if (_CompletedChannel != null)
            {
                builder.RegisterInstance(_CompletedChannel);
            }

            builder.Register(resolver => new MissionTracker(
                _ProgressChannel,
                _CompletedChannel,
                resolver.Resolve<PlayerDataService>()), Lifetime.Singleton).AsSelf();
        }
    }
}
