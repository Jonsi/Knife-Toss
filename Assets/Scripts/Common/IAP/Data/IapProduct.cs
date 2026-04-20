using UnityEngine;

namespace Common.IAP.Data
{
    /// <summary>
    /// ScriptableObject definition for a platform IAP product.
    /// </summary>
    [CreateAssetMenu(fileName = "IapProduct", menuName = "Knife Toss/IAP/IAP Product")]
    public class IapProduct : ScriptableObject
    {
        [field: SerializeField] public string ProductKey { get; private set; }
        [field: SerializeField] public string StoreProductId { get; private set; }
        [field: SerializeField] public bool IsConsumable { get; private set; }
    }
}
