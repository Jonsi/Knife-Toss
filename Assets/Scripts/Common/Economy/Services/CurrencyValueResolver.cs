using System.Collections.Generic;
using Common.Economy.Data;
using Common.Economy.Interfaces;

namespace Common.Economy.Services
{
    /// <summary>
    /// Implementation of ICurrencyValueProvider.
    /// Resolves currency values by checking remote overrides first, then falling back to the local catalog defaults.
    /// </summary>
    public class CurrencyValueResolver : ICurrencyValueProvider
    {
        private readonly CurrencyTypeCatalog _catalog;
        private readonly Dictionary<string, int> _remoteOverrides;

        /// <summary>
        /// Initializes a new instance of the CurrencyValueResolver.
        /// </summary>
        /// <param name="catalog">The local catalog of currency types and default values.</param>
        public CurrencyValueResolver(CurrencyTypeCatalog catalog)
        {
            _catalog = catalog;
            _remoteOverrides = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets the resolved value for a specific currency type.
        /// Checks for remote override first, then falls back to catalog default.
        /// </summary>
        /// <param name="typeId">The unique identifier of the currency type.</param>
        /// <returns>The resolved value.</returns>
        public int GetValue(string typeId)
        {
            if (_remoteOverrides.TryGetValue(typeId, out var value))
            {
                return value;
            }

            return _catalog.GetDefaultValue(typeId);
        }

        /// <summary>
        /// Checks if a currency type is supported by the catalog.
        /// </summary>
        /// <param name="typeId">The unique identifier to check.</param>
        /// <returns>True if the type exists in the catalog.</returns>
        public bool HasType(string typeId)
        {
            return _catalog.HasType(typeId);
        }

        /// <summary>
        /// Applies remote configuration overrides to currency values.
        /// </summary>
        /// <param name="overrides">Dictionary mapping type IDs to overridden values.</param>
        public void ApplyRemoteOverrides(Dictionary<string, int> overrides)
        {
            if (overrides == null) return;

            foreach (var kvp in overrides)
            {
                _remoteOverrides[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Clears all active remote overrides, reverting to default values.
        /// </summary>
        public void ClearOverrides()
        {
            _remoteOverrides.Clear();
        }
    }
}
