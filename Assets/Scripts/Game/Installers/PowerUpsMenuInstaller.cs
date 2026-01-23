using Game.UI.PowerUps;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the PowerUpsMenu scene.
    /// Connects to the parent GameInstaller scope to access scene services.
    /// </summary>
    public class PowerUpsMenuInstaller : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private PowerUpsMenuController _PowerUpsMenuController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_PowerUpsMenuController);
        }
    }
}
