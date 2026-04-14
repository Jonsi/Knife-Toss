using System.Collections.Generic;
using Game.Persistence.Services;
using Game.Pooling;
using Game.Store.Configs;
using Game.Store.Data;
using Game.Store.Rewards;
using UnityEngine;
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
        private readonly List<KnifeSkinConfig> _OrderedConfigs = new List<KnifeSkinConfig>();

        private StoreCatalog _CachedCatalog;
        private PrefabsConfiguration _CachedPrefabsConfiguration;
        private bool _HasResolvedCatalog;
        private bool _HasResolvedPrefabsConfiguration;
        private const string DefaultKnifeDisplayName = "Default";

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

        public string GetEquippedId()
        {
            return _PlayerDataService != null
                ? (_PlayerDataService.EnsureKnifeSkinCollection().EquippedId ?? string.Empty)
                : string.Empty;
        }

        public bool TryEquip(string skinId)
        {
            if (_PlayerDataService == null)
            {
                return false;
            }

            var collection = _PlayerDataService.EnsureKnifeSkinCollection();
            skinId ??= string.Empty;

            if (collection.EquippedId == skinId)
            {
                return true;
            }

            if (!collection.SetEquipped(skinId))
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

        public IReadOnlyList<KnifeSkinSelectionOption> GetSelectableOptions()
        {
            var options = new List<KnifeSkinSelectionOption>
            {
                new KnifeSkinSelectionOption(string.Empty, DefaultKnifeDisplayName, GetDefaultKnifeSprite())
            };

            EnsureConfigLookup();

            for (var i = 0; i < _OrderedConfigs.Count; i++)
            {
                var config = _OrderedConfigs[i];
                if (config == null || string.IsNullOrEmpty(config.Id) || !IsUnlocked(config.Id))
                {
                    continue;
                }

                options.Add(new KnifeSkinSelectionOption(
                    config.Id,
                    config.GetItemDisplayName(),
                    config.GetKnifeSprite(),
                    config));
            }

            return options;
        }

        public int GetSelectedOptionIndex(IReadOnlyList<KnifeSkinSelectionOption> options)
        {
            if (options == null || options.Count == 0)
            {
                return -1;
            }

            var equippedId = GetEquippedId();
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].SkinId == equippedId)
                {
                    return i;
                }
            }

            return 0;
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
                _OrderedConfigs.Add(config);
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

        private Sprite GetDefaultKnifeSprite()
        {
            var prefabsConfiguration = TryResolvePrefabsConfiguration();
            return prefabsConfiguration?.GetDefaultKnifePrefab()?.SpriteRenderer?.sprite;
        }

        private PrefabsConfiguration TryResolvePrefabsConfiguration()
        {
            if (_HasResolvedPrefabsConfiguration)
            {
                return _CachedPrefabsConfiguration;
            }

            _HasResolvedPrefabsConfiguration = true;

            if (_Resolver == null)
            {
                return null;
            }

            try
            {
                _CachedPrefabsConfiguration = _Resolver.Resolve<PrefabsConfiguration>();
            }
            catch
            {
                _CachedPrefabsConfiguration = null;
            }

            return _CachedPrefabsConfiguration;
        }
    }
}
