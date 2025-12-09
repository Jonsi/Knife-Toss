using System;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using Presentation.Gameplay.Knife;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay.Target
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles rotation via injected rotation behavior.
    /// </summary>
    public class TargetView : MonoBehaviour, ITargetView, IHitable
    {
        private static readonly Type CachedType = typeof(TargetView);
        private const float KnifeRotationFacingCenter = 90f;

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

            AttachKnife(knife);
            ApplyStickTransform(knife, impactPoint);
        }

        private void AttachKnife(KnifeView knife) => knife.transform.SetParent(Transform);

        private void ApplyStickTransform(KnifeView knife, Vector2 impactPoint)
        {
            Vector2 localPoint = TransformImpactToLocal(impactPoint);
            float angleDegrees = CalculateImpactAngle(localPoint);
            StickPositionData stickData = knife.StickPositionData;
            Vector2 finalPosition = CalculateStickPosition(stickData.Offset, angleDegrees);
            float finalRotation = CalculateStickRotation(angleDegrees, stickData.RotationOffset);

            knife.transform.localPosition = finalPosition;
            knife.transform.localRotation = Quaternion.Euler(0f, 0f, finalRotation);
        }

        private Vector2 TransformImpactToLocal(Vector2 impactPoint) => transform.InverseTransformPoint(impactPoint);

        private static float CalculateImpactAngle(Vector2 localPoint) => Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

        private Vector2 CalculateStickPosition(float offset, float angleDegrees)
        {
            float finalRadius = CalculateStickRadius(offset);
            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(angleRadians) * finalRadius,
                Mathf.Sin(angleRadians) * finalRadius
            );
        }

        private float CalculateStickRadius(float offset)
        {
            if (_collider == null)
                return 1f + offset;

            float scaledRadius = _collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            return scaledRadius + offset;
        }

        private static float CalculateStickRotation(float angleDegrees, float rotationOffset)
        {
            return angleDegrees + KnifeRotationFacingCenter + rotationOffset;
        }
    }
}