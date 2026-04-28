using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.IAP.Data;
using Common.IAP.Events;
using Common.IAP.Services;
using Common.IAP.Tests.TestHelpers;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Common.IAP.Tests
{
    public class IapServiceTests
    {
        [Test]
        public async Task PurchaseAsync_ReturnsDisabledWhenIapDisabled()
        {
            var provider = new FakeIapProvider();
            var service = CreateService(provider, settings: IapTestHelpers.Settings(false));

            var result = await service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));

            Assert.AreEqual(IapPurchaseResult.Disabled.FailureReason, result.FailureReason);
            Assert.AreEqual(0, provider.PurchaseCallCount);
        }

        [Test]
        public async Task PurchaseAsync_ReturnsInvalidProductForNull()
        {
            var service = CreateService(new FakeIapProvider());

            var result = await service.PurchaseAsync(null);

            Assert.AreEqual(IapPurchaseResult.InvalidProduct.FailureReason, result.FailureReason);
        }

        [Test]
        public async Task PurchaseAsync_ReturnsInProgressWhenPurchaseIsRunning()
        {
            var provider = new FakeIapProvider();
            var pending = new UniTaskCompletionSource<IapPurchaseResult>();
            provider.PurchaseHandler = (_, _) => pending.Task;
            var service = CreateService(provider);

            var first = service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));
            await UniTask.Yield();
            var second = await service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));

            pending.TrySetResult(IapPurchaseResult.Purchased("receipt"));
            await first;

            Assert.AreEqual(IapPurchaseResult.InProgress.FailureReason, second.FailureReason);
            Assert.AreEqual(1, provider.PurchaseCallCount);
        }

        [Test]
        public async Task PurchaseAsync_NonConsumableAlreadyOwned_DoesNotCallProviderAndRaisesFailure()
        {
            var provider = new FakeIapProvider();
            var store = new FakeIapEntitlementStore();
            var product = IapTestHelpers.Product("remove_ads");
            store.SetOwned(product, true);
            var failedChannel = ScriptableObject.CreateInstance<IapPurchaseFailedEventChannel>();
            var failedCount = 0;
            failedChannel.Subscribe(_ => failedCount++);
            var telemetry = new FakeIapTelemetryListener();
            var service = CreateService(provider, store, failedChannel: failedChannel, telemetry: telemetry);

            var result = await service.PurchaseAsync(product);

            Assert.IsTrue(result.AlreadyOwned);
            Assert.AreEqual(0, provider.PurchaseCallCount);
            Assert.AreEqual(1, failedCount);
            Assert.AreEqual(1, telemetry.FailedCount);
        }

        [Test]
        public async Task PurchaseAsync_SuccessStoresNonConsumableAndRaisesCompletedTelemetry()
        {
            var provider = new FakeIapProvider();
            var store = new FakeIapEntitlementStore();
            var completedChannel = ScriptableObject.CreateInstance<IapPurchaseCompletedEventChannel>();
            var completedCount = 0;
            completedChannel.Subscribe(_ => completedCount++);
            var telemetry = new FakeIapTelemetryListener();
            var service = CreateService(provider, store, completedChannel: completedChannel, telemetry: telemetry);
            var product = IapTestHelpers.Product("remove_ads");

            var result = await service.PurchaseAsync(product);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(store.HasEntitlement(product));
            Assert.AreEqual(1, completedCount);
            Assert.AreEqual(1, telemetry.CompletedCount);
        }

        [Test]
        public async Task PurchaseAsync_ConsumableSuccess_DoesNotStoreEntitlement()
        {
            var store = new FakeIapEntitlementStore();
            var service = CreateService(new FakeIapProvider(), store);
            var product = IapTestHelpers.Product("coins", true);

            var result = await service.PurchaseAsync(product);

            Assert.IsTrue(result.Success);
            Assert.IsFalse(store.HasEntitlement(product));
        }

        [Test]
        public async Task PurchaseAsync_CallerCancellation_ReturnsCancelledNotTimeout()
        {
            var provider = new FakeIapProvider();
            provider.PurchaseHandler = (_, ct) => throw new OperationCanceledException(ct);
            var service = CreateService(provider, settings: IapTestHelpers.Settings(timeoutSeconds: 30f));

            var result = await service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));

            Assert.IsTrue(result.Cancelled);
            Assert.AreEqual(IapPurchaseResult.CancelledByUser.FailureReason, result.FailureReason);
        }

        [Test]
        public async Task PurchaseAsync_TimeoutCancellation_ReturnsTimeout()
        {
            var provider = new FakeIapProvider();
            provider.PurchaseHandler = async (_, ct) =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: ct);
                return IapPurchaseResult.Purchased("receipt");
            };
            var service = CreateService(provider, settings: IapTestHelpers.Settings(timeoutSeconds: 0.01f));

            var result = await service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));

            Assert.AreEqual(IapPurchaseResult.Timeout.FailureReason, result.FailureReason);
        }

        [Test]
        public async Task PurchaseAsync_ProviderException_ReturnsFailedAndRaisesFailure()
        {
            var provider = new FakeIapProvider();
            provider.PurchaseHandler = (_, _) => throw new InvalidOperationException("store down");
            var failedChannel = ScriptableObject.CreateInstance<IapPurchaseFailedEventChannel>();
            var failedCount = 0;
            failedChannel.Subscribe(_ => failedCount++);
            var service = CreateService(provider, failedChannel: failedChannel);

            var result = await service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));

            Assert.IsFalse(result.Success);
            Assert.AreEqual("store down", result.FailureReason);
            Assert.AreEqual(1, failedCount);
        }

        [Test]
        public async Task RestorePurchasesAsync_Disabled_ReturnsEmpty()
        {
            var provider = new FakeIapProvider();
            var service = CreateService(provider, settings: IapTestHelpers.Settings(false));

            var restored = await service.RestorePurchasesAsync();

            Assert.AreEqual(0, restored.Count);
            Assert.AreEqual(0, provider.RestoreCallCount);
        }

        [Test]
        public async Task RestorePurchasesAsync_SetsNonConsumablesOwnedAndRaisesEvents()
        {
            var product = IapTestHelpers.Product("remove_ads");
            var consumable = IapTestHelpers.Product("coins", true);
            var provider = new FakeIapProvider();
            provider.RestoreHandler = _ => UniTask.FromResult<IReadOnlyList<IapProduct>>(new[] { product, consumable });
            var store = new FakeIapEntitlementStore();
            var restoredChannel = ScriptableObject.CreateInstance<IapPurchaseRestoredEventChannel>();
            var restoredEventCount = 0;
            restoredChannel.Subscribe(_ => restoredEventCount++);
            var telemetry = new FakeIapTelemetryListener();
            var service = CreateService(provider, store, restoredChannel: restoredChannel, telemetry: telemetry);

            var restored = await service.RestorePurchasesAsync();

            Assert.AreEqual(2, restored.Count);
            Assert.IsTrue(store.HasEntitlement(product));
            Assert.IsFalse(store.HasEntitlement(consumable));
            Assert.AreEqual(1, restoredEventCount);
            Assert.AreEqual(2, telemetry.RestoredCount);
        }

        [Test]
        public async Task RestorePurchasesAsync_BlockedWhenPurchaseInProgress()
        {
            var provider = new FakeIapProvider();
            var pending = new UniTaskCompletionSource<IapPurchaseResult>();
            provider.PurchaseHandler = (_, _) => pending.Task;
            var service = CreateService(provider);

            var purchase = service.PurchaseAsync(IapTestHelpers.Product("remove_ads"));
            await UniTask.Yield();
            var restored = await service.RestorePurchasesAsync();

            pending.TrySetResult(IapPurchaseResult.Purchased("receipt"));
            await purchase;

            Assert.AreEqual(0, restored.Count);
            Assert.AreEqual(0, provider.RestoreCallCount);
        }

        private static IapService CreateService(
            FakeIapProvider provider,
            FakeIapEntitlementStore store = null,
            IapSettings settings = null,
            IapPurchaseCompletedEventChannel completedChannel = null,
            IapPurchaseFailedEventChannel failedChannel = null,
            IapPurchaseRestoredEventChannel restoredChannel = null,
            FakeIapTelemetryListener telemetry = null)
        {
            return new IapService(
                provider,
                store ?? new FakeIapEntitlementStore(),
                settings ?? IapTestHelpers.Settings(),
                ScriptableObject.CreateInstance<IapCatalog>(),
                ScriptableObject.CreateInstance<IapPurchaseStartedEventChannel>(),
                completedChannel ?? ScriptableObject.CreateInstance<IapPurchaseCompletedEventChannel>(),
                failedChannel ?? ScriptableObject.CreateInstance<IapPurchaseFailedEventChannel>(),
                restoredChannel ?? ScriptableObject.CreateInstance<IapPurchaseRestoredEventChannel>(),
                telemetry);
        }
    }
}
