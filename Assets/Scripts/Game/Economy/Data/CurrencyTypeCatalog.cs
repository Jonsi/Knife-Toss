using System.Collections.Generic;
using Game.Entities.Collectible.Data;
using UnityEngine;

namespace Game.Economy.Data
{
    /// <summary>
    /// Registry of all available currency types in the project.
    /// Provides lookup methods by TypeId.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyTypeCatalog", menuName = "Knife Toss/Currency Type Catalog")]
    public class CurrencyTypeCatalog : ScriptableObject
    {
        [Tooltip("List of all currency type configurations")]
        [SerializeField] private CollectibleCurrencyConfig[] _currencyTypes;

        private Dictionary<string, CollectibleCurrencyConfig> _lookupCache;

        /// <summary>
        /// Initializes the lookup cache if it hasn't been initialized yet.
        /// </summary>
        private void InitializeCache()
        {
            if (_lookupCache != null) return;

            _lookupCache = new Dictionary<string, CollectibleCurrencyConfig>();
            if (_currencyTypes != null)
            {
                foreach (var config in _currencyTypes)
                {
                    if (config != null && !string.IsNullOrEmpty(config.TypeId))
                    {
                        if (!_lookupCache.ContainsKey(config.TypeId))
                        {
                            _lookupCache.Add(config.TypeId, config);
                        }
                        else
                        {
                            Debug.LogWarning($"[CurrencyTypeCatalog] Duplicate Currency Type ID detected: {config.TypeId}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the configuration for a specific currency type ID.
        /// </summary>
        /// <param name="typeId">The unique identifier of the currency type.</param>
        /// <returns>The CollectibleCurrencyConfig if found, otherwise null.</returns>
        public CollectibleCurrencyConfig GetConfig(string typeId)
        {
            InitializeCache();
            if (_lookupCache.TryGetValue(typeId, out var config))
            {
                return config;
            }
            Debug.LogError($"[CurrencyTypeCatalog] Currency type not found: {typeId}");
            return null;
        }

        /// <summary>
        /// Returns a read-only list of all registered currency configurations.
        /// </summary>
        /// <returns>List of CollectibleCurrencyConfig.</returns>
        public IReadOnlyList<CollectibleCurrencyConfig> GetAllConfigs()
        {
            return _currencyTypes;
        }

        /// <summary>
        /// Checks if a currency type exists in the catalog.
        /// </summary>
        /// <param name="typeId">The unique identifier to check.</param>
        /// <returns>True if the type exists, false otherwise.</returns>
        public bool HasType(string typeId)
        {
            InitializeCache();
            return _lookupCache.ContainsKey(typeId);
        }

        /// <summary>
        /// Gets the default value for a specific currency type.
        /// </summary>
        /// <param name="typeId">The unique identifier of the currency type.</param>
        /// <returns>The default value if found, otherwise 0.</returns>
        public int GetDefaultValue(string typeId)
        {
            var config = GetConfig(typeId);
            return config != null ? config.DefaultValue : 0;
        }
    }
}
