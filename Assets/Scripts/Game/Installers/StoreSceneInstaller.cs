using Game.Store.Data;
using Game.Store.Events;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Composition root for the Store scene.
    /// </summary>
    public class StoreSceneInstaller : LifetimeScope
    {
        [Header("Store Configuration")]
        [SerializeField] private StoreCatalog _StoreCatalog;

        [Header("Store Event Channels")]
        [SerializeField] private PurchaseStartedEventChannel _PurchaseStartedEventChannel;
        [SerializeField] private PurchaseCompletedEventChannel _PurchaseCompletedEventChannel;
        [SerializeField] private PurchaseFailedEventChannel _PurchaseFailedEventChannel;

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateStoreInstaller());
        }

        private static void Install(IContainerBuilder builder, IInstaller installer)
        {
            installer.Install(builder);
        }

        private StoreInstaller CreateStoreInstaller()
        {
            return new StoreInstaller(
                _StoreCatalog,
                _PurchaseStartedEventChannel,
                _PurchaseCompletedEventChannel,
                _PurchaseFailedEventChannel);
        }
    }
}
