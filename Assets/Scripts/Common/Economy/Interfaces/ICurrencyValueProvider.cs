using System.Collections.Generic;

namespace Common.Economy.Interfaces
{
    /// <summary>
    /// Interface for resolving currency values.
    /// Allows abstraction over local defaults and remote config overrides.
    /// </summary>
    public interface ICurrencyValueProvider
    {
        /// <summary>
        /// Gets the resolved value for a specific currency type.
        /// </summary>
        /// <param name="typeId">The unique identifier of the currency type.</param>
        /// <returns>The resolved value (remote override or default).</returns>
        int GetValue(string typeId);

        /// <summary>
        /// Checks if a currency type is supported/recognized.
        /// </summary>
        /// <param name="typeId">The unique identifier to check.</param>
        /// <returns>True if supported, false otherwise.</returns>
        bool HasType(string typeId);

        /// <summary>
        /// Applies remote configuration overrides to currency values.
        /// </summary>
        /// <param name="overrides">Dictionary mapping type IDs to overridden values.</param>
        void ApplyRemoteOverrides(Dictionary<string, int> overrides);

        /// <summary>
        /// Clears all active remote overrides, reverting to default values.
        /// </summary>
        void ClearOverrides();
    }
}
