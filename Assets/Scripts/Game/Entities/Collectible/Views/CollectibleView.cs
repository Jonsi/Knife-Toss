using System;
using Common.Collision.Interfaces;
using Game.Entities.Collectible.Data;
using Game.Feedback.Animation;
using UnityEngine;

namespace Game.Entities.Collectible.Views
{
    /// <summary>
    /// Visual representation of a collectible item.
    /// Handles hit logic and visual feedback.
    /// </summary>
    public class CollectibleView : MonoBehaviour, IHitable
    {
        private static readonly Type CachedType = typeof(CollectibleView);

        [SerializeField] private Collider2D _Collider;
        [SerializeField] private SpriteShatterController _ShatterController;
        
        /// <summary>
        /// Particle effect to spawn when collected.
        /// </summary>
        [Tooltip("Prefab for particles to spawn on hit.")]
        [SerializeField] private GameObject _HitParticlesPrefab;

        private bool _IsHit;
        public int Value { get; private set; }

        public Type HitableType => CachedType;

        public void Initialize(CollectibleConfig config)
        {
            Value = config.Value;
            _IsHit = false;
            
            if (_Collider != null)
                _Collider.enabled = true;
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
    }
}
