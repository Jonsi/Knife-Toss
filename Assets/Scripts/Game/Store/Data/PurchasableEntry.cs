using System;
using Common.IAP.Data;
using Game.Store.Attributes;
using Game.Store.Rewards;
using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Configuration for a single purchasable store item.
    /// </summary>
    [Serializable]
    public class PurchasableEntry
    {
        [field: SerializeField] public PriceDefinition Price { get; private set; }

        [field: SerializeReference]
        [field: SerializeReferenceDropdown]
        public ItemReward Reward { get; private set; }

        public IapProduct GetIapProduct()
        {
            return Price?.IapProduct;
        }
    }
}
