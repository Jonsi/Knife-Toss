using Common.Collision.Events;
using Common.Events;
using Game.Entities.Collectible.Events;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Flow.Events;
using Game.KnifeThrower.Configs;
using Game.Level.Catalogs;
using Game.Settings;
using Game.UI.Controllers;
using Game.UI.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Main composition root that configures dependency injection for all game systems.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private GameSettings _GameSettings;
        [SerializeField] private KnifeThrowConfigAsset _KnifeThrowConfig;
        [SerializeField] private StagesConfigAsset _StagesConfig;
        
        [Header("Event Channels")]
        [SerializeField] private CollisionEventChannel _CollisionEventChannel;
        [SerializeField] private KnifeImpactEventChannel _KnifeImpactEventChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitEventChannel;
        
        [Header("Flow Event Channels")]
        [SerializeField] private LevelCompletedEventChannel _LevelCompletedEventChannel;
        [SerializeField] private LevelFailedEventChannel _LevelFailedEventChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedEventChannel;
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;

        [Header("UI Event Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelClickedChannel;
        [SerializeField] private VoidEventChannel _RestartStageClickedChannel;
        
        [Header("Scene References")]
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private StageFailPopup _StageFailPopup;
        [SerializeField] private UIController _UIController;

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateTargetInstaller());
            Install(builder, CreateKnifeInstaller());
            Install(builder, CreateCollectibleInstaller());
            Install(builder, CreateLevelInstaller());
            Install(builder, CreateStageInstaller());
            Install(builder, CreateGameFlowInstaller());
            Install(builder, CreateCollisionInstaller());
            Install(builder, CreateUIInstaller());
        }

        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }

        private TargetInstaller CreateTargetInstaller()
        {
            return new TargetInstaller(_StagesConfig.DefaultTargetPrefab);
        }

        private KnifeInstaller CreateKnifeInstaller()
        {
            return new KnifeInstaller(_KnifeThrowConfig, _KnifeThrowerView, _KnifeImpactEventChannel);
        }

        private CollectibleInstaller CreateCollectibleInstaller()
        {
            return new CollectibleInstaller(_CollectibleHitEventChannel);
        }

        private LevelInstaller CreateLevelInstaller()
        {
            return new LevelInstaller(_LevelRoot, _LevelCompletedEventChannel, _LevelFailedEventChannel);
        }

        private StageInstaller CreateStageInstaller()
        {
            return new StageInstaller(
                _StagesConfig,
                _StageCompletedEventChannel,
                _ReturnToMenuEventChannel,
                _RetryLevelClickedChannel,
                _RestartStageClickedChannel);
        }

        private GameFlowInstaller CreateGameFlowInstaller()
        {
            return new GameFlowInstaller(_GameSettings);
        }

        private CollisionInstaller CreateCollisionInstaller()
        {
            return new CollisionInstaller(_CollisionEventChannel);
        }

        private UIInstaller CreateUIInstaller()
        {
            return new UIInstaller(_UIController, _StageFailPopup);
        }
    }
}
