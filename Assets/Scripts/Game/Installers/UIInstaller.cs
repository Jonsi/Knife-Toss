using Common.MVP.Popup;
using Game.Infrastructure.Factories;
using Game.Presentation.HUD.Collectibles;
using Game.Presentation.HUD.KnifeMagazine;
using Game.Presentation.HUD.MuteButton;
using Game.Presentation.HUD.PowerUps;
using Game.Presentation.HUD.StageProgress;
using Game.UI.Controllers;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs UI components.
    /// </summary>
    public class UIInstaller : IInstaller
    {
        private readonly UIController _Controller;
        private readonly StageFailFactorySO _StageFailFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIInstaller"/> class.
        /// </summary>
        /// <param name="controller">UI controller component.</param>
        /// <param name="stageFailFactory">Factory for stage fail popups.</param>
        public UIInstaller(
            UIController controller,
            StageFailFactorySO stageFailFactory)
        {
            _Controller = controller;
            _StageFailFactory = stageFailFactory;
        }

        /// <summary>
        /// Registers UI components.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StageFailFactory);
            builder.Register<PopupNavigationService>(Lifetime.Singleton).As<IPopupNavigationService>();
            builder.RegisterComponent(_Controller);
            builder.RegisterComponentInHierarchy<KnifeMagazineView>();
            builder.RegisterComponentInHierarchy<StageProgressView>();
            builder.RegisterComponentInHierarchy<CollectibleCountView>();
            builder.RegisterComponentInHierarchy<MuteButtonView>();
            builder.RegisterComponentInHierarchy<PowerUpHudView>();

            builder.RegisterEntryPoint<KnifeMagazinePresenter>();
            builder.RegisterEntryPoint<StageProgressPresenter>();
            builder.RegisterEntryPoint<CollectibleCountPresenter>();
            builder.RegisterEntryPoint<MuteButtonPresenter>();
            builder.RegisterEntryPoint<PowerUpHudPresenter>();
        }
    }
}
