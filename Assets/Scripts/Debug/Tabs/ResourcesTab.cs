#if TEST_ENABLED
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Game.Flow.Game;
using Game.Persistence.Services;
using Game.PowerUps.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Debugging.Tabs
{
    public class ResourcesTab : OptionsTab
    {
        public override string Key => "Resources";
        public override string Title => "3. Resources";
        public override string IconStyleKey => "Icon_Options";
        public override int SortIndex => -108;

        public ResourcesTab(IObjectResolver resolver)
        {
        }

        private static IObjectResolver CurrentResolver
        {
            get
            {
                var scopes = Object.FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None);
                foreach (var scope in scopes)
                {
                    if (scope.gameObject.scene.name != "DontDestroyOnLoad" && scope.Container != null)
                        return scope.Container;
                }
                return scopes.FirstOrDefault(s => s.Container != null)?.Container;
            }
        }

        private static T TryResolve<T>(IObjectResolver resolver) where T : class
        {
            return resolver?.Resolve<T>();
        }

        [Category("Currency")]
        [DisplayName("Total Tomatoes")]
        public int TotalTomatoes
        {
            get
            {
                var resolver = CurrentResolver;
                return TryResolve<GameData>(resolver)?.TotalCollected ?? 0;
            }
            set
            {
                var resolver = CurrentResolver;
                var gameData = TryResolve<GameData>(resolver);
                if (gameData != null)
                {
                    var prop = typeof(GameData).GetProperty("TotalCollected", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    prop?.SetValue(gameData, value);
                    TryResolve<PlayerDataService>(resolver)?.RequestSave();
                }
            }
        }

        [Category("Currency")]
        [DisplayName("Add 100 Tomatoes")]
        public void AddTomatoes()
        {
            TotalTomatoes += 100;
            RequestRefresh();
        }

        internal override List<OptionDefinition> GetOptions()
        {
            var options = base.GetOptions();
            
            var resolver = CurrentResolver;
            var playerDataService = TryResolve<PlayerDataService>(resolver);
            
            // Just load the fucking catalog
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
