using System;
using System.Collections.Generic;
using System.Threading;
using Common.IAP.Data;
using Common.IAP.Events;
using Common.IAP.Interfaces;
using Cysharp.Threading.Tasks;

namespace Common.IAP.Services
{
    /// <summary>
    /// IAP facade that owns gating, timeout, event dispatch, and entitlement persistence.
    /// </summary>
    public class IapService : IIapService
    {
        private readonly IIapProvider _Provider;
        private readonly IIapEntitlementStore _EntitlementStore;
        private readonly IapSettings _Settings;
        private readonly IapCatalog _Catalog;
        private readonly IapPurchaseStartedEventChannel _StartedChannel;
        private readonly IapPurchaseCompletedEventChannel _CompletedChannel;
        private readonly IapPurchaseFailedEventChannel _FailedChannel;
        private readonly IapPurchaseRestoredEventChannel _RestoredChannel;
        private readonly IIapTelemetryListener _Telemetry;

        private bool _Initialized;
        private bool _IsInitializing;
        private UniTaskCompletionSource _InitializationCompletionSource;
        private bool _IsRestoreInProgress;

        public bool IsPurchaseInProgress { get; private set; }

        public IapService(
            IIapProvider provider,
            IIapEntitlementStore entitlementStore,
            IapSettings settings,
            IapCatalog catalog,
            IapPurchaseStartedEventChannel startedChannel,
            IapPurchaseCompletedEventChannel completedChannel,
            IapPurchaseFailedEventChannel failedChannel,
            IapPurchaseRestoredEventChannel restoredChannel,
            IIapTelemetryListener telemetry)
        {
            _Provider = provider;
            _EntitlementStore = entitlementStore;
            _Settings = settings;
            _Catalog = catalog;
            _StartedChannel = startedChannel;
            _CompletedChannel = completedChannel;
            _FailedChannel = failedChannel;
            _RestoredChannel = restoredChannel;
            _Telemetry = telemetry;
        }

        public IapService(
            IIapProvider provider,
            IIapEntitlementStore entitlementStore,
            IapSettings settings,
            IapCatalog catalog,
            IapPurchaseStartedEventChannel startedChannel,
            IapPurchaseCompletedEventChannel completedChannel,
            IapPurchaseFailedEventChannel failedChannel,
            IapPurchaseRestoredEventChannel restoredChannel)
            : this(provider, entitlementStore, settings, catalog, startedChannel, completedChannel, failedChannel, restoredChannel, null) { }

        public bool IsOwned(IapProduct product)
        {
            return _EntitlementStore != null && _EntitlementStore.HasEntitlement(product);
        }

        public async UniTask<IapPurchaseResult> PurchaseAsync(IapProduct product, CancellationToken ct = default)
        {
            if (IsPurchaseInProgress)
                return IapPurchaseResult.InProgress;

            if (!_Settings.IapEnabled)
                return IapPurchaseResult.Disabled;

            if (product == null)
                return IapPurchaseResult.InvalidProduct;

            if (!product.IsConsumable && IsOwned(product))
            {
                var alreadyOwnedResult = IapPurchaseResult.ProductAlreadyOwned;
                _FailedChannel?.Raise(new IapPurchaseEventData(product, alreadyOwnedResult));
                _Telemetry?.OnPurchaseFailed(product, alreadyOwnedResult.FailureReason);
                return alreadyOwnedResult;
            }

            var timedOut = false;

            try
            {
                IsPurchaseInProgress = true;
                await EnsureInitializedAsync(ct);

                _StartedChannel?.Raise(new IapPurchaseEventData(product, default));
                _Telemetry?.OnPurchaseStarted(product);

                var result = await ExecuteWithTimeoutAsync(
                    token => _Provider.PurchaseAsync(product, token),
                    ct,
                    () => timedOut = true);

                if (result.Success)
                {
                    if (!product.IsConsumable)
                        _EntitlementStore?.SetOwned(product, true);

                    _CompletedChannel?.Raise(new IapPurchaseEventData(product, result));
                    _Telemetry?.OnPurchaseCompleted(product, result);
                    return result;
                }

                if (result.AlreadyOwned)
                {
                    _EntitlementStore?.SetOwned(product, true);
                }

                _FailedChannel?.Raise(new IapPurchaseEventData(product, result));
                _Telemetry?.OnPurchaseFailed(product, result.FailureReason);
                return result;
            }
            catch (OperationCanceledException) when (!timedOut)
            {
                var cancelledResult = IapPurchaseResult.CancelledByUser;
                _FailedChannel?.Raise(new IapPurchaseEventData(product, cancelledResult));
                _Telemetry?.OnPurchaseFailed(product, cancelledResult.FailureReason);
                return cancelledResult;
            }
            catch (OperationCanceledException)
            {
                var timeoutResult = IapPurchaseResult.Timeout;
                _FailedChannel?.Raise(new IapPurchaseEventData(product, timeoutResult));
                _Telemetry?.OnPurchaseFailed(product, timeoutResult.FailureReason);
                return timeoutResult;
            }
            catch (Exception ex)
            {
                var failedResult = IapPurchaseResult.Failed(ex.Message);
                _FailedChannel?.Raise(new IapPurchaseEventData(product, failedResult));
                _Telemetry?.OnPurchaseFailed(product, failedResult.FailureReason);
                return failedResult;
            }
            finally
            {
                IsPurchaseInProgress = false;
            }
        }

