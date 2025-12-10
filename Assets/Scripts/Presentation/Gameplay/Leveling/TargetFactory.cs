using System;
using UnityEngine;
using KnifeToss.Presentation.Gameplay.Target;
using VContainer;
using KnifeToss.Core.Data.Target;
using Infrastructure.Leveling; // For RotationBehaviorFactory

namespace KnifeToss.Presentation.Gameplay.Leveling
{
    /// <summary>
    /// Factory responsible for instantiating and configuring target objects.
    /// </summary>
    public class TargetFactory
    {
        private readonly IObjectResolver _objectResolver;
        private readonly RotationBehaviorFactory _rotationBehaviorFactory;
        private readonly TargetView _defaultPrefab;

        public TargetFactory(
            IObjectResolver objectResolver,
            RotationBehaviorFactory rotationBehaviorFactory,
            TargetView defaultPrefab)
        {
            _objectResolver = objectResolver;
            _rotationBehaviorFactory = rotationBehaviorFactory;
            _defaultPrefab = defaultPrefab;
        }

        /// <summary>
        /// Creates a target instance at the specified orbit position.
        /// </summary>
        /// <param name="config">Configuration for the target.</param>
        /// <param name="parent">Parent transform for the target.</param>
        /// <returns>The instantiated TargetView.</returns>
        public TargetView Create(TargetConfig config, Transform parent)
        {
            TargetView prefab = config.PrefabOverride != null 
                ? config.PrefabOverride.GetComponent<TargetView>() 
                : _defaultPrefab;

            if (prefab == null)
            {
                if (_defaultPrefab == null)
                    throw new InvalidOperationException("No target prefab available for creation.");
                prefab = _defaultPrefab;
            }

            Vector3 position = CalculateOrbitPosition(config.OrbitRadius, config.StartingAngle);
            
            TargetView instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, parent);
            
            instance.transform.localScale = Vector3.one * config.BaseScale;

            var rotationBehavior = _rotationBehaviorFactory.Create(config);
            instance.SetRotationBehavior(rotationBehavior);

            instance.Initialize(config);
            
            _objectResolver.Inject(instance);

            return instance;
        }

        private Vector3 CalculateOrbitPosition(float radius, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Cos(angleRadians) * radius,
                Mathf.Sin(angleRadians) * radius,
                0f
            );
        }
    }
}

