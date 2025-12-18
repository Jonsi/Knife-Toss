using System;
using Common.Collision.Interfaces;
using Common.Interfaces;
using Game.Entities.Collectible.Data;
using Game.Feedback.Animation;
using Game.PowerUps.Services;
using UnityEngine;
using VContainer;

namespace Game.Entities.Collectible.Views
{
    /// <summary>
    /// Visual representation of a collectible item.
    /// Handles hit logic and visual feedback.
    /// </summary>
    public class CollectibleView : MonoBehaviour, IHitable, IStickable
    {
        private static readonly Type CachedType = typeof(CollectibleView);

        [SerializeField] private Collider2D _Collider;
        [SerializeField] private SpriteShatterController _ShatterController;
        [SerializeField] private SpriteRenderer _SpriteRenderer;
        
        /// <summary>
        /// Particle effect to spawn when collected.
        /// </summary>
        [Tooltip("Prefab for particles to spawn on hit.")]
        [SerializeField] private GameObject _HitParticlesPrefab;

        private bool _IsHit;
        private CollectibleRegistry _CollectibleRegistry;
        public int Value { get; private set; }

        public Transform Transform => transform;
        public float RadiusOffset { get; private set; }
        public float RotationOffset { get; private set; }

        public Type HitableType => CachedType;

        [Inject]
        private void Construct(CollectibleRegistry collectibleRegistry)
        {
            _CollectibleRegistry = collectibleRegistry;
        }
        
        /// <inheritdoc />
        public void ApplySorting(int sortingLayerId, int sortingOrder)
        {
            if (_SpriteRenderer == null)
                return;

            _SpriteRenderer.sortingLayerID = sortingLayerId;
            _SpriteRenderer.sortingOrder = sortingOrder;
        }

        public void Initialize(CollectibleConfig config)
        {
            Value = config.Value;
            _IsHit = false;
            
            RadiusOffset = config.RadiusOffset;
            RotationOffset = -180f; // Compensate for default +90 rotation in StickPositionService to get -90

            if (_Collider != null)
                _Collider.enabled = true;
        }

        public void OnAttachedToTarget()
        {
            // Optional: Logic when attached
        }

        /// <summary>
        /// Marks the collectible as hit to prevent double processing.
        /// </summary>
        /// <returns>True if successfully marked as hit (was not hit before), false otherwise.</returns>
        public bool MarkAsHit()
        {
            if (_IsHit)
                return false;

            _IsHit = true;
            if (_Collider != null)
                _Collider.enabled = false;
            
            return true;
        }

        /// <summary>
        /// Plays the configured hit visual effect.
        /// </summary>
        public void PlayHitFeedback()
        {
            if (_ShatterController != null)
            {
                _ShatterController.PlayShatter();
                return;
            }

            if (_HitParticlesPrefab != null)
            {
                Instantiate(_HitParticlesPrefab, transform.position, Quaternion.identity);
            }
        }

        private void OnDestroy()
        {
            _CollectibleRegistry?.Unregister(this);
        }
    }
}
