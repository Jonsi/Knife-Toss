using System.Threading;
using System.Threading.Tasks;
using Common.IAP.Data;
using Game.IAP.Services;
using Game.IAP.Tests.TestHelpers;
using Game.Store.Rewards;
using NUnit.Framework;

namespace Game.IAP.Tests
{
    public class MainMenuRemoveAdsFlowTests
    {
        [Test]
        public void IsAvailable_FalseWhenNoCatalogOrNoRemoveAdsEntry()
        {
            var service = new FakeIapService();

            Assert.IsFalse(new MainMenuRemoveAdsFlow(service, null, null).IsAvailable);
            Assert.IsFalse(new MainMenuRemoveAdsFlow(service, GameIapTestHelpers.Catalog(), null).IsAvailable);
        }

        [Test]
        public void IsAvailable_TrueWhenRemoveAdsEntryFound()
        {
            var flow = CreateFlow(out _, out _, out _);

            Assert.IsTrue(flow.IsAvailable);
        }

        [Test]
        public void CanPurchase_FalseWhenPurchaseInProgressOrAlreadyOwned()
        {
            var flow = CreateFlow(out var service, out var product, out _);
            service.IsPurchaseInProgress = true;
            Assert.IsFalse(flow.CanPurchase);

            service.IsPurchaseInProgress = false;
            service.SetOwned(product, true);
            Assert.IsFalse(flow.CanPurchase);
        }

        [Test]
        public async Task TryPurchaseAsync_CallsPurchaseWithCorrectProductAndToken()
        {
            var flow = CreateFlow(out var service, out var product, out _);
            using var cts = new CancellationTokenSource();

            await flow.TryPurchaseAsync(cts.Token);

            Assert.AreEqual(1, service.PurchaseCallCount);
            Assert.AreSame(product, service.LastPurchasedProduct);
            Assert.AreEqual(cts.Token, service.LastCancellationToken);
        }

        [Test]
        public async Task TryPurchaseAsync_DoesNotPurchaseWhenUnavailable()
        {
            var service = new FakeIapService();
            var analytics = new FakeAnalyticsService();
            var flow = new MainMenuRemoveAdsFlow(service, GameIapTestHelpers.Catalog(), analytics);

            await flow.TryPurchaseAsync();

            Assert.AreEqual(0, service.PurchaseCallCount);
            Assert.AreEqual("main_menu_remove_ads_unavailable", analytics.Events[1].Name);
        }

        [Test]
        public async Task TryPurchaseAsync_TracksStartedResultAndAlreadyOwnedPaths()
        {
            var flow = CreateFlow(out var service, out var product, out var analytics);
            service.PurchaseResult = IapPurchaseResult.Purchased("receipt");

            await flow.TryPurchaseAsync();
            service.SetOwned(product, true);
            await flow.TryPurchaseAsync();

            Assert.AreEqual("main_menu_remove_ads_clicked", analytics.Events[0].Name);
            Assert.AreEqual("main_menu_remove_ads_purchase_result", analytics.Events[1].Name);
            Assert.AreEqual("main_menu_remove_ads_already_owned", analytics.Events[3].Name);
        }

        private static MainMenuRemoveAdsFlow CreateFlow(
            out FakeIapService service,
            out IapProduct product,
            out FakeAnalyticsService analytics)
        {
            service = new FakeIapService();
            product = GameIapTestHelpers.Product("remove_ads");
            analytics = new FakeAnalyticsService();
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), product);
            return new MainMenuRemoveAdsFlow(service, GameIapTestHelpers.Catalog(entry), analytics);
        }
    }
}
