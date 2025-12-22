using Game.DI;
using Common.Logging;
using Game.Flow.Game;
using Game.Flow.Interfaces;
using Game.Settings;
using Game.UI.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs game-level flow services and logging.
    /// </summary>
    [CreateAssetMenu(fileName = "GameFlowInstaller", menuName = "Knife Toss/Installers/Game Flow")]
    public class GameFlowInstallerSO : BaseInstallerSO
    {
        [Header("Settings")]
        [SerializeField] private GameSettings _Settings;

        /// <summary>
        /// Registers game-level flow services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Settings);
            builder.RegisterInstance(CreateLogger()).As<ILogger>();
            builder.Register<SettingsService>(Lifetime.Singleton);
            builder.Register<GameData>(Lifetime.Singleton);
            builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
            builder.RegisterEntryPoint(CreateGameFlowController, Lifetime.Singleton);
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

