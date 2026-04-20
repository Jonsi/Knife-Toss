using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.Store.Services;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Installs store services and event channels.
    /// </summary>
    public class StoreInstaller : IInstaller
    {
        private readonly StoreCatalog _Catalog;
        private readonly PurchaseStartedEventChannel _StartedEventChannel;
        private readonly PurchaseCompletedEventChannel _CompletedEventChannel;
        private readonly PurchaseFailedEventChannel _FailedEventChannel;

        public StoreInstaller(
            StoreCatalog catalog,
            PurchaseStartedEventChannel startedEventChannel,
            PurchaseCompletedEventChannel completedEventChannel,
            PurchaseFailedEventChannel failedEventChannel)
        {
            _Catalog = catalog;
            _StartedEventChannel = startedEventChannel;
            _CompletedEventChannel = completedEventChannel;
            _FailedEventChannel = failedEventChannel;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_Catalog).AsSelf();
            builder.RegisterInstance(_StartedEventChannel).AsSelf();
            builder.RegisterInstance(_CompletedEventChannel).AsSelf();
            builder.RegisterInstance(_FailedEventChannel).AsSelf();
            builder.Register<KnifeSkinService>(Lifetime.Singleton).AsSelf();
            builder.Register<PurchaseService>(Lifetime.Singleton).As<IPurchaseService>();
            builder.Register<StorePurchaseRouter>(Lifetime.Singleton).As<IStorePurchaseRouter>();
        }
    }
}
