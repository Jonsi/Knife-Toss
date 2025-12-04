using UnityEngine;
using KnifeToss.Core.Constants;
using KnifeToss.Core.Utilities;
using KnifeToss.Core.Events;

namespace KnifeToss.Presentation.Gameplay.Projectiles
{
    /// <summary>
    /// Handles the logic for sticking a knife to targets and surfaces.
    /// </summary>
    public sealed class KnifeSticking : MonoBehaviour
    {
        [Header("Stick Settings")]
        [SerializeField] private float embedOffset = 0.02f;
        [SerializeField] private Transform snapPosition;

        [Header("Events")]
        [SerializeField] private KnifeImpactEventChannel impactEventChannel;

        public float EmbedOffset => embedOffset;
        public bool IsStuck { get; private set; }

        public void StickToCollider(Transform anchor, Collider2D collider, Vector3 impactPoint)
        {
            if (anchor == null)
            {
                return;
            }

            ResolveCircleSnap(anchor, collider, impactPoint, out Vector3 center, out Vector3 surfacePoint, out Vector2 surfaceDirection);
            AlignToSurface(anchor, center, surfacePoint, surfaceDirection);
            IsStuck = true;
            impactEventChannel?.RaiseKnifeStuck(impactPoint);
        }

        public void MarkAsPreStuck()
        {
            IsStuck = true;
        }

        public void ResetState()
        {
            IsStuck = false;
        }

        private void AlignToSurface(Transform anchorParent, Vector3 circleCenter, Vector3 surfacePoint, Vector2 surfaceDirection)
        {
            Vector2 direction = surfaceDirection.sqrMagnitude > Mathf.Epsilon
                ? surfaceDirection.normalized
                : CollisionUtility.ResolveImpactDirection(circleCenter, surfacePoint, transform.position);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -direction);

            Vector3 snapTarget = surfacePoint;
            if (TryResolveSnapOffset(rotation, out Vector3 snapOffset))
            {
                transform.SetPositionAndRotation(snapTarget - snapOffset, rotation);
            }
            else
            {
                transform.SetPositionAndRotation(snapTarget, rotation);
            }

            transform.SetParent(anchorParent, true);
        }

        private bool TryResolveSnapOffset(Quaternion rotation, out Vector3 snapOffset)
        {
            Transform resolvedSnap = ResolveSnapPosition();
            if (resolvedSnap == null)
            {
                snapOffset = Vector3.zero;
                return false;
            }

            Vector3 scaledLocal = Vector3.Scale(resolvedSnap.localPosition, transform.lossyScale);
            snapOffset = rotation * scaledLocal;
            return true;
        }

        private Transform ResolveSnapPosition()
        {
            if (snapPosition != null)
            {
                return snapPosition;
            }

            snapPosition = transform.Find("SnapPosition");
            return snapPosition;
        }

        private void ResolveCircleSnap(Transform anchor, Collider2D collider, Vector3 impactPoint, out Vector3 center, out Vector3 surfacePoint, out Vector2 surfaceDirection)
        {
            CircleCollider2D circle = collider as CircleCollider2D;
            if (circle == null && anchor != null)
            {
                circle = anchor.GetComponent<CircleCollider2D>();
            }

            center = anchor != null ? anchor.position : impactPoint;
            surfaceDirection = CollisionUtility.ResolveImpactDirection(center, impactPoint, transform.position);
            surfacePoint = center;

            if (circle == null)
            {
                return;
            }

            center = circle.transform.TransformPoint(circle.offset);
            float uniformScale = Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
            float radius = circle.radius * uniformScale;

            surfaceDirection = CollisionUtility.ResolveImpactDirection(center, impactPoint, transform.position);
            surfaceDirection = surfaceDirection.sqrMagnitude > Mathf.Epsilon ? surfaceDirection.normalized : Vector2.up;
            surfacePoint = center + (Vector3)(surfaceDirection * radius);
        }
    }
}

