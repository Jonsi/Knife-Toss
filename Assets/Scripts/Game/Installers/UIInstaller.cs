using Game.UI.Controllers;
using Game.UI.Popups;
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
        private readonly StageFailPopup _FailPopup;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIInstaller"/> class.
        /// </summary>
        /// <param name="controller">UI controller component.</param>
        /// <param name="failPopup">Stage fail popup component.</param>
        public UIInstaller(
            UIController controller,
            StageFailPopup failPopup)
        {
            _Controller = controller;
            _FailPopup = failPopup;
        }

        /// <summary>
        /// Registers UI components.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(_Controller);
            builder.RegisterComponent(_FailPopup);
        }
    }
}

