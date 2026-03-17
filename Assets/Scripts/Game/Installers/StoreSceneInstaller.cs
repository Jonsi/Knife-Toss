using Common.Ads.Interfaces;
using Game.Ads.Controllers;
using Game.Ads.Data;
using Game.Store.Data;
using Game.Store.Events;
using Game.UI.Common;
using Game.UI.Store;
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
        [SerializeField] private CurrencyAccumulationConfig _CurrencyAccumulationConfig;

        [Header("Store Event Channels")]
        [SerializeField] private PurchaseStartedEventChannel _PurchaseStartedEventChannel;
        [SerializeField] private PurchaseCompletedEventChannel _PurchaseCompletedEventChannel;
        [SerializeField] private PurchaseFailedEventChannel _PurchaseFailedEventChannel;

        [Header("Scene References")]
        [SerializeField] private StoreMenuController _StoreMenuController;
        [SerializeField] private StoreAdCtaView _StoreAdCtaView;

        [Header("Ads (optional; requires AdSettings in GameInstaller)")]
        [SerializeField] private RewardedPlacement _StoreAdPlacement;

        protected override void Configure(IContainerBuilder builder)
        {
            Install(builder, CreateStoreInstaller());

            if (_CurrencyAccumulationConfig == null)
            {
                _CurrencyAccumulationConfig = Resources.Load<CurrencyAccumulationConfig>("Configurations/Systems/UI/CurrencyAccumulationConfig");
            }

            if (_CurrencyAccumulationConfig != null)
            {
                builder.RegisterInstance(_CurrencyAccumulationConfig);
            }

            if (_StoreMenuController != null)
                builder.RegisterComponent(_StoreMenuController);

            builder.RegisterComponentInHierarchy<StoreBalanceChipView>();

            if (_StoreAdPlacement != null && _StoreAdCtaView != null)
            {
                builder.RegisterInstance(_StoreAdPlacement);
                builder.RegisterComponent(_StoreAdCtaView);

                builder.Register(resolver => new StoreAdController(
                    resolver.Resolve<IAdService>(),
                    _StoreAdPlacement,
                    resolver,
                    resolver.Resolve<StoreAdCtaView>(),
                    resolver.Resolve<StoreBalanceChipView>()
                ), Lifetime.Scoped).AsImplementedInterfaces();
            }
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
