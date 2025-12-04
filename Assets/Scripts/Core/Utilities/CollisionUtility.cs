using UnityEngine;

namespace KnifeToss.Core.Utilities
{
    /// <summary>
    /// Utility methods for resolving collision impact points and directions.
    /// </summary>
    public static class CollisionUtility
    {
        /// <summary>
        /// Resolves the impact point from a Collision2D.
        /// </summary>
        public static Vector3 ResolveImpactPoint(Collision2D collision, Vector3 fallback)
        {
            if (collision == null) return fallback;
            if (collision.contactCount > 0)
                return collision.GetContact(0).point;
            return ResolveImpactPoint(collision.collider, fallback);
        }

        /// <summary>
        /// Resolves the impact point from a Collider2D.
        /// </summary>
        public static Vector3 ResolveImpactPoint(Collider2D collider, Vector3 position)
        {
            return collider?.ClosestPoint(position) ?? position;
        }

        /// <summary>
        /// Resolves the impact direction from center to impact point.
        /// </summary>
        public static Vector2 ResolveImpactDirection(Vector3 center, Vector3 impactPoint, Vector3 fallbackPosition)
        {
            Vector2 fromCenter = (Vector2)(impactPoint - center);
            if (fromCenter.sqrMagnitude < Mathf.Epsilon)
            {
                fromCenter = (Vector2)(fallbackPosition - center);
            }

            return fromCenter.sqrMagnitude > Mathf.Epsilon ? fromCenter.normalized : Vector2.up;
        }
    }
}

