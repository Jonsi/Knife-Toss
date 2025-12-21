using Common.Events;
using Game.Feedback.Audio.Data;
using Game.Flow.Events;
using Game.Flow.Game;
using Game.Flow.Loading;
using Game.Input;
using Game.KnifeThrower.Interfaces;
using Game.Settings;
using Game.Feedback.Audio.Interfaces;
using Game.Feedback.Audio.Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Main composition root that configures dependency injection for general game systems.
    /// Persists across scenes.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private GameSettings _GameSettings;
        
        [Header("Event Channels")]
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;

        [Header("Feedback")]
        [SerializeField] private SoundSettings _SoundSettings;
        [SerializeField] private AudioPlayer _AudioPlayer;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateGameFlowInstaller());
            Install(builder, CreatePersistenceInstaller());

            // General Services
            builder.Register<InputAdapter>(Lifetime.Singleton).As<IPlayerThrowInputListener>();
            builder.Register<SceneLoaderService>(Lifetime.Singleton);

            // Audio Core (Feedback listeners are in StageInstaller)
            builder.RegisterInstance(_SoundSettings);
            builder.RegisterComponent(_AudioPlayer).As<IAudioPlayer>();

            // Register Event Channels (Shared/Persistence)
            builder.RegisterInstance(_ReturnToMenuEventChannel);

            // Loading flow - transitions to MainMenu after initialization
            builder.RegisterEntryPoint<LoadingController>();
        }

        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }

        private GameFlowInstaller CreateGameFlowInstaller()
        {
            return new GameFlowInstaller(_GameSettings);
        }

        private PersistenceInstaller CreatePersistenceInstaller()
        {
            return new PersistenceInstaller();
        }
    }
}
