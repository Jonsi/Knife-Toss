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
        [field: SerializeField] public AudioClip KnifeLoadClip { get; private set; }
        [Header("Impacts")]
        [field: SerializeField] public AudioClip HitTargetClip { get; private set; }
        [field: SerializeField] public AudioClip HitKnifeClip { get; private set; }
        [field: SerializeField] public AudioClip HitCollectibleClip { get; private set; }
        [Header("Flow")]
        [field: SerializeField] public AudioClip WinClip { get; private set; }
        [field: SerializeField] public AudioClip LoseClip { get; private set; }
        [Header("UI")]
        [field: SerializeField] public AudioClip ButtonClickClip { get; private set; }
        [field: SerializeField] public AudioClip CurrencyGainClip { get; private set; }
        [field: SerializeField] public AudioClip CurrencySpendClip { get; private set; }
    }
}
