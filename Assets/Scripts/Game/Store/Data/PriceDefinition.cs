using System;
using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Currency price definition for a store entry.
    /// </summary>
    [Serializable]
    public class PriceDefinition
    {
        [field: SerializeField] public string CurrencyId { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }

        public bool IsFree => Amount <= 0 || string.IsNullOrEmpty(CurrencyId);
    }
}
