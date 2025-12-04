using System;
using KnifeToss.Application;
using KnifeToss.Application.Services;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Core.Abstractions.Factories;
using KnifeToss.Core.Abstractions.Services;
using KnifeToss.Infrastructure.Audio;
using KnifeToss.Infrastructure.Factories;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Infrastructure.Persistence;
using KnifeToss.Infrastructure.Services;
using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Installers
{



    /// <summary>
    /// Represents the GameplayInstaller class.
    /// </summary>
    public class GameplayInstaller : LifetimeScope
    {
        private static readonly string HighestLevelKey = Infrastructure.Persistence.PlayerPrefsProgressStore.DefaultHighestLevelKey;

        [Header("Configurations")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private KnifeImpactEventChannel knifeImpactChannel;
        [SerializeField] private LevelCommandEventChannel levelCommandEventChannel;
        [SerializeField] private SoundSettings soundSettings;
        [SerializeField] private ButtonClickEventChannel buttonClickEventChannel;
        [SerializeField] private int startLevel = 1;

        [Header("Prefabs For Gameplay")]
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private GameObject knifeThrowerPrefab;
        [SerializeField] private GameObject knifePrefab;

        [Header("Events")]
        [SerializeField] private HitableObjectHitEventChannel hitableHitEventChannel;

        [Header("UI References")]
        [SerializeField] private LevelHudPresenter hudPresenter;

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelConfig);
            builder.RegisterInstance(knifeImpactChannel);
            builder.RegisterInstance(levelCommandEventChannel);
            builder.RegisterInstance(knifePrefab);
            builder.RegisterInstance(soundSettings);
            builder.RegisterInstance(hitableHitEventChannel);
            builder.RegisterInstance(buttonClickEventChannel);
            builder.RegisterInstance<ILevelProgressStore>(new PlayerPrefsProgressStore(HighestLevelKey));
            builder.RegisterInstance(hudPresenter);
            
            // Register RuntimeCacheService if not already registered in parent scope
            builder.Register<RuntimeCacheService>(Lifetime.Singleton);
            
            builder.Register<IGameEventBus, GameEventBus>(Lifetime.Singleton);
            builder.Register<IAudioPlayer, AudioPlayer>(Lifetime.Singleton);
            
            // Register pooled audio service for Knife components
            builder.Register<PooledAudioService>(Lifetime.Singleton)
                .As<IAudioService>()
                .As<IDisposable>()
                .WithParameter("poolRoot", resolver => resolver.Resolve<RuntimeCacheService>().MainCameraTransform ?? transform)
                .WithParameter("poolSize", 8);
            
            builder.Register<ILevelCommandInvoker, LevelCommandChannelInvoker>(Lifetime.Singleton)
                .WithParameter(levelCommandEventChannel);
            builder.Register<IKnifeTracker, KnifeTracker>(Lifetime.Singleton);
            builder.Register<IMusicPlayer, MusicPlayer>(Lifetime.Singleton);
            builder.Register<ISettingsRepository, PlayerPrefsSettingsRepository>(Lifetime.Singleton);

            builder.Register<SettingsService>(Lifetime.Singleton)
                .As<ISettingsService, IStartable>();

            builder.Register<ScoreService>(Lifetime.Singleton)
                .As<IScoreService, IStartable, IDisposable>();

            builder.Register<IStarRatingService, StarRatingService>(Lifetime.Singleton);

            // Register knife factory with pooling
            // Note: IObjectResolver is automatically injected by VContainer
            builder.Register<PooledKnifeFactory>(Lifetime.Singleton)
                .As<IKnifeFactory>()
                .WithParameter("prefab", knifePrefab)
                .WithParameter("parent", resolver => resolver.Resolve<KnivesHierarchyManager>().EnsureParent())
                .WithParameter("warmupCount", 15);

            builder.Register<GameplayObjectProvider>(Lifetime.Singleton)
                .WithParameter("targetPrefab", targetPrefab)
                .WithParameter("knifeThrowerPrefab", knifeThrowerPrefab)
                .WithParameter("knifePrefab", knifePrefab)
                .WithParameter("lifetimeTransform", transform);

            builder.Register<ITargetScaleProvider>(resolver => resolver.Resolve<GameplayObjectProvider>().GetTargetGroupManager(), Lifetime.Singleton);

            builder.Register<LevelSession>(Lifetime.Singleton);
            builder.Register<ILevelCatalog, LevelConfigCatalog>(Lifetime.Singleton);
            builder.Register<KnivesHierarchyManager>(Lifetime.Singleton)
                .WithParameter(transform)
                .WithParameter("Knives");
            builder.Register<HitableObjectSpawner>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<ITargetScaleProvider>())
                .WithParameter(soundSettings)
                .WithParameter(hitableHitEventChannel);
            builder.Register<PreStuckKnifeSpawner>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<ITargetScaleProvider>())
                .WithParameter(knifePrefab);
            builder.Register<LevelSessionService>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<GameplayObjectProvider>())
                .WithParameter(startLevel)
                .WithParameter(knifeImpactChannel)
                .WithParameter(hitableHitEventChannel);

            builder.Register<HitableObjectSoundPlayer>(Lifetime.Singleton)
                .As<IStartable>()
                .As<IDisposable>();

            builder.Register<LevelFlowCoordinator>(Lifetime.Singleton)
                .As<IStartable>()
                .As<IDisposable>();

            builder.RegisterEntryPoint<MusicPlayerStart>();

            builder.Register<ButtonClickSoundPlayer>(Lifetime.Singleton)
                .WithParameter(soundSettings)
                .WithParameter(buttonClickEventChannel)
                .As<IStartable>()
                .As<IDisposable>();
        }

        /// <summary>
        /// Represents the MusicPlayerStart class.
        /// </summary>
        public class MusicPlayerStart : IStartable
        {
            private readonly IMusicPlayer _musicPlayer;

            /// <summary>
            /// Performs the MusicPlayerStart operation.
            /// </summary>
            public MusicPlayerStart(IMusicPlayer musicPlayer)
            {
                _musicPlayer = musicPlayer;
            }

            /// <summary>
            /// Performs the Start operation.
            /// </summary>
            public void Start()
            {
                _musicPlayer.PlayMusic();
            }
        }
    }
}
