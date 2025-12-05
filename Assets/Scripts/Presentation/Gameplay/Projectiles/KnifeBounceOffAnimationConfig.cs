using DG.Tweening;
using UnityEngine;

namespace Presentation.Gameplay.Projectiles
{
    [CreateAssetMenu(
        fileName = "KnifeBounceOffAnimationConfig",
        menuName = "Knife Toss/Configurations/Animations/Knife Bounce Off Animation")]
    public sealed class KnifeBounceOffAnimationConfig : ScriptableObject
    {
        [SerializeField] private float fallDistance = 4f;
        [SerializeField] private float fallDuration = 0.6f;
        [SerializeField] private float horizontalDrift = 0.5f;
        [SerializeField] private float rotationDegrees = 360f;
        [SerializeField] private float rotationDuration = 0.6f;
        [SerializeField] private Ease movementEase = Ease.InQuad;
        [SerializeField] private Ease rotationEase = Ease.OutQuad;

        public float FallDistance => Mathf.Max(0f, fallDistance);
        public float FallDuration => Mathf.Max(0.01f, fallDuration);
        public float HorizontalDrift => Mathf.Max(0f, horizontalDrift);
        public float RotationDegrees => rotationDegrees;
        public float RotationDuration => Mathf.Max(0.01f, rotationDuration);
        public Ease MovementEase => movementEase;
        public Ease RotationEase => rotationEase;
    }
}
