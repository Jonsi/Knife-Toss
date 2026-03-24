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
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<StageListController>();
        }
    }
}
