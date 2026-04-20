using System;
using Common.IAP.Data;
using Game.Economy.Data;
using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Currency price definition for a store entry.
    /// </summary>
    [Serializable]
    public class PriceDefinition
    {
        [field: SerializeField] public CurrencyType Currency { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }
        [field: SerializeField] public IapProduct IapProduct { get; private set; }

        public bool IsIapPrice => IapProduct != null;
        public bool IsFree => !IsIapPrice && (Amount <= 0 || Currency == CurrencyType.None);
    }
}