        public async UniTask<IReadOnlyList<IapProduct>> RestorePurchasesAsync(CancellationToken ct = default)
        {
            if (IsPurchaseInProgress || _IsRestoreInProgress)
                return Array.Empty<IapProduct>();

            if (!_Settings.IapEnabled)
                return Array.Empty<IapProduct>();

            try
            {
                _IsRestoreInProgress = true;
                await EnsureInitializedAsync(ct);

                var restoredProducts = await ExecuteWithTimeoutAsync(token => _Provider.RestorePurchasesAsync(token), ct);
                if (restoredProducts == null || restoredProducts.Count == 0)
                {
                    _Telemetry?.OnPurchasesRestored(0);
                    _RestoredChannel?.Raise(new IapPurchaseEventData(null, default, Array.Empty<IapProduct>()));
                    return Array.Empty<IapProduct>();
                }

                for (var i = 0; i < restoredProducts.Count; i++)
                {
                    var product = restoredProducts[i];
                    if (product != null && !product.IsConsumable)
                        _EntitlementStore?.SetOwned(product, true);
                }

                _Telemetry?.OnPurchasesRestored(restoredProducts.Count);
                _RestoredChannel?.Raise(new IapPurchaseEventData(null, default, restoredProducts));
                return restoredProducts;
            }
            finally
            {
                _IsRestoreInProgress = false;
            }
        }

        public async UniTask<IapProductInfo> GetProductInfoAsync(IapProduct product, CancellationToken ct = default)
        {
            if (product == null)
                return new IapProductInfo(null, false, false, string.Empty);

            await EnsureInitializedAsync(ct);
            var info = await ExecuteWithTimeoutAsync(token => _Provider.GetProductInfoAsync(product, token), ct);
            var isOwned = IsOwned(product) || info.IsOwned;
            return new IapProductInfo(product, info.IsAvailable, isOwned, info.LocalizedPrice);
        }

        private async UniTask EnsureInitializedAsync(CancellationToken ct)
        {
            if (_Initialized)
                return;

            if (_IsInitializing)
            {
                await _InitializationCompletionSource.Task.AttachExternalCancellation(ct);
                return;
            }

            _IsInitializing = true;
            _InitializationCompletionSource = new UniTaskCompletionSource();

            try
            {
                await _Provider.InitializeAsync(_Catalog, ct);
                _Initialized = true;
                _InitializationCompletionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                _InitializationCompletionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                _IsInitializing = false;
            }
        }

        private UniTask<T> ExecuteWithTimeoutAsync<T>(Func<CancellationToken, UniTask<T>> action, CancellationToken ct)
        {
            return ExecuteWithTimeoutAsync(action, ct, null);
        }

        private async UniTask<T> ExecuteWithTimeoutAsync<T>(Func<CancellationToken, UniTask<T>> action, CancellationToken ct, Action onTimeout)
        {
            if (_Settings.GlobalTimeoutSeconds <= 0f)
                return await action(ct);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_Settings.GlobalTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            try
            {
                return await action(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                onTimeout?.Invoke();
                throw;
            }
        }
    }
}
