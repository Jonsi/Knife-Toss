using UnityEngine;

namespace Game.Feedback.Audio.Data
{
    /// <summary>
    /// Stores audio clips used by gameplay feedback systems.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Knife Toss/Systems/Audio/Sound Settings")]
    public class SoundSettings : ScriptableObject
    {
        [Header("Knife")]
        [field: SerializeField] public AudioClip KnifeThrowClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float KnifeThrowVolume { get; private set; } = 0.72f;
        [field: SerializeField, Min(0f)] public float KnifeThrowCooldownSeconds { get; private set; } = 0.08f;
        [field: SerializeField] public AudioClip KnifeLoadClip { get; private set; }

        [Header("Impacts")]
        [field: SerializeField] public AudioClip HitTargetClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float HitTargetVolume { get; private set; } = 0.78f;
        [field: SerializeField, Min(0f)] public float HitTargetCooldownSeconds { get; private set; } = 0.1f;
        [field: SerializeField] public AudioClip HitKnifeClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float HitKnifeVolume { get; private set; } = 0.95f;
        [field: SerializeField, Min(0f)] public float HitKnifeCooldownSeconds { get; private set; } = 0.55f;
        [field: SerializeField] public AudioClip HitCollectibleClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float HitCollectibleVolume { get; private set; } = 0.82f;

        [Header("Flow")]
        [field: SerializeField] public AudioClip WinClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float WinVolume { get; private set; } = 0.82f;
        [field: SerializeField] public AudioClip LoseClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float LoseVolume { get; private set; } = 0.58f;

        [Header("UI")]
        [field: SerializeField] public AudioClip ButtonClickClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float ButtonClickVolume { get; private set; } = 0.55f;
        [field: SerializeField] public AudioClip CurrencyGainClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float CurrencyGainVolume { get; private set; } = 0.72f;
        [field: SerializeField] public AudioClip CurrencySpendClip { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float CurrencySpendVolume { get; private set; } = 0.68f;
    }
}
