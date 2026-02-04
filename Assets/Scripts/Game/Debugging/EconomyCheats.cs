#if TEST_ENABLED
using System.Linq;
using Game.Economy.Interfaces;
using Game.Persistence.Services;
using Game.Persistence.Storage;
using UnityEngine;

namespace Game.Debugging
{
    public class EconomyCheats
    {
        private readonly ICurrencyService _currencyService;
        private readonly PlayerDataService _playerDataService;

        public EconomyCheats(ICurrencyService currencyService, PlayerDataService playerDataService)
        {
            _currencyService = currencyService;
            _playerDataService = playerDataService;
        }

        public void SetBalance(string currencyId, int value)
        {
            if (Application.isPlaying)
            {
                if (_currencyService == null || string.IsNullOrEmpty(currencyId)) return;
                
                int current = _currencyService.GetBalance(currencyId);
                int diff = value - current;
                
                if (diff > 0)
                    _currencyService.AddBalance(currencyId, diff);
                else if (diff < 0)
                    _currencyService.TrySpend(currencyId, -diff);
                    
                _playerDataService?.RequestSave();
            }
            else
            {
                var store = new FilePlayerDataStore();
                var data = store.Load();
                if (data?.Resources?.Currencies != null)
                {
                    var entry = data.Resources.Currencies.FirstOrDefault(c => c.TypeId == currencyId);
                    if (entry != null)
                    {
                        entry.Balance = value;
                    }
                    else
                    {
                        data.Resources.Currencies.Add(new Game.Persistence.Data.CurrencyBalanceEntry { TypeId = currencyId, Balance = value });
                    }
                    store.Save(data);
                }
            }
        }

        public void AddBalance(string currencyId, int amount)
        {
            if (Application.isPlaying)
            {
                if (_currencyService == null || string.IsNullOrEmpty(currencyId)) return;
                _currencyService.AddBalance(currencyId, amount);
                _playerDataService?.RequestSave();
            }
            else
            {
                var store = new FilePlayerDataStore();
                var data = store.Load();
                if (data?.Resources?.Currencies != null)
                {
                    var entry = data.Resources.Currencies.FirstOrDefault(c => c.TypeId == currencyId);
                    if (entry != null)
                    {
                        entry.Balance += amount;
                    }
                    else
                    {
                        data.Resources.Currencies.Add(new Game.Persistence.Data.CurrencyBalanceEntry { TypeId = currencyId, Balance = amount });
                    }
                    store.Save(data);
                }
            }
        }
    }
}
#endif
