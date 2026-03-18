using Common.Logging;
using Game.Flow.Game;
using Game.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs global services that persist across all scenes.
    /// Does not include gameplay-specific services.
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
        /// Registers global services.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            GameSettingsAccess.Initialize(_Settings);
            builder.RegisterInstance(_Settings);
            ILogger logger = CreateLogger();
            GameLogger.SetSharedLogger(logger);
            builder.RegisterInstance(logger).As<ILogger>();
            builder.Register<GameData>(Lifetime.Singleton);
        }

        private ILogger CreateLogger()
        {
#if TEST_ENABLED
            ILogHandler coloredHandler = new ColoredLogHandler(Debug.unityLogger.logHandler);
            ILogHandler gatedHandler = new TestEnvLogHandler(coloredHandler, () => GameSettingsAccess.IsTestLoggingEnabled, includeStackTrace: false);
            return new GameLogger(gatedHandler)
            {
                logEnabled = true,
                filterLogType = Debug.unityLogger.filterLogType
            };
#else
            return Debug.unityLogger;
#endif
        }
    }
}
