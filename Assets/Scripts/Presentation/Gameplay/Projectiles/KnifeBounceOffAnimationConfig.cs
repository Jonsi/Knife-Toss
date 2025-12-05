using DG.Tweening;
using UnityEngine;

namespace Presentation.Gameplay.Projectiles
{
    [CreateAssetMenu(
        fileName = "KnifeBounceOffAnimationConfig",
        menuName = "Knife Toss/Configurations/Animations/Knife Bounce Off Animation")]
    public sealed class KnifeBounceOffAnimationConfig : ScriptableObject
    {
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float impactForce = 0.5f;
        [SerializeField] private float rotationDegrees = 360f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private Ease movementEase = Ease.InQuad;
        [SerializeField] private Ease rotationEase = Ease.OutQuad;

        public float Gravity => Mathf.Max(0f, gravity);
        public float ImpactForce => Mathf.Max(0f, impactForce);
        public float RotationDegrees => rotationDegrees;
        public float RotationSpeed => Mathf.Max(0.01f, rotationSpeed);
        public Ease MovementEase => movementEase;
        public Ease RotationEase => rotationEase;
    }
}
