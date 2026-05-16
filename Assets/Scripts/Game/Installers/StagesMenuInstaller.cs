using Game.UI.Interfaces;
using Game.UI.Popups;
using Game.UI.Services;
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

        [SerializeField]
        [Tooltip("Optional rewards chest popup (test scenes); when null it is skipped.")]
        private RewardsPresentationPopup _RewardsPresentationPopup;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<PopupGamePauseController>(Lifetime.Singleton).As<IGamePauseController>();

            if (_StagesMenuController != null)
            {
                builder.RegisterComponent(_StagesMenuController);
            }
            else
            {
                builder.RegisterComponentInHierarchy<StagesMenuController>();
            }

            if (_RewardsPresentationPopup != null)
            {
                builder.RegisterComponent(_RewardsPresentationPopup);
            }
        }
    }
}
