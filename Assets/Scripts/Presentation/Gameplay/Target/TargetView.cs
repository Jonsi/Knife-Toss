using System;
using Core.Abstractions;
using Core.Data;
using UnityEngine;
using VContainer;

namespace Presentation.Presentation.Gameplay.Target
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles rotation via injected rotation behavior.
    /// Implements IHitable to participate in the collision system.
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
    }
}
