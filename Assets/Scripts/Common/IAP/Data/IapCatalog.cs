using System.Collections.Generic;
using UnityEngine;

namespace Common.IAP.Data
{
    /// <summary>
    /// Catalog of all configured IAP products.
    /// </summary>
    [CreateAssetMenu(fileName = "IapCatalog", menuName = "Knife Toss/IAP/IAP Catalog")]
    public class IapCatalog : ScriptableObject
    {
        [field: SerializeField] public List<IapProduct> Products { get; private set; } = new List<IapProduct>();
    }
}
