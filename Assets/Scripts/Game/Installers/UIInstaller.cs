using Common.Events;
using Common.MVP.Popup;
using Game.PowerUps.Views;
using Game.UI.Controllers;
using Game.UI.Factories;
using Game.UI.HUD;
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
        private readonly VoidEventChannel _RetryLevelChannel;
        private readonly VoidEventChannel _RestartStageChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIInstaller"/> class.
        /// </summary>
        /// <param name="controller">UI controller component.</param>
        /// <param name="stageFailFactory">Factory for stage fail popups.</param>
        /// <param name="retryLevelChannel">Retry level request channel.</param>
        /// <param name="restartStageChannel">Restart stage request channel.</param>
        public UIInstaller(
            UIController controller,
            StageFailFactorySO stageFailFactory,
            VoidEventChannel retryLevelChannel,
            VoidEventChannel restartStageChannel)
        {
            _Controller = controller;
            _StageFailFactory = stageFailFactory;
            _RetryLevelChannel = retryLevelChannel;
            _RestartStageChannel = restartStageChannel;
        }

        /// <summary>
        /// Registers UI components.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StageFailFactory);
            builder.RegisterInstance(_RetryLevelChannel);
            builder.RegisterInstance(_RestartStageChannel);
            builder.Register<PopupNavigationService>(Lifetime.Singleton).As<IPopupNavigationService>();
            builder.RegisterComponent(_Controller);
            builder.RegisterComponentInHierarchy<CollectibleCountDisplay>();
            builder.RegisterComponentInHierarchy<PowerUpHudController>();
        }
    }
}

