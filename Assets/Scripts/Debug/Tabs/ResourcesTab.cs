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
                var cheats = TryResolve<EconomyCheats>(resolver);
                string currencyId = GetFirstCurrencyId();
                if (cheats != null && !string.IsNullOrEmpty(currencyId))
                {
                    cheats.SetBalance(currencyId, value);
                }
            }
        }

        [Category("Currency")]
        [DisplayName("Add 100 Coins")]
        public void AddCoins()
        {
            var resolver = CurrentResolver;
            var cheats = TryResolve<EconomyCheats>(resolver);
            string currencyId = GetFirstCurrencyId();
            if (cheats != null && !string.IsNullOrEmpty(currencyId))
            {
                cheats.AddBalance(currencyId, 100);
            }
            RequestRefresh();
        }

        internal override List<OptionDefinition> GetOptions()
        {
            var options = base.GetOptions();
            
            var resolver = CurrentResolver;
            var cheats = TryResolve<PowerUpCheats>(resolver);
            var catalog = cheats?.GetCatalog();
            
            if (catalog != null && cheats != null)
            {
                foreach (var entry in catalog.Entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.InventoryKey)) continue;
                    
                    options.Add(new OptionDefinition
                    {
                        Name = entry.DisplayName,
                        Category = "Power Ups",
                        Target = new PowerUpOptionWrapper(entry.InventoryKey, cheats, this),
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
            private readonly PowerUpCheats _Cheats;
            private readonly ResourcesTab _Tab;

            public PowerUpOptionWrapper(string key, PowerUpCheats cheats, ResourcesTab tab)
            {
                _Key = key;
                _Cheats = cheats;
                _Tab = tab;
            }

            public int Count
            {
                get => _Cheats.GetPowerUpCount(_Key);
                set
                {
                    _Cheats.SetPowerUpCount(_Key, value);
                    _Tab.RequestRefresh();
                }
            }
        }
    }
}
#endif
