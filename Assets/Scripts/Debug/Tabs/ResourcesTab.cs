#if TEST_ENABLED
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Game.Economy.Interfaces;
using VContainer;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using UnityEngine;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Debug tab for resources (currency and power-ups).
    /// </summary>
    public class ResourcesTab : OptionsTab
    {
        public override string Key => "Resources";
        public override string Title => "3. Resources";
        public override string IconStyleKey => "Icon_Options";
        public override int SortIndex => -108;

        public ResourcesTab(IObjectResolver resolver)
        {
        }

        [Category("Currency")]
        [DisplayName("Total Coins")]
        public int TotalCoins
        {
            get
            {
                var resolver = CurrentResolver;
                string currencyId = GetFirstCurrencyId();
                return string.IsNullOrEmpty(currencyId) ? 0 : (TryResolve<ICurrencyService>(resolver)?.GetBalance(currencyId) ?? 0);
            }
            set
            {
                var resolver = CurrentResolver;
                var currencyService = TryResolve<ICurrencyService>(resolver);
                var playerDataService = TryResolve<PlayerDataService>(resolver);
                string currencyId = GetFirstCurrencyId();
                if (currencyService != null && playerDataService != null && value >= 0 && !string.IsNullOrEmpty(currencyId))
                {
                    int current = currencyService.GetBalance(currencyId);
                    int diff = value - current;
                    if (diff > 0)
                        currencyService.AddBalance(currencyId, diff);
                    else if (diff < 0)
                        currencyService.TrySpend(currencyId, -diff);
                    playerDataService.RequestSave();
                }
            }
        }

        [Category("Currency")]
        [DisplayName("Add 100 Coins")]
        public void AddCoins()
        {
            TotalCoins += 100;
            RequestRefresh();
        }

        internal override List<OptionDefinition> GetOptions()
        {
            var options = base.GetOptions();
            
            var resolver = CurrentResolver;
            var playerDataService = TryResolve<PlayerDataService>(resolver);
            
            var catalog = Resources.FindObjectsOfTypeAll<PowerUpCatalog>().FirstOrDefault();
            
            if (catalog != null && playerDataService != null)
            {
                foreach (var entry in catalog.Entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.InventoryKey)) continue;
                    
                    options.Add(new OptionDefinition
                    {
                        Name = entry.DisplayName,
                        Category = "Power Ups",
                        Target = new PowerUpOptionWrapper(entry.InventoryKey, playerDataService),
                        PropertyInfo = typeof(PowerUpOptionWrapper).GetProperty("Count"),
                        IsMethod = false
                    });
                }
            }
            
            return options;
        }

        private class PowerUpOptionWrapper
        {
            private readonly string _Key;
            private readonly PlayerDataService _PlayerDataService;

            public PowerUpOptionWrapper(string key, PlayerDataService playerDataService)
            {
                _Key = key;
                _PlayerDataService = playerDataService;
            }

            public int Count
            {
                get => _PlayerDataService.EnsurePowerUpInventory().GetCount(_Key);
                set
                {
                    _PlayerDataService.EnsurePowerUpInventory().SetCount(_Key, value);
                    _PlayerDataService.RequestSave();
                }
            }
        }
    }
}
#endif
