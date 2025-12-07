using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Provides target geometry calculations for spawning objects around targets.
    /// </summary>
    public static class TargetGeometry
    {
        /// <summary>
        /// Calculates the center position and radius of a target transform.
        /// </summary>
        public static (Vector3 center, float radius) GetCenterAndRadius(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return (Vector3.zero, 0.5f);
            }

            CircleCollider2D collider = targetTransform.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                Vector3 center = collider.transform.TransformPoint(collider.offset);
                float scale = Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y);
                float radius = collider.radius * scale;
                return (center, radius);
            }

            float defaultScale = Mathf.Max(targetTransform.lossyScale.x, targetTransform.lossyScale.y);
            return (targetTransform.position, 0.5f * defaultScale);
        }
    }
}

