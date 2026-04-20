using Common.IAP.Data;
using Common.IAP.Events;
using Common.IAP.Interfaces;
using Common.IAP.Providers;
using Common.IAP.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.IAP.Installers
{
    /// <summary>
    /// MonoBehaviour installer that owns global IAP configuration and registrations.
    /// </summary>
    public class IapInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private IapCatalog _IapCatalog;
        [SerializeField] private IapSettings _IapSettings;
        [SerializeField] private IapPurchaseStartedEventChannel _PurchaseStartedChannel;
        [SerializeField] private IapPurchaseCompletedEventChannel _PurchaseCompletedChannel;
        [SerializeField] private IapPurchaseFailedEventChannel _PurchaseFailedChannel;
        [SerializeField] private IapPurchaseRestoredEventChannel _PurchaseRestoredChannel;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_IapCatalog);
            builder.RegisterInstance(_IapSettings);
            builder.RegisterInstance(_PurchaseStartedChannel);
            builder.RegisterInstance(_PurchaseCompletedChannel);
            builder.RegisterInstance(_PurchaseFailedChannel);
            builder.RegisterInstance(_PurchaseRestoredChannel);

            builder.Register<StubIapProvider>(Lifetime.Singleton).As<IIapProvider>();
            builder.Register<NullIapTelemetryListener>(Lifetime.Singleton).As<IIapTelemetryListener>();
            builder.Register<IapService>(Lifetime.Singleton).As<IIapService>();
        }
    }
}
