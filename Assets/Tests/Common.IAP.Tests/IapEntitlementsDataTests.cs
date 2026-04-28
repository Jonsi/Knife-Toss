using Game.Persistence.Data;
using NUnit.Framework;

namespace Common.IAP.Tests
{
    public class IapEntitlementsDataTests
    {
        [TestCase(null)]
        [TestCase("")]
        public void IsOwned_ReturnsFalseForInvalidKeys(string key)
        {
            Assert.IsFalse(new IapEntitlementsData().IsOwned(key));
        }

        [Test]
        public void SetOwned_AddsAndRemovesKeys_ReportingChanges()
        {
            var data = new IapEntitlementsData();

            Assert.IsTrue(data.SetOwned("remove_ads", true));
            Assert.IsTrue(data.IsOwned("remove_ads"));
            Assert.IsFalse(data.SetOwned("remove_ads", true));
            Assert.AreEqual(1, data.OwnedProductKeys.Count);

            Assert.IsTrue(data.SetOwned("remove_ads", false));
            Assert.IsFalse(data.IsOwned("remove_ads"));
            Assert.IsFalse(data.SetOwned("remove_ads", false));
        }
    }
}
