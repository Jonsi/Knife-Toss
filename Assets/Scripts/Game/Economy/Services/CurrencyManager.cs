using System.Collections.Generic;
using Game.Economy.Events;
using Game.Economy.Interfaces;
using Game.Persistence.Data;
using Game.Persistence.Services;

namespace Game.Economy.Services
{
    /// <summary>
    /// Manages runtime currency balances, syncs to persisted resources, and raises balance-changed events.
    /// </summary>
    public class CurrencyManager : ICurrencyService
    {
        private const int MinimumAmount = 1;

        private readonly PlayerDataService _PlayerDataService;
        private readonly CurrencyBalanceChangedEventChannel _EventChannel;
        private readonly Dictionary<string, int> _RuntimeBalances = new();

        /// <summary>
        /// Initializes the manager from persisted data and subscribes to the data source.
        /// </summary>
        /// <param name="playerDataService">Service providing current player data.</param>
        /// <param name="eventChannel">Channel to raise when a balance changes.</param>
        public CurrencyManager(
            PlayerDataService playerDataService,
            CurrencyBalanceChangedEventChannel eventChannel)
        {
            _PlayerDataService = playerDataService;
            _EventChannel = eventChannel;
            LoadFromPersistedData();
        }

        /// <inheritdoc />
        public int GetBalance(string currencyId)
        {
            return _RuntimeBalances.TryGetValue(currencyId, out var balance) ? balance : 0;
        }

        /// <inheritdoc />
        public void AddBalance(string currencyId, int amount)
        {
            if (string.IsNullOrEmpty(currencyId) || amount < MinimumAmount)
                return;

            int previous = GetBalance(currencyId);
            int newBalance = previous + amount;
            _RuntimeBalances[currencyId] = newBalance;
            SyncToPersistedData(currencyId);
            RaiseEvent(currencyId, previous, newBalance, amount);
            _PlayerDataService.RequestSave();
        }

        /// <inheritdoc />
        public bool TrySpend(string currencyId, int amount)
        {
            if (string.IsNullOrEmpty(currencyId) || amount < MinimumAmount)
                return false;

            if (!CanAfford(currencyId, amount))
                return false;

            int previous = GetBalance(currencyId);
            int newBalance = previous - amount;
            _RuntimeBalances[currencyId] = newBalance;
            SyncToPersistedData(currencyId);
            RaiseEvent(currencyId, previous, newBalance, -amount);
            _PlayerDataService.RequestSave();
            return true;
        }

        /// <inheritdoc />
        public bool CanAfford(string currencyId, int amount)
        {
            return GetBalance(currencyId) >= amount;
        }

        private void LoadFromPersistedData()
        {
            _RuntimeBalances.Clear();
            var data = _PlayerDataService.GetData();
            if (data?.Resources?.Currencies == null)
                return;

            foreach (var entry in data.Resources.Currencies)
            {
                if (string.IsNullOrEmpty(entry.TypeId))
                    continue;
                _RuntimeBalances[entry.TypeId] = entry.Balance;
            }
        }

        private void SyncToPersistedData(string currencyId)
        {
            var data = _PlayerDataService.GetData();
            if (data?.Resources?.Currencies == null)
                return;

            var list = data.Resources.Currencies;
            var entry = list.Find(e => e.TypeId == currencyId);
            if (entry != null)
            {
                entry.Balance = _RuntimeBalances[currencyId];
            }
            else
            {
                list.Add(new CurrencyBalanceEntry
                {
                    TypeId = currencyId,
                    Balance = _RuntimeBalances[currencyId]
                });
            }
        }

        private void RaiseEvent(string currencyId, int previousBalance, int newBalance, int amountChanged)
        {
            _EventChannel?.Raise(new CurrencyBalanceChangedEventData
            {
                CurrencyId = currencyId,
                PreviousBalance = previousBalance,
                NewBalance = newBalance,
                AmountChanged = amountChanged
            });
        }
    }
}
