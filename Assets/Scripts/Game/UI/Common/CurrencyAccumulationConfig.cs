using UnityEngine;

namespace Game.UI.Common
{
    /// <summary>
    /// Shared settings for coin accumulation bursts.
    /// </summary>
    [CreateAssetMenu(fileName = "CurrencyAccumulationConfig", menuName = "Knife Toss/UI/Currency Accumulation Config")]
    public class CurrencyAccumulationConfig : ScriptableObject
    {
        [field: SerializeField] public Sprite IconSprite { get; private set; }
        [field: SerializeField, Min(1)] public int MinCoinCount { get; private set; } = 3;
        [field: SerializeField, Min(1)] public int MaxCoinCount { get; private set; } = 8;
        [field: SerializeField, Min(0f)] public float SpawnDelayStep { get; private set; } = 0.05f;
        [field: SerializeField, Min(0f)] public float PopDuration { get; private set; } = 0.16f;
        [field: SerializeField, Min(0f)] public float TravelDuration { get; private set; } = 0.45f;
        [field: SerializeField, Min(0f)] public float TravelDurationStep { get; private set; } = 0.025f;
        [field: SerializeField, Min(0f)] public float ScatterRadius { get; private set; } = 44f;
        [field: SerializeField, Min(0f)] public float IconSize { get; private set; } = 28f;
        [field: SerializeField, Min(0f)] public float EndScale { get; private set; } = 0.45f;
    }
}
