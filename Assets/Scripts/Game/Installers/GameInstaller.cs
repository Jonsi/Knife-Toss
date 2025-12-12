using Common.Collision.Events;
using Common.Events;
using Game.Entities.Collectible.Events;
using Game.Entities.Knife.Events;
using Game.Entities.Knife.Views;
using Game.Flow.Events;
using Game.KnifeThrower.Configs;
using Game.KnifeThrower.Events;
using Game.Level.Catalogs;
using Game.Settings;
using Game.UI.Controllers;
using Game.UI.Popups;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Systems;
using Game.Feedback.VFX.Data;
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
        [SerializeField] private KnifeThrownEventChannel _KnifeThrownEventChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitEventChannel;
        
        [Header("Flow Event Channels")]
        [SerializeField] private LevelCompletedEventChannel _LevelCompletedEventChannel;
        [SerializeField] private LevelFailedEventChannel _LevelFailedEventChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedEventChannel;
        [SerializeField] private ReturnToMenuEventChannel _ReturnToMenuEventChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedEventChannel;

        [Header("UI Event Channels")]
        [SerializeField] private VoidEventChannel _RetryLevelClickedChannel;
        [SerializeField] private VoidEventChannel _RestartStageClickedChannel;

        [Header("Feedback")]
        [SerializeField] private SoundSettings _SoundSettings;
        [SerializeField] private VFXSettings _VfxSettings;
        [SerializeField] private AudioPlayer _AudioPlayer;
        
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
            Install(builder, CreatePersistenceInstaller());
            Install(builder, CreateCollisionInstaller());
            Install(builder, CreateAudioInstaller());
            Install(builder, CreateVFXInstaller());
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
            return new KnifeInstaller(
                _KnifeThrowConfig,
                _KnifeThrowerView,
                _KnifeImpactEventChannel,
                _KnifeThrownEventChannel);
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
                _RestartStageClickedChannel,
                _LevelStartedEventChannel);
        }

        private GameFlowInstaller CreateGameFlowInstaller()
        {
            return new GameFlowInstaller(_GameSettings);
        }

        private PersistenceInstaller CreatePersistenceInstaller()
        {
            return new PersistenceInstaller(
                _LevelCompletedEventChannel,
                _StageCompletedEventChannel,
                _CollectibleHitEventChannel);
        }

        private CollisionInstaller CreateCollisionInstaller()
        {
            return new CollisionInstaller(_CollisionEventChannel);
        }

        private AudioInstaller CreateAudioInstaller()
        {
            return new AudioInstaller(
                _SoundSettings,
                _AudioPlayer,
                _KnifeThrownEventChannel,
                _KnifeImpactEventChannel,
                _CollectibleHitEventChannel,
                _LevelCompletedEventChannel,
                _LevelFailedEventChannel);
        }

        private VFXInstaller CreateVFXInstaller()
        {
            return new VFXInstaller(_VfxSettings, _KnifeImpactEventChannel);
        }

        private UIInstaller CreateUIInstaller()
        {
            return new UIInstaller(
                _UIController,
                _StageFailPopup);
        }
    }
}
