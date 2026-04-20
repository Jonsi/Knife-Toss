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
        [SerializeField] private StagesMenuController _StagesMenuController;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_StagesMenuController != null)
            {
                builder.RegisterComponent(_StagesMenuController);
            }
            else
            {
                builder.RegisterComponentInHierarchy<StagesMenuController>();
            }
        }
    }
}
