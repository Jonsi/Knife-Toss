using Game.PowerUps.Views;
using Game.UI.Controllers;
using Game.UI.Data;
using Game.UI.HUD;
using Game.UI.Interfaces;
using Game.UI.Popups;
using Game.UI.Services;
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
        private readonly SettingsPopup _SettingsPopup;
        private readonly PrivacyPolicyPopup _PrivacyPopup;
        private readonly ReportErrorPopup _ReportErrorPopup;
        private readonly PrivacyPolicyContent _PrivacyPolicyContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIInstaller"/> class.
        /// </summary>
        /// <param name="controller">UI controller component.</param>
        /// <param name="failPopup">Stage fail popup component.</param>
        public UIInstaller(
            UIController controller,
            StageFailPopup failPopup,
            SettingsPopup settingsPopup,
            PrivacyPolicyPopup privacyPopup,
            ReportErrorPopup reportErrorPopup,
            PrivacyPolicyContent privacyPolicyContent)
        {
            _Controller = controller;
            _FailPopup = failPopup;
            _SettingsPopup = settingsPopup;
            _PrivacyPopup = privacyPopup;
            _ReportErrorPopup = reportErrorPopup;
            _PrivacyPolicyContent = privacyPolicyContent;
        }

        /// <summary>
        /// Registers UI components.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(_Controller);
            builder.RegisterComponent(_FailPopup);
            builder.RegisterComponent(_SettingsPopup);
            builder.RegisterComponent(_PrivacyPopup);
            builder.RegisterComponent(_ReportErrorPopup);

            if (_PrivacyPolicyContent != null)
                builder.RegisterInstance(_PrivacyPolicyContent);

            builder.Register<PopupGamePauseController>(Lifetime.Singleton).As<IGamePauseController>();
            builder.Register<SettingsPopupController>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<CoinsUiDisplay>();
            builder.RegisterComponentInHierarchy<PowerUpHudController>();
            builder.RegisterComponentInHierarchy<SettingsButtonView>();
        }
    }
}
