#if TEST_ENABLED
using System;
using System.Collections.Generic;
using System.Threading;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;

namespace Common.IAP.Providers
{
    /// <summary>
    /// QA mock IAP provider with simulated delay, forced outcomes, and seeded ownership.
    /// </summary>
    public class MockIapProvider : IIapProvider
    {
        private readonly MockIapSettings _Settings;
        private readonly HashSet<string> _OwnedProductKeys = new HashSet<string>();
        private readonly Dictionary<string, int> _SequenceIndices = new Dictionary<string, int>();

        private IapCatalog _Catalog;
        private IapMockOutcome? _ForcedNextOutcome;

        public Func<IapProduct, UniTaskCompletionSource<IapPurchaseResult>> PopupCallback { get; set; }

        public MockIapProvider(MockIapSettings settings)
        {
            _Settings = settings;
        }

        public UniTask InitializeAsync(IapCatalog catalog, CancellationToken ct = default)
        {
            _Catalog = catalog;
            _OwnedProductKeys.Clear();
            _SequenceIndices.Clear();

            if (_Settings.SeededOwnedProducts != null)
            {
                for (var i = 0; i < _Settings.SeededOwnedProducts.Count; i++)
                {
                    var seeded = _Settings.SeededOwnedProducts[i];
                    if (!string.IsNullOrEmpty(seeded?.ProductKey))
                        _OwnedProductKeys.Add(seeded.ProductKey);
                }
            }

            return UniTask.CompletedTask;
        }

        public async UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default)
        {
            if (product == null)
                return IapPurchaseResult.InvalidProduct;

            if (PopupCallback != null)
            {
                var popupTcs = PopupCallback.Invoke(product);
                var popupResult = await popupTcs.Task;
                return ApplyOwnership(product, popupResult);
            }

            await SimulateDelayAsync(ct);

            if (!product.IsConsumable && _OwnedProductKeys.Contains(product.ProductKey))
                return IapPurchaseResult.ProductAlreadyOwned;

            var outcome = ResolveNextOutcome(product);
            var result = outcome.ToPurchaseResult(CreateReceipt(product));
            return ApplyOwnership(product, result);
        }

        public async UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default)
        {
            await SimulateDelayAsync(ct);

            var restored = new List<IapProduct>();
            if (_Catalog?.Products == null)
                return restored;

            for (var i = 0; i < _Catalog.Products.Count; i++)
            {
                var product = _Catalog.Products[i];
                if (product != null && !product.IsConsumable && _OwnedProductKeys.Contains(product.ProductKey))
                    restored.Add(product);
            }

            return restored;
        }

        public UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default)
        {
            var isOwned = product != null && _OwnedProductKeys.Contains(product.ProductKey);
            var price = _Settings != null ? _Settings.DefaultLocalizedPriceLabel : string.Empty;
            return UniTask.FromResult(new IapProductInfo(product, product != null, isOwned, price));
        }

        public void ForceNextOutcome(IapMockOutcome outcome) => _ForcedNextOutcome = outcome;

        public void SetOwnedForDebug(IapProduct product, bool owned)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductKey))
                return;

            if (owned)
                _OwnedProductKeys.Add(product.ProductKey);
            else
                _OwnedProductKeys.Remove(product.ProductKey);
        }

        public void Dispose()
        {
            _OwnedProductKeys.Clear();
            _SequenceIndices.Clear();
            PopupCallback = null;
        }

        private IapPurchaseResult ApplyOwnership(IapProduct product, IapPurchaseResult result)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductKey))
                return result;

            if (!product.IsConsumable && (result.Success || result.AlreadyOwned))
                _OwnedProductKeys.Add(product.ProductKey);

            return result;
        }

        private UniTask SimulateDelayAsync(CancellationToken ct)
        {
            return UniTask.Delay(TimeSpan.FromSeconds(_Settings.SimulatedDelaySeconds), cancellationToken: ct);
        }

        private IapMockOutcome ResolveNextOutcome(IapProduct product)
        {
            if (_ForcedNextOutcome.HasValue)
            {
                var forced = _ForcedNextOutcome.Value;
                _ForcedNextOutcome = null;
                return forced;
            }

            var sequenced = _Settings.SequencedOutcomes;
            if (sequenced == null || sequenced.Count == 0)
                return _Settings.DefaultOutcome;

            var productKey = product?.ProductKey ?? string.Empty;
            if (!_SequenceIndices.TryGetValue(productKey, out var index))
                index = 0;

            var outcome = sequenced[index % sequenced.Count];
            _SequenceIndices[productKey] = (index + 1) % sequenced.Count;
            return outcome;
        }

        private static string CreateReceipt(IapProduct product)
        {
            var key = product != null && !string.IsNullOrEmpty(product.ProductKey) ? product.ProductKey : "iap";
            return $"mock-receipt:{key}:{Guid.NewGuid():N}";
        }
    }
}
#endif
