using Common.IAP.Data;
using NUnit.Framework;

namespace Common.IAP.Tests
{
    public class IapPurchaseResultTests
    {
        [Test]
        public void PurchasedAndRestored_SetDistinctRestoreFlags()
        {
            var purchased = IapPurchaseResult.Purchased("receipt");
            var restored = IapPurchaseResult.Restored("receipt");

            Assert.IsTrue(purchased.Success);
            Assert.IsFalse(purchased.IsRestored);
            Assert.IsTrue(restored.Success);
            Assert.IsTrue(restored.IsRestored);
        }

        [Test]
        public void Failed_WithNullReason_UsesDefaultMessage()
        {
            var result = IapPurchaseResult.Failed(null);

            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.FailureReason);
        }

        [Test]
        public void DefaultStruct_IsNotSuccessful()
        {
            var result = default(IapPurchaseResult);

            Assert.IsFalse(result.Success);
            Assert.IsFalse(result.Cancelled);
            Assert.IsFalse(result.Deferred);
            Assert.IsFalse(result.AlreadyOwned);
            Assert.IsFalse(result.IsRestored);
        }
    }
}
