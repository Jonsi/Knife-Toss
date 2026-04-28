using System;
using Common.IAP.Data;
using Common.IAP.Interfaces;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.Store.Services
{
    /// <summary>
    /// Routes store purchases to either the existing soft-currency service or IAP service.
    /// </summary>
    public class StorePurchaseRouter : IStorePurchaseRouter
    {
        private readonly IPurchaseService _PurchaseService;
        private readonly IIapService _IapService;
        private readonly IRewardGrantContext _GrantContext;
        private readonly PurchaseStartedEventChannel _StartedEventChannel;
        private readonly PurchaseCompletedEventChannel _CompletedEventChannel;
        private readonly PurchaseFailedEventChannel _FailedEventChannel;

        public bool IsPurchaseInProgress =>
            (_PurchaseService != null && _PurchaseService.IsPurchaseInProgress) ||
            (_IapService != null && _IapService.IsPurchaseInProgress);

        public StorePurchaseRouter(
            IPurchaseService purchaseService,
            IIapService iapService,
            IRewardGrantContext grantContext,
            PurchaseStartedEventChannel startedEventChannel,
            PurchaseCompletedEventChannel completedEventChannel,
            PurchaseFailedEventChannel failedEventChannel)
        {
            _PurchaseService = purchaseService;
            _IapService = iapService;
            _GrantContext = grantContext;
            _StartedEventChannel = startedEventChannel;
            _CompletedEventChannel = completedEventChannel;
            _FailedEventChannel = failedEventChannel;
        }

        public StorePurchaseRouter(
            IPurchaseService purchaseService,
            IRewardGrantContext grantContext,
            PurchaseStartedEventChannel startedEventChannel,
            PurchaseCompletedEventChannel completedEventChannel,
            PurchaseFailedEventChannel failedEventChannel)
            : this(
                purchaseService,
                null,
                grantContext,
                startedEventChannel,
                completedEventChannel,
                failedEventChannel) { }

        public bool CanPurchase(PurchasableEntry entry)
        {
            if (entry == null || entry.Reward == null || IsPurchaseInProgress)
                return false;

            if (entry.Reward is IUniqueReward uniqueReward && uniqueReward.IsOwned(_GrantContext))
                return false;

            if (!IsIapEntry(entry))
                return _PurchaseService != null && _PurchaseService.CanPurchase(entry);

            var product = entry.GetIapProduct();
            if (_IapService == null || product == null)
                return false;

            return !_IapService.IsOwned(product) || product.IsConsumable;
        }

        public async UniTask<PurchaseResult> TryPurchaseAsync(PurchasableEntry entry)
        {
            if (entry == null || entry.Reward == null)
                return Fail(entry, PurchaseResult.InvalidItem);

            if (!IsIapEntry(entry))
            {
                if (_PurchaseService == null)
                    return Fail(entry, PurchaseResult.Error("Purchase service unavailable."));

                return await _PurchaseService.TryPurchaseAsync(entry);
            }

            if (_IapService == null)
                return Fail(entry, PurchaseResult.Error("IAP service unavailable."));

            if (IsPurchaseInProgress)
                return Fail(entry, PurchaseResult.PurchaseInProgress);

            var product = entry.GetIapProduct();
            if (product == null)
                return Fail(entry, PurchaseResult.InvalidItem);

            if (entry.Reward is IUniqueReward uniqueReward && uniqueReward.IsOwned(_GrantContext))
                return Fail(entry, PurchaseResult.Error(uniqueReward.AlreadyOwnedReason));

            if (!product.IsConsumable && _IapService.IsOwned(product))
                return Fail(entry, PurchaseResult.AlreadyOwned);

            try
            {
                _StartedEventChannel?.Raise(new PurchaseStartedEventData(entry));

                var iapResult = await _IapService.PurchaseAsync(product);
                if (!iapResult.Success)
                    return Fail(entry, MapIapFailure(iapResult));

                try
                {
                    await entry.Reward.GrantAsync(_GrantContext);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"IAP purchase succeeded but reward grant failed for product {product.ProductKey}: {ex}");
                    return Fail(entry, PurchaseResult.Error("Reward grant failed - entitlement preserved. Please restart and contact support."));
                }

                _CompletedEventChannel?.Raise(new PurchaseCompletedEventData(entry));
                return PurchaseResult.Succeeded;
            }
            catch (Exception ex)
            {
                return Fail(entry, PurchaseResult.Error(ex.Message));
            }
        }

        public async UniTask<int> RestorePurchasesAsync()
        {
            if (_IapService == null)
                return 0;

            var restored = await _IapService.RestorePurchasesAsync();
            return restored?.Count ?? 0;
        }

        private PurchaseResult Fail(PurchasableEntry entry, PurchaseResult result)
        {
            _FailedEventChannel?.Raise(new PurchaseFailedEventData(entry, result.FailureReason));
            return result;
        }

        private static PurchaseResult MapIapFailure(IapPurchaseResult iapResult)
        {
            if (iapResult.AlreadyOwned)
                return PurchaseResult.AlreadyOwned;

            if (iapResult.Cancelled)
                return PurchaseResult.Cancelled;

            return PurchaseResult.Error(iapResult.FailureReason);
        }

        private static bool IsIapEntry(PurchasableEntry entry)
        {
            return entry?.GetIapProduct() != null || entry?.Reward is RemoveAdsReward;
        }
    }
}
