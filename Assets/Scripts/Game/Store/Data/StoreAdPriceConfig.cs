using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Shared coin amount displayed by store ad CTA views.
    /// </summary>
    [CreateAssetMenu(fileName = "StoreAdPriceConfig", menuName = "Knife Toss/Systems/Store/Store Ad Price Config")]
    public class StoreAdPriceConfig : ScriptableObject
    {
        [field: SerializeField] public int CoinAmount { get; private set; } = 50;
    }
}
