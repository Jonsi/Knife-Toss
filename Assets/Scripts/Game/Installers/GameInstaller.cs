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
        [Header("Scene References")]
        [SerializeField] private LevelSceneReferenceInstaller _LevelSceneReferenceInstaller;

        [Header("Installers")]
        [SerializeField] private TargetInstallerSO _TargetInstaller;
        [SerializeField] private KnifeInstallerSO _KnifeInstaller;
        [SerializeField] private CollectibleInstallerSO _CollectibleInstaller;
        [SerializeField] private PowerUpInstallerSO _PowerUpInstaller;
        [SerializeField] private LevelInstallerSO _LevelInstaller;
        [SerializeField] private StageInstallerSO _StageInstaller;
        [SerializeField] private GameFlowInstallerSO _GameFlowInstaller;
        [SerializeField] private PersistenceInstallerSO _PersistenceInstaller;
        [SerializeField] private CollisionInstallerSO _CollisionInstaller;
        [SerializeField] private AudioInstallerSO _AudioInstaller;
        [SerializeField] private VFXInstallerSO _VFXInstaller;
        [SerializeField] private UIInstallerSO _UIInstaller;

        protected override void Configure(IContainerBuilder builder)
        {
            _LevelSceneReferenceInstaller?.Install(builder);
            InstallCore(builder);
            InstallFlow(builder);
            InstallGameplay(builder);
            InstallFeedbackAndUI(builder);
        }

        private void InstallCore(IContainerBuilder builder)
        {
            _GameFlowInstaller?.Install(builder);
            _PersistenceInstaller?.Install(builder);
        }

        private void InstallFlow(IContainerBuilder builder)
        {
            _StageInstaller?.Install(builder);
            _LevelInstaller?.Install(builder);
        }

        private void InstallGameplay(IContainerBuilder builder)
        {
            _TargetInstaller?.Install(builder);
            _KnifeInstaller?.Install(builder);
            _CollectibleInstaller?.Install(builder);
            _PowerUpInstaller?.Install(builder);
            _CollisionInstaller?.Install(builder);
        }

        private void InstallFeedbackAndUI(IContainerBuilder builder)
        {
            _AudioInstaller?.Install(builder);
            _VFXInstaller?.Install(builder);
            _UIInstaller?.Install(builder);
        }
    }
}
