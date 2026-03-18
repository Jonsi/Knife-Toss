using Game.Flow.Game;
using Game.Persistence.Services;
using Game.UI.Common;
using Game.UI.HUD;
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
        [SerializeField] private CurrencyAccumulationConfig _CurrencyAccumulationConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            builder.RegisterComponent(_MainMenuController);
            builder.RegisterComponentInHierarchy<CoinsUiDisplay>();
        }
    }
}
