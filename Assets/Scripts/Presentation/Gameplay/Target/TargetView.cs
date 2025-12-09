using System;
using Core.Abstractions;
using Core.Data;
using Presentation.Presentation.Gameplay.Knife;
using UnityEngine;
using VContainer;

namespace Presentation.Presentation.Gameplay.Target
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles rotation via injected rotation behavior.
    /// </summary>
    public class TargetView : MonoBehaviour, ITargetView, IHitable
    {
        private static readonly Type CachedType = typeof(TargetView);

        [SerializeField] private CircleCollider2D _collider;

        private IRotationBehavior _rotationBehavior;
        private IConfigurationProvider<TargetConfiguration> _configurationProvider;
        private float _currentRotation;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets the current rotation angle of the target in degrees.
        /// </summary>
        public float Rotation => _currentRotation;

        /// <summary>
        /// Gets the radius of the target.
        /// </summary>
        public float Radius
        {
            get
            {
                if (_collider != null)
                {
                    return _collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                }
                return _configurationProvider?.GetConfiguration().Radius ?? 1f;
            }
        }

        /// <summary>
        /// Gets the world position of the target center.
        /// </summary>
        public Vector2 Center => transform.position;

        /// <summary>
        /// Gets the transform for parenting stuck knives.
        /// </summary>
        public Transform Transform => transform;

        [Inject]
        private void Construct(
            IConfigurationProvider<TargetConfiguration> configurationProvider,
            IRotationBehavior rotationBehavior)
        {
            _configurationProvider = configurationProvider;
            _rotationBehavior = rotationBehavior;
        }

        private void Update()
        {
            if (_rotationBehavior == null)
                return;

            float rotationDelta = _rotationBehavior.GetRotationDelta(Time.deltaTime);
            _currentRotation += rotationDelta;
            _currentRotation %= 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, _currentRotation);
        }

        /// <summary>
        /// Sticks a knife to this target at the specified position and rotation.
        /// </summary>
        /// <param name="knife">The knife view to stick to this target.</param>
        /// <param name="impactPoint">The world position where the impact occurred.</param>
        public void Stick(KnifeView knife, Vector2 impactPoint)
        {
            if (knife == null)
                return;

            // 1. Parent first to handle relative transforms correctly
            knife.transform.SetParent(Transform);

            // 2. Convert impact point to local space
            Vector2 localPoint = transform.InverseTransformPoint(impactPoint);
            
            // 3. Calculate angle from center to impact point
            float angleDegrees = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

            // 4. Get knife config
            StickPositionData stickData = knife.StickPositionData;

            // 5. Calculate position with offset (radius + penetration/offset)
            float localRadius = _collider != null ? _collider.radius : 1f;
            float finalRadius = localRadius + stickData.Offset;

            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            Vector2 finalPosition = new Vector2(
                Mathf.Cos(angleRadians) * finalRadius,
                Mathf.Sin(angleRadians) * finalRadius
            );

            // 6. Apply Position
            knife.transform.localPosition = finalPosition;

            // 7. Apply Rotation
            // Angle + 180 makes it point TO center.
            // RotationOffset adds random variation or adjustment.
            float finalRotation = angleDegrees + 90 + stickData.RotationOffset;
            knife.transform.localRotation = Quaternion.Euler(0f, 0f, finalRotation);
        }
    }
}
