using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Catalog containing all configured store entries.
    /// </summary>
    [CreateAssetMenu(fileName = "StoreCatalog", menuName = "Knife Toss/Systems/Store/Store Catalog")]
    public class StoreCatalog : ScriptableObject
    {
        [field: SerializeField] public List<PurchasableEntry> PowerUps { get; private set; } = new List<PurchasableEntry>();
        [field: SerializeField] public List<PurchasableEntry> CurrencyPacks { get; private set; } = new List<PurchasableEntry>();
        [field: SerializeField] public List<PurchasableEntry> Skins { get; private set; } = new List<PurchasableEntry>();
        [field: SerializeField] public List<PurchasableEntry> IapEntries { get; private set; } = new List<PurchasableEntry>();

        public IEnumerable<PurchasableEntry> AllEntries => PowerUps.Concat(CurrencyPacks).Concat(Skins).Concat(IapEntries);
    }
}
