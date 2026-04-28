using Game.IAP.Services;
using Game.IAP.Tests.TestHelpers;
using NUnit.Framework;

namespace Game.IAP.Tests
{
    public class PlayerDataIapEntitlementStoreTests
    {
        [Test]
        public void HasEntitlement_ReturnsFalseForInvalidProduct()
        {
            var store = new PlayerDataIapEntitlementStore(GameIapTestHelpers.PlayerDataService());

            Assert.IsFalse(store.HasEntitlement(null));
            Assert.IsFalse(store.HasEntitlement(GameIapTestHelpers.Product("")));
        }

        [Test]
        public void SetOwned_PersistsOnlyWhenChanged()
        {
            var dataStore = new FakePlayerDataStore();
            var store = new PlayerDataIapEntitlementStore(GameIapTestHelpers.PlayerDataService(dataStore));
            var product = GameIapTestHelpers.Product("remove_ads");
            var changeCount = 0;
            store.EntitlementsChanged += () => changeCount++;

            Assert.IsFalse(store.HasEntitlement(product));
            store.SetOwned(product, true);
            Assert.IsTrue(store.HasEntitlement(product));
            Assert.AreEqual(1, dataStore.SaveCallCount);
            Assert.AreEqual(1, changeCount);

            store.SetOwned(product, true);
            Assert.AreEqual(1, dataStore.SaveCallCount);
            Assert.AreEqual(1, changeCount);
        }
    }
}
