using UnityEngine;

namespace KnifeToss.Core.Data
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "KnifeToss/Sound Settings", order = 0)]
    public sealed class SoundSettings : ScriptableObject
    {
        [Header("Gameplay")]
        [SerializeField] private AudioClip hitTargetSound;
        [SerializeField] private AudioClip hitKnifeSound;

        [Header("Music")]
        [SerializeField] private AudioClip music;

        [Header("UI")]
        [SerializeField] private AudioClip buttonClickSound;

        public AudioClip HitTargetSound => hitTargetSound;
        public AudioClip HitKnifeSound => hitKnifeSound;
        public AudioClip Music => music;
        public AudioClip ButtonClickSound => buttonClickSound;
    }
}
