using UnityEngine;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Data structure containing collision information.
    /// </summary>
    public readonly struct CollisionData
    {
        public readonly Vector3 ImpactPoint;
        public readonly GameObject Other;
        public readonly Transform OtherTransform;
        public readonly CollisionType Type;

        public CollisionData(Vector3 impactPoint, GameObject other, Transform otherTransform, CollisionType type)
        {
            ImpactPoint = impactPoint;
            Other = other;
            OtherTransform = otherTransform;
            Type = type;
        }

        public static CollisionData FromTrigger(Collider2D collider, Transform knifeTransform)
        {
            Vector3 impactPoint = collider?.ClosestPoint(knifeTransform.position) ?? knifeTransform.position;
            return new CollisionData(impactPoint, collider?.gameObject, collider?.transform, CollisionType.Trigger);
        }

        public static CollisionData FromCollision(Collision2D collision, Transform knifeTransform)
        {
            Vector3 impactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (collision.collider?.ClosestPoint(knifeTransform.position) ?? knifeTransform.position);
            return new CollisionData(impactPoint, collision.gameObject, collision.transform, CollisionType.Collision);
        }
    }

    public enum CollisionType
    {
        Trigger,
        Collision
    }
}

