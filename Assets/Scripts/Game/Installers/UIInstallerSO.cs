using Game.DI;
using Common.MVP.Popup;
using Game.Infrastructure.Factories;
using Game.Presentation.HUD.Collectibles;
using Game.Presentation.HUD.KnifeMagazine;
using Game.Presentation.HUD.MuteButton;
using Game.Presentation.HUD.PowerUps;
using Game.Presentation.HUD.StageProgress;
using Game.UI.Controllers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs UI components.
    /// </summary>
    [CreateAssetMenu(fileName = "UIInstaller", menuName = "Knife Toss/Installers/UI")]
    public class UIInstallerSO : BaseInstallerSO
    {
        [Header("Factories")]
        [SerializeField] private StageFailFactorySO _StageFailFactory;

        /// <summary>
        /// Registers UI components.
        /// </summary>
        /// <param name="builder">Container builder instance.</param>
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StageFailFactory);
            builder.Register<PopupNavigationService>(Lifetime.Singleton).As<IPopupNavigationService>();

            builder.Register<KnifeMagazinePresenter>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StageProgressPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CollectibleCountPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MuteButtonPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PowerUpHudPresenter>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
