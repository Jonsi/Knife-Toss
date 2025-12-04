using UnityEngine;
using KnifeToss.Infrastructure.Services;

namespace KnifeToss.Presentation.Gameplay.Throwing
{
    /// <summary>
    /// Calculates aim direction from screen position to world direction.
    /// Uses cached camera service instead of Camera.main.
    /// </summary>
    public sealed class AimCalculator : MonoBehaviour
    {
        [SerializeField] private Transform launcherTransform;

        private RuntimeCacheService _cacheService;

        public void Initialize(RuntimeCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public Vector2 CalculateDirection(Vector2 screenPosition)
        {
            if (launcherTransform == null)
            {
                launcherTransform = transform;
            }

            Camera cam = _cacheService?.MainCamera;
            if (cam == null)
            {
                return launcherTransform.up;
            }

            float zDistance = Mathf.Abs(launcherTransform.position.z - cam.transform.position.z);
            zDistance = Mathf.Approximately(zDistance, 0f) ? 1f : zDistance;
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistance));
            worldPoint.z = launcherTransform.position.z;

            Vector2 direction = (Vector2)(worldPoint - launcherTransform.position);
            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                return launcherTransform.up;
            }

            return direction.normalized;
        }

        public void UpdateAim(Vector2 screenPosition)
        {
            Vector2 direction = CalculateDirection(screenPosition);
            if (launcherTransform != null)
            {
                launcherTransform.up = direction;
            }
        }
    }
}

