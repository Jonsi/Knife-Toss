using UnityEngine;

namespace Common.Economy.Data
{
    /// <summary>
    /// Configuration for a single currency type.
    /// Defines the unique identifier and default value.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyTypeConfig", menuName = "Economy/Currency Type Config")]
    public class CurrencyTypeConfig : ScriptableObject
    {
        /// <summary>
        /// Gets the unique identifier for this currency type.
        /// </summary>
        [field: Tooltip("Unique identifier for this currency type (e.g., 'tier_1')")]
        [field: SerializeField] public string TypeId { get; private set; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        [field: Tooltip("Default value/worth of this currency unit if no remote override exists")]
        [field: SerializeField] public int DefaultValue { get; private set; }
    }
}