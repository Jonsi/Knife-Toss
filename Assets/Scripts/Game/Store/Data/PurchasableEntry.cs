using System;
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
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public PriceDefinition Price { get; private set; }

        [field: SerializeReference]
        [field: SerializeReferenceDropdown]
        public ItemReward Reward { get; private set; }
    }
}
