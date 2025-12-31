using Game.Flow.Game;
using Game.Persistence.Services;
using Game.UI.MainMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the MainMenu scene.
    /// Connects to the parent GameInstaller scope to access persistence and scene services.
    /// </summary>
    public class MainMenuInstaller : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private MainMenuController _MainMenuController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_MainMenuController);
        }
    }
}
