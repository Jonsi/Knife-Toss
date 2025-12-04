using DG.Tweening;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetHitAnimationConfig",
        menuName = "Knife Toss/Gameplay/Target Hit Animation")]
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

        public float ShakeDuration => Mathf.Max(0f, shakeDuration);
        public float ShakeStrength => Mathf.Max(0f, shakeStrength);
        public int ShakeVibrato => Mathf.Max(1, shakeVibrato);
        public float ShakeRandomness => Mathf.Clamp(shakeRandomness, 0f, 180f);
        public bool ShakeFadeOut => shakeFadeOut;

        public Color TintColor => tintColor;
        public float TintFadeInDuration => Mathf.Max(0f, tintFadeInDuration);
        public float TintHoldDuration => Mathf.Max(0f, tintHoldDuration);
        public float TintFadeOutDuration => Mathf.Max(0f, tintFadeOutDuration);
        public Ease TintEase => tintEase;
    }
}
