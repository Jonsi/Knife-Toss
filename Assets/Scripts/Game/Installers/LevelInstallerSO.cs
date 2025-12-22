using Game.DI;
using Common.Events;
using Game.App.Flow.Events;
using Game.Flow.Interfaces;
using Game.Flow.Level;
using Game.Flow.Level.States;
using Game.Level.Data;
using Game.Level.Interfaces;
using Game.Level.Spawning;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs level feature services.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelInstaller", menuName = "Knife Toss/Installers/Level")]
    public class LevelInstallerSO : BaseInstallerSO
    {
        [Header("Event Channels")]
        [SerializeField] private LevelCompletedEventChannel _CompletedChannel;
        [SerializeField] private LevelFailedEventChannel _FailedChannel;

        /// <summary>
        /// Registers level feature services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<LevelSpawner>(Lifetime.Singleton)
                .WithParameter<Transform>(resolver => resolver.Resolve<LevelSceneReferences>().LevelRoot)
                .As<ILevelSpawner>();
            builder.Register<LevelData>(Lifetime.Singleton);
            builder.Register<LevelStateMachine>(Lifetime.Singleton);
            builder.Register<LevelFlowController>(Lifetime.Singleton).As<ILevelFlowController>();
            builder.RegisterInstance(_CompletedChannel).AsSelf().As<IEventChannel<LevelData>>();
            builder.RegisterInstance(_FailedChannel).AsSelf().As<IEventChannel<LevelData>>();
        }
    }
}

