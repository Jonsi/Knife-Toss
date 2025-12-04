using UnityEngine;
using KnifeToss.Infrastructure.Services;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Checks if the knife is visible in the gameplay camera viewport.
    /// Used to determine when to return knives to the pool.
    /// </summary>
    public sealed class KnifeVisibility : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;

        private RuntimeCacheService _cacheService;
        private const float Margin = 0.05f;

        public void Initialize(RuntimeCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public bool IsOutOfBounds()
        {
            Camera cam = gameplayCamera != null ? gameplayCamera : _cacheService?.MainCamera;
            if (cam == null)
            {
                return false;
            }

            Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);

            if (viewportPosition.z < 0f)
            {
                return true;
            }

            return viewportPosition.x < -Margin
                   || viewportPosition.x > 1f + Margin
                   || viewportPosition.y < -Margin
                   || viewportPosition.y > 1f + Margin;
        }
    }
}

