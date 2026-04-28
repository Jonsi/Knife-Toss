using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.IAP.Tests.TestHelpers
{
    internal static class IapTestHelpers
    {
        public static IapProduct Product(string key, bool consumable = false)
        {
            var product = ScriptableObject.CreateInstance<IapProduct>();
            SetProperty(product, nameof(IapProduct.ProductKey), key);
            SetProperty(product, nameof(IapProduct.StoreProductId), key);
            SetProperty(product, nameof(IapProduct.IsConsumable), consumable);
            return product;
        }

        public static IapSettings Settings(bool enabled = true, float timeoutSeconds = 30f)
        {
            var settings = ScriptableObject.CreateInstance<IapSettings>();
            SetProperty(settings, nameof(IapSettings.IapEnabled), enabled);
            SetProperty(settings, nameof(IapSettings.GlobalTimeoutSeconds), timeoutSeconds);
            return settings;
        }

        public static void SetProperty<T>(object target, string propertyName, T value)
        {
            var field = target.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, propertyName);

            field.SetValue(target, value);
        }
    }

    internal sealed class FakeIapProvider : IIapProvider
    {
        public int InitializeCallCount { get; private set; }
        public int PurchaseCallCount { get; private set; }
        public int RestoreCallCount { get; private set; }

        public Func<IapProduct, CancellationToken, UniTask<IapPurchaseResult>> PurchaseHandler { get; set; }
        public Func<CancellationToken, UniTask<IReadOnlyList<IapProduct>>> RestoreHandler { get; set; }

        public UniTask InitializeAsync(IapCatalog catalog, CancellationToken ct = default)
        {
            InitializeCallCount++;
            return UniTask.CompletedTask;
        }

        public UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default)
        {
            PurchaseCallCount++;
            return PurchaseHandler != null
                ? PurchaseHandler(product, ct)
                : UniTask.FromResult(IapPurchaseResult.Purchased("receipt"));
        }

        public UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default)
        {
            RestoreCallCount++;
            return RestoreHandler != null
                ? RestoreHandler(ct)
                : UniTask.FromResult<IReadOnlyList<IapProduct>>(Array.Empty<IapProduct>());
        }

        public UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default)
        {
            return UniTask.FromResult(new IapProductInfo(product, product != null, false, "$0.99"));
        }

        public void Dispose() { }
    }

    internal sealed class FakeIapEntitlementStore : IIapEntitlementStore
    {
        private readonly Dictionary<string, bool> _owned = new Dictionary<string, bool>();

        public int SetOwnedCallCount { get; private set; }

        public bool HasEntitlement(IapProduct product)
        {
            return product != null &&
                   !string.IsNullOrEmpty(product.ProductKey) &&
                   _owned.TryGetValue(product.ProductKey, out var owned) &&
                   owned;
        }

        public void SetOwned(IapProduct product, bool owned)
        {
            SetOwnedCallCount++;
            if (product == null || string.IsNullOrEmpty(product.ProductKey))
                return;

            _owned[product.ProductKey] = owned;
        }
    }

    internal sealed class FakeIapTelemetryListener : IIapTelemetryListener
    {
        public int StartedCount { get; private set; }
        public int CompletedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int RestoredCount { get; private set; }
        public string LastFailureReason { get; private set; }

        public void OnPurchaseStarted(IapProduct product) => StartedCount++;
        public void OnPurchaseCompleted(IapProduct product, IapPurchaseResult result) => CompletedCount++;
        public void OnPurchaseFailed(IapProduct product, string reason)
        {
            FailedCount++;
            LastFailureReason = reason;
        }

        public void OnPurchasesRestored(int restoredCount) => RestoredCount = restoredCount;
    }
}
