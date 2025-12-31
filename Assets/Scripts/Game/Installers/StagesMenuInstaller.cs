using Game.Flow.Game;
using Game.UI.Stages;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the StagesMenu scene.
    /// Connects to the parent GameInstaller scope to access scene services.
    /// </summary>
    public class StagesMenuInstaller : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private StagesMenuController _StagesMenuController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_StagesMenuController);

            builder.RegisterBuildCallback(container =>
            {
                var sceneLoaderService = container.Resolve<SceneLoaderService>();
                _StagesMenuController.Construct(sceneLoaderService);
            });
        }
    }
}
