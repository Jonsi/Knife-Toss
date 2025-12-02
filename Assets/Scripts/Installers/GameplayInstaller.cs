using System;
using KnifeToss.Application.Services;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Audio;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Infrastructure.Persistence;
using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Installers
{
    /// <summary>
    /// VContainer LifetimeScope responsible for wiring gameplay dependencies.
    /// </summary>
    public class GameplayInstaller : LifetimeScope
    {
        private static readonly string HighestLevelKey = Infrastructure.Persistence.PlayerPrefsProgressStore.DefaultHighestLevelKey;

        [Header("Configurations")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private KnifeImpactEventChannel knifeImpactChannel;
        [SerializeField] private LevelCommandEventChannel levelCommandEventChannel;
        [SerializeField] private int startLevel = 1;

        [Header("Prefabs For Gameplay")]
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private GameObject knifeThrowerPrefab;
        [SerializeField] private GameObject knifePrefab;

        [Header("UI References")]
        [SerializeField] private LevelHudPresenter hudPresenter;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelConfig);
            builder.RegisterInstance(knifeImpactChannel);
            builder.RegisterInstance(levelCommandEventChannel);
            builder.RegisterInstance(knifePrefab);
            builder.RegisterInstance<ILevelProgressStore>(new PlayerPrefsProgressStore(HighestLevelKey));
            builder.RegisterInstance(hudPresenter);
            builder.Register<IAudioPlayer, AudioPlayer>(Lifetime.Singleton);
            builder.Register<ILevelCommandInvoker, LevelCommandChannelInvoker>(Lifetime.Singleton)
                .WithParameter(levelCommandEventChannel);
            builder.Register<IKnifeTracker, KnifeTracker>(Lifetime.Singleton);

            builder.Register<GameplayObjectProvider>(Lifetime.Singleton)
                .WithParameter("targetPrefab", targetPrefab)
                .WithParameter("knifeThrowerPrefab", knifeThrowerPrefab)
                .WithParameter("lifetimeTransform", transform);

            builder.Register<LevelSession>(Lifetime.Singleton);
            builder.Register<ILevelCatalog, LevelConfigCatalog>(Lifetime.Singleton);
            builder.Register<KnivesHierarchyManager>(Lifetime.Singleton)
                .WithParameter(transform)
                .WithParameter(resolver => resolver.Resolve<GameplayObjectProvider>().GetTarget().transform)
                .WithParameter("Knives");
            builder.Register<PreStuckKnifeSpawner>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<GameplayObjectProvider>().GetTarget().transform)
                .WithParameter(knifePrefab);
            builder.Register<LevelSessionService>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<GameplayObjectProvider>())
                .WithParameter(startLevel)
                .WithParameter(knifeImpactChannel);

            builder.Register<LevelFlowCoordinator>(Lifetime.Singleton)
                .As<IStartable>()
                .As<IDisposable>();

            builder.Register<RotatingTargetFailHandler>(Lifetime.Singleton)
                .As<IStartable>()
                .As<IDisposable>();
        }
    }
}
