using System.Collections.Generic;
using Game.Persistence.Services;
using Game.Store.Configs;
using Game.Store.Data;
using Game.Store.Rewards;
using VContainer;

namespace Game.Store.Services
{
    /// <summary>
    /// Central access point for knife skin ownership, equip state, and config lookup.
    /// </summary>
    public class KnifeSkinService
    {
        private readonly PlayerDataService _PlayerDataService;
        private readonly IObjectResolver _Resolver;
        private readonly Dictionary<string, KnifeSkinConfig> _ConfigLookup = new Dictionary<string, KnifeSkinConfig>();

        private StoreCatalog _CachedCatalog;
        private bool _HasResolvedCatalog;

        public KnifeSkinService(PlayerDataService playerDataService, IObjectResolver resolver)
        {
            _PlayerDataService = playerDataService;
            _Resolver = resolver;
        }

        public bool IsUnlocked(string skinId)
        {
            return !string.IsNullOrEmpty(skinId) &&
                _PlayerDataService != null &&
                _PlayerDataService.EnsureKnifeSkinCollection().IsUnlocked(skinId);
        }

        public bool IsEquipped(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || _PlayerDataService == null)
            {
                return false;
            }

            return _PlayerDataService.EnsureKnifeSkinCollection().EquippedId == skinId;
        }

        public string GetEquippedId()
        {
            return _PlayerDataService != null
                ? (_PlayerDataService.EnsureKnifeSkinCollection().EquippedId ?? string.Empty)
                : string.Empty;
        }

        public bool TryEquip(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) || _PlayerDataService == null)
            {
                return false;
            }

            var collection = _PlayerDataService.EnsureKnifeSkinCollection();
            if (!collection.Equip(skinId))
            {
                return false;
            }

            _PlayerDataService.SaveKnifeSkinCollection();
            return true;
        }

        public void ReloadState()
        {
            _PlayerDataService?.ReloadKnifeSkinCollection();
        }

        public KnifeSkinConfig GetEquippedConfig()
        {
            if (_PlayerDataService == null)
            {
                return null;
            }

            return GetConfig(_PlayerDataService.EnsureKnifeSkinCollection().EquippedId);
        }

        public KnifeSkinConfig GetConfig(string skinId)
        {
            if (string.IsNullOrEmpty(skinId))
            {
                return null;
            }

            EnsureConfigLookup();
            return _ConfigLookup.TryGetValue(skinId, out var config) ? config : null;
        }

        private void EnsureConfigLookup()
        {
            var catalog = TryResolveCatalog();
            if (catalog == null)
            {
                return;
            }

            if (_ConfigLookup.Count > 0)
            {
                return;
            }

            var entries = catalog.Skins;
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var reward = entries[i]?.Reward as KnifeSkinReward;
                var config = reward?.Config;
                if (config == null || string.IsNullOrEmpty(config.Id) || _ConfigLookup.ContainsKey(config.Id))
                {
                    continue;
                }

                _ConfigLookup.Add(config.Id, config);
            }
        }

        private StoreCatalog TryResolveCatalog()
        {
            if (_HasResolvedCatalog)
            {
                return _CachedCatalog;
            }

            _HasResolvedCatalog = true;

            if (_Resolver == null)
            {
                return null;
            }

            try
            {
                _CachedCatalog = _Resolver.Resolve<StoreCatalog>();
            }
            catch
            {
                _CachedCatalog = null;
            }

            return _CachedCatalog;
        }
    }
}
