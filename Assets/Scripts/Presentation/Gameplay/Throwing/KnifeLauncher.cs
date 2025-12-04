using KnifeToss.Core.Abstractions.Factories;
using KnifeToss.Presentation.Gameplay;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Throwing
{
    /// <summary>
    /// Handles launching knives. Prepares and activates knives for throwing.
    /// </summary>
    public sealed class KnifeLauncher : MonoBehaviour
    {
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private float throwSpeed = 10f;

        private Transform _knivesParent;
        private IKnifeFactory _knifeFactory;
        private ITargetScaleProvider _scaleProvider;
        private Vector3 _prefabOriginalScale = Vector3.one;
        private Color _prefabOriginalColor = Color.white;

        public void Initialize(
            Transform launcher,
            Transform knivesParent,
            IKnifeFactory knifeFactory,
            ITargetScaleProvider scaleProvider,
            Vector3 prefabScale,
            Color prefabColor)
        {
            launcherTransform = launcher;
            _knivesParent = knivesParent;
            _knifeFactory = knifeFactory;
            _scaleProvider = scaleProvider;
            _prefabOriginalScale = prefabScale;
            _prefabOriginalColor = prefabColor;
        }

        public Knife Launch(Vector2 direction)
        {
            if (_knifeFactory == null || launcherTransform == null)
            {
                return null;
            }

            Knife knife = _knifeFactory.Create();
            if (knife == null)
            {
                return null;
            }

            PrepareKnifeForThrow(knife);
            knife.Initialize(GetThrower(), throwSpeed, direction);
            return knife;
        }

        public void SetThrowSpeed(float speed)
        {
            throwSpeed = speed;
        }

        public void SetKnivesParent(Transform parent)
        {
            _knivesParent = parent;
        }

        public void SetScaleProvider(ITargetScaleProvider scaleProvider)
        {
            _scaleProvider = scaleProvider;
        }

        private void PrepareKnifeForThrow(Knife knife)
        {
            Transform knifeTransform = knife.transform;
            knifeTransform.SetParent(_knivesParent);

            // Position at launcher
            knifeTransform.position = launcherTransform.position;
            knifeTransform.rotation = launcherTransform.rotation;

            // Apply scale
            float scaleFactor = _scaleProvider != null ? _scaleProvider.PrimaryScale : 1f;
            knifeTransform.localScale = _prefabOriginalScale * scaleFactor;

            // Enable colliders
            var collision = knife.GetComponent<KnifeCollision>();
            collision?.Enable();
        }

        private KnifeThrower GetThrower()
        {
            return GetComponentInParent<KnifeThrower>();
        }
    }
}

