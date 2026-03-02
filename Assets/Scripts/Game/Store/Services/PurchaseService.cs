using System;
using Cysharp.Threading.Tasks;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Store.Data;
using Game.Store.Events;
using Game.Store.Interfaces;
using Game.Store.Rewards;
using VContainer;

namespace Game.Store.Services
{
    /// <summary>
    /// Handles validation, spending, reward grant, and purchase event dispatching.
    /// </summary>
    public class PurchaseService : IPurchaseService
    {
        private readonly ICurrencyService _CurrencyService;
        private readonly IObjectResolver _Resolver;
        private readonly PurchaseStartedEventChannel _StartedEventChannel;
        private readonly PurchaseCompletedEventChannel _CompletedEventChannel;
        private readonly PurchaseFailedEventChannel _FailedEventChannel;

        public bool IsPurchaseInProgress { get; private set; }

        public PurchaseService(
            ICurrencyService currencyService,
            IObjectResolver resolver,
            PurchaseStartedEventChannel startedEventChannel,
            PurchaseCompletedEventChannel completedEventChannel,
            PurchaseFailedEventChannel failedEventChannel)
        {
            _CurrencyService = currencyService;
            _Resolver = resolver;
            _StartedEventChannel = startedEventChannel;
            _CompletedEventChannel = completedEventChannel;
            _FailedEventChannel = failedEventChannel;
        }

        public bool CanPurchase(PurchasableEntry entry)
        {
            if (IsPurchaseInProgress)
                return false;

            if (entry == null || entry.Reward == null)
                return false;

            if (entry.Reward is IUniqueReward uniqueReward && uniqueReward.IsOwned(_Resolver))
                return false;

            var price = entry.Price;
            if (price == null || price.IsFree)
                return true;

            return _CurrencyService.CanAfford(price.Currency.ToTypeId(), price.Amount);
        }

        public async UniTask<PurchaseResult> TryPurchaseAsync(PurchasableEntry entry)
        {
            if (IsPurchaseInProgress)
                return PurchaseResult.PurchaseInProgress;

            if (entry == null || entry.Reward == null)
                return Fail(entry, PurchaseResult.InvalidItem);

            if (entry.Reward is IUniqueReward uniqueReward && uniqueReward.IsOwned(_Resolver))
            {
                return Fail(entry, PurchaseResult.Error(uniqueReward.AlreadyOwnedReason));
            }

            var price = entry.Price;
            var isFree = price == null || price.IsFree;

            if (!isFree)
            {
                if (!_CurrencyService.CanAfford(price.Currency.ToTypeId(), price.Amount))
                    return Fail(entry, PurchaseResult.InsufficientFunds);
            }

            try
            {
                IsPurchaseInProgress = true;
                _StartedEventChannel?.Raise(new PurchaseStartedEventData(entry));

                if (!isFree)
                {
                    if (!_CurrencyService.TrySpend(price.Currency.ToTypeId(), price.Amount))
                        return Fail(entry, PurchaseResult.InsufficientFunds);
                }

                await entry.Reward.GrantAsync(_Resolver);

                _CompletedEventChannel?.Raise(new PurchaseCompletedEventData(entry));
                return PurchaseResult.Succeeded;
            }
            catch (Exception ex)
            {
                if (!isFree)
                {
                    _CurrencyService.AddBalance(price.Currency.ToTypeId(), price.Amount);
                }

                return Fail(entry, PurchaseResult.Error(ex.Message));
            }
            finally
            {
                IsPurchaseInProgress = false;
            }
        }

        private PurchaseResult Fail(PurchasableEntry entry, PurchaseResult result)
        {
            _FailedEventChannel?.Raise(new PurchaseFailedEventData(entry, result.FailureReason));
            return result;
        }
    }
}
