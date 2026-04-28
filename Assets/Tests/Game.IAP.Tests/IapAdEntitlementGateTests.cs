using Game.Ads.Services;
using Game.IAP.Services;
using Game.IAP.Tests.TestHelpers;
using Game.Store.Rewards;
using NUnit.Framework;

namespace Game.IAP.Tests
{
    public class IapAdEntitlementGateTests
    {
        [Test]
        public void CanShowInterstitial_ReturnsTrueWhenCatalogMissingRemoveAds()
        {
            var gate = new IapAdEntitlementGate(new FakeEntitlementStore(), GameIapTestHelpers.Catalog());

            Assert.IsTrue(gate.CanShowInterstitial());
        }

        [Test]
        public void CanShowInterstitial_ReturnsTrueWhenEntitlementNotOwned()
        {
            var product = GameIapTestHelpers.Product("remove_ads");
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), product);
            var store = new FakeEntitlementStore { Owned = false };
            var gate = new IapAdEntitlementGate(store, GameIapTestHelpers.Catalog(entry));

            Assert.IsTrue(gate.CanShowInterstitial());
        }

        [Test]
        public void CanShowInterstitial_ReturnsFalseWhenRemoveAdsOwned()
        {
            var product = GameIapTestHelpers.Product("remove_ads");
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), product);
            var store = new FakeEntitlementStore { Owned = true };
            var gate = new IapAdEntitlementGate(store, GameIapTestHelpers.Catalog(entry));

            Assert.IsFalse(gate.CanShowInterstitial());
        }

        [Test]
        public void CanShowInterstitial_ReturnsTrueForNullCatalog()
        {
            var gate = new IapAdEntitlementGate(new FakeEntitlementStore(), null);

            Assert.IsTrue(gate.CanShowInterstitial());
        }

        [Test]
        public void CanShowInterstitial_CachesResultUntilInvalidated()
        {
            var product = GameIapTestHelpers.Product("remove_ads");
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), product);
            var store = new FakeEntitlementStore { Owned = false };
            var gate = new IapAdEntitlementGate(store, GameIapTestHelpers.Catalog(entry));

            Assert.IsTrue(gate.CanShowInterstitial());
            store.Owned = true;
            Assert.IsTrue(gate.CanShowInterstitial());
            Assert.AreEqual(1, store.HasEntitlementCallCount);

            gate.Invalidate();
            Assert.IsFalse(gate.CanShowInterstitial());
        }

        [Test]
        public void CanShowInterstitial_InvalidatesWhenPlayerDataEntitlementChanges()
        {
            var product = GameIapTestHelpers.Product("remove_ads");
            var entry = GameIapTestHelpers.Entry(new RemoveAdsReward(), product);
            var store = new PlayerDataIapEntitlementStore(GameIapTestHelpers.PlayerDataService());
            var gate = new IapAdEntitlementGate(store, GameIapTestHelpers.Catalog(entry));

            Assert.IsTrue(gate.CanShowInterstitial());
            store.SetOwned(product, true);

            Assert.IsFalse(gate.CanShowInterstitial());
        }
    }
}
