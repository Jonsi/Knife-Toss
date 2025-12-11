using Common.Logging;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs game-level flow services and logging.
    /// </summary>
    public class GameFlowInstaller : IInstaller
    {
        private readonly GameSettings _Settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameFlowInstaller"/> class.
        /// </summary>
        /// <param name="settings">Game settings asset.</param>
        public GameFlowInstaller(GameSettings settings)
        {
            _Settings = settings;
        }

        /// <summary>
        /// Registers game-level flow services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Settings);
            builder.RegisterInstance(CreateLogger()).As<ILogger>();
            builder.Register<GameData>(Lifetime.Singleton);
            builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
            builder.Register(CreateGameFlowController, Lifetime.Singleton).AsImplementedInterfaces();
        }

        private GameFlowController CreateGameFlowController(IObjectResolver resolver)
        {
            return new GameFlowController(resolver.Resolve<IGameStateMachine>());
        }

        private ILogger CreateLogger()
        {
#if TEST_ENV
            ILogHandler handlerBase = new TestEnvLogHandler(Debug.unityLogger.logHandler, () => GameSettingsAccess.IsTestLoggingEnabled, includeStackTrace: false);
            return new Logger(handlerBase) { logEnabled = true };
#else
            return Debug.unityLogger;
#endif
        }
    }
}

