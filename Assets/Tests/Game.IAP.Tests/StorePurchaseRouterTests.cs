using System.Threading.Tasks;
using Common.IAP.Data;
using Game.IAP.Tests.TestHelpers;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Rewards;
using Game.Store.Services;
using NUnit.Framework;
using UnityEngine;

namespace Game.IAP.Tests
{
    public class StorePurchaseRouterTests
    {
        [Test]
        public async Task TryPurchaseAsync_SoftCurrencyEntry_DelegatesToPurchaseService()
        {
            var purchaseService = new FakePurchaseService();
            var router = CreateRouter(purchaseService, new FakeIapService());
            var entry = GameIapTestHelpers.Entry(new FakeReward());

            var result = await router.TryPurchaseAsync(entry);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, purchaseService.TryPurchaseCallCount);
        }

        [Test]
        public async Task TryPurchaseAsync_IapEntryWithNullProduct_ReturnsInvalidItem()
        {
            var router = CreateRouter(new FakePurchaseService(), new FakeIapService());
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), null);

            var result = await router.TryPurchaseAsync(entry);

            Assert.AreEqual(PurchaseResult.InvalidItem.FailureReason, result.FailureReason);
        }

        [Test]
        public async Task TryPurchaseAsync_IapEntryWhenInProgress_ReturnsPurchaseInProgress()
        {
            var iapService = new FakeIapService { IsPurchaseInProgress = true };
            var router = CreateRouter(new FakePurchaseService(), iapService);
            var entry = GameIapTestHelpers.Entry(new FakeReward(), GameIapTestHelpers.Product("remove_ads"));

            var result = await router.TryPurchaseAsync(entry);

            Assert.AreEqual(PurchaseResult.PurchaseInProgress.FailureReason, result.FailureReason);
        }

        [Test]
        public async Task TryPurchaseAsync_IapEntryAlreadyOwned_ReturnsAlreadyOwnedAndDoesNotCallIap()
        {
            var product = GameIapTestHelpers.Product("remove_ads");
            var iapService = new FakeIapService();
            iapService.SetOwned(product, true);
            var router = CreateRouter(new FakePurchaseService(), iapService);
            var entry = GameIapTestHelpers.Entry(new FakeReward(), product);

            var result = await router.TryPurchaseAsync(entry);

            Assert.AreEqual(PurchaseResult.AlreadyOwned.FailureReason, result.FailureReason);
            Assert.AreEqual(0, iapService.PurchaseCallCount);
        }

        [Test]
        public async Task TryPurchaseAsync_IapSuccess_GrantsRewardAndRaisesCompleted()
        {
            var reward = new FakeReward();
            var completed = ScriptableObject.CreateInstance<PurchaseCompletedEventChannel>();
            var completedCount = 0;
            completed.Subscribe(_ => completedCount++);
            var iapService = new FakeIapService();
            var router = CreateRouter(new FakePurchaseService(), iapService, completedChannel: completed);
            var entry = GameIapTestHelpers.Entry(reward, GameIapTestHelpers.Product("remove_ads"));

            var result = await router.TryPurchaseAsync(entry);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, iapService.PurchaseCallCount);
            Assert.AreEqual(1, reward.GrantCallCount);
            Assert.AreEqual(1, completedCount);
        }

        [TestCase(true, false, "Purchase cancelled.")]
        [TestCase(false, true, "Item already owned.")]
        public async Task TryPurchaseAsync_IapFailure_MapsFailureAndRaisesFailed(bool cancelled, bool alreadyOwned, string expected)
        {
            var failed = ScriptableObject.CreateInstance<PurchaseFailedEventChannel>();
            var failedCount = 0;
            failed.Subscribe(_ => failedCount++);
            var iapService = new FakeIapService
            {
                PurchaseResult = cancelled
                    ? IapPurchaseResult.CancelledByUser
                    : alreadyOwned
                        ? IapPurchaseResult.ProductAlreadyOwned
                        : IapPurchaseResult.Failed("failed")
            };
            var router = CreateRouter(new FakePurchaseService(), iapService, failedChannel: failed);
            var entry = GameIapTestHelpers.Entry(new FakeReward(), GameIapTestHelpers.Product("remove_ads"));

            var result = await router.TryPurchaseAsync(entry);

            Assert.AreEqual(expected, result.FailureReason);
            Assert.AreEqual(1, failedCount);
        }

        [Test]
        public async Task TryPurchaseAsync_GrantThrowsAfterIapSuccess_ReturnsExplicitErrorWithoutRetryingIap()
        {
            var reward = new FakeReward { ThrowOnGrant = true };
            var iapService = new FakeIapService();
            var router = CreateRouter(new FakePurchaseService(), iapService);
            var entry = GameIapTestHelpers.Entry(reward, GameIapTestHelpers.Product("remove_ads"));
            var previousLogEnabled = Debug.unityLogger.logEnabled;

            PurchaseResult result;
            try
            {
                Debug.unityLogger.logEnabled = false;
                result = await router.TryPurchaseAsync(entry);
            }
            finally
            {
                Debug.unityLogger.logEnabled = previousLogEnabled;
            }

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Reward grant failed - entitlement preserved. Please restart and contact support.", result.FailureReason);
            Assert.AreEqual(1, iapService.PurchaseCallCount);
        }

        [Test]
        public async Task RestorePurchasesAsync_DelegatesToIapServiceAndReturnsCount()
        {
            var iapService = new FakeIapService
            {
                RestoreResult = new[] { GameIapTestHelpers.Product("remove_ads") }
            };
            var router = CreateRouter(new FakePurchaseService(), iapService);

            var count = await router.RestorePurchasesAsync();

            Assert.AreEqual(1, count);
        }

        private static StorePurchaseRouter CreateRouter(
            FakePurchaseService purchaseService,
            FakeIapService iapService,
            PurchaseCompletedEventChannel completedChannel = null,
            PurchaseFailedEventChannel failedChannel = null)
        {
            return new StorePurchaseRouter(
                purchaseService,
                iapService,
                new FakeGrantContext(),
                ScriptableObject.CreateInstance<PurchaseStartedEventChannel>(),
                completedChannel ?? ScriptableObject.CreateInstance<PurchaseCompletedEventChannel>(),
                failedChannel ?? ScriptableObject.CreateInstance<PurchaseFailedEventChannel>());
        }
    }
}
