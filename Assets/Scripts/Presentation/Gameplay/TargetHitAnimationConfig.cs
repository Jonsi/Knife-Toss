using DG.Tweening;
using UnityEngine;

namespace Presentation.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetHitAnimationConfig",
        menuName = "Knife Toss/Configurations/Animations/Target Hit Animation")]
    public sealed class TargetHitAnimationConfig : ScriptableObject
    {
        [Header("Shake")]
        [SerializeField] private float shakeDuration = 0.25f;
        [SerializeField] private float shakeStrength = 15f;
        [SerializeField] private int shakeVibrato = 20;
        [SerializeField, Range(0f, 180f)] private float shakeRandomness = 45f;
        [SerializeField] private bool shakeFadeOut = true;

        [Header("Tint")]
        [SerializeField] private Color tintColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField] private float tintFadeInDuration = 0.05f;
        [SerializeField] private float tintHoldDuration = 0.05f;
        [SerializeField] private float tintFadeOutDuration = 0.2f;
        [SerializeField] private Ease tintEase = Ease.OutQuad;

        public float ShakeDuration => shakeDuration;
        public float ShakeStrength => shakeStrength;
        public int ShakeVibrato => shakeVibrato;
        public float ShakeRandomness => shakeRandomness;
        public bool ShakeFadeOut => shakeFadeOut;

        public Color TintColor => tintColor;
        public float TintFadeInDuration => tintFadeInDuration;
        public float TintHoldDuration => tintHoldDuration;
        public float TintFadeOutDuration => tintFadeOutDuration;
        public Ease TintEase => tintEase;
    }
}
