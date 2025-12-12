using System;
using Common.Collision.Interfaces;
using Common.Events;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Data;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Feedback;
using UnityEngine;

namespace Game.Entities.Target.Views
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles rotation via injected rotation behavior.
    /// </summary>
    public class TargetView : MonoBehaviour, ITargetView, IHitable
    {
        private static readonly Type CachedType = typeof(TargetView);
        private const float KNIFE_ROTATION_FACING_CENTER = 90f;

        [SerializeField] private CircleCollider2D _Collider;
        [SerializeField] private TargetHitFeedback _HitFeedback;
        [SerializeField] private Transform _StuckObjectsHolder;


        private IRotationBehavior _RotationBehavior;
        private float _CurrentRotation;
        private Transform StickParent => _StuckObjectsHolder != null ? _StuckObjectsHolder : transform;
        private bool _HasNotifiedDestroyed;
        private System.Collections.Generic.List<KnifeView> _StuckKnives = new();

        public int KnivesToBreak { get; private set; }
        public int HitsReceived { get; private set; }
        
        /// <summary>
        /// Event raised when the target is destroyed.
        /// </summary>
        public event Action<ITargetView> OnDestroyed;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets the current rotation angle of the target in degrees.
        /// </summary>
        public float Rotation => _CurrentRotation;

        /// <summary>
        /// Gets the radius of the target.
        /// </summary>
        public float Radius
        {
            get
            {
                if (_Collider != null)
                {
                    return _Collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                }
                return 1f;
            }
        }

        /// <summary>
        /// Gets the world position of the target center.
        /// </summary>
        public Vector2 Center => transform.position;

        /// <summary>
        /// Gets the transform for parenting stuck knives.
        /// </summary>
        public Transform Transform => StickParent;

        /// <summary>
        /// Plays hit feedback on the target.
        /// </summary>
        public void PlayHitFeedback()
        {
            _HitFeedback?.PlayHitFeedback();
        }

        public void Initialize(TargetConfig config)
        {
            KnivesToBreak = config.KnivesToBreak;
            _CurrentRotation = config.StartingAngle;
            HitsReceived = 0;

            // Set initial rotation
            transform.rotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
        }

        public void SetRotationBehavior(IRotationBehavior rotationBehavior)
        {
            _RotationBehavior = rotationBehavior;
        }

        private void Update()
        {
            if (_RotationBehavior == null)
                return;

            float rotationDelta = _RotationBehavior.GetRotationDelta(Time.deltaTime);
            _CurrentRotation += rotationDelta;
            _CurrentRotation %= 360f;
            transform.rotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
        }

        /// <summary>
        /// Sticks a knife to this target at the specified position and rotation.
        /// </summary>
        /// <param name="knife">The knife view to stick to this target.</param>
        /// <param name="impactPoint">The world position where the impact occurred.</param>
        public void Stick(KnifeView knife, Vector2 impactPoint)
        {
            if (knife == null)
                return;

            AttachKnife(knife);
            ApplyStickTransform(knife, impactPoint);
            _StuckKnives.Add(knife);

            RegisterHit();
        }

        private void RegisterHit()
        {
            HitsReceived++;
            if (HitsReceived >= KnivesToBreak)
            {
                DestroyTarget();
            }
        }

        /// <summary>
        /// Places a pre-stuck knife at the specified angle without affecting hit counts.
        /// </summary>
        /// <param name="knife">Knife view to position.</param>
        /// <param name="angleDegrees">Angle in degrees around the target.</param>
        public void PlacePreStuckKnife(KnifeView knife, float angleDegrees)
        {
            if (knife == null)
                return;

            AttachKnife(knife);
            StickPositionData stickData = knife.StickPositionData;
            Vector2 finalPosition = CalculateStickPosition(stickData.Offset, angleDegrees);
            float finalRotation = CalculateStickRotation(angleDegrees, stickData.RotationOffset);

            knife.transform.localPosition = finalPosition;
            knife.transform.localRotation = Quaternion.Euler(0f, 0f, finalRotation);
        }

        private void DestroyTarget()
        {
            // Knives will be cleaned up when level completes
            _StuckKnives.Clear();

            NotifyDestroyed();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            NotifyDestroyed();
        }

        private void NotifyDestroyed()
        {
            if (_HasNotifiedDestroyed)
                return;

            _HasNotifiedDestroyed = true;
            OnDestroyed?.Invoke(this);
        }

        private void AttachKnife(KnifeView knife) => knife.transform.SetParent(StickParent);

        private void ApplyStickTransform(KnifeView knife, Vector2 impactPoint)
        {
            Vector2 localPoint = TransformImpactToLocal(impactPoint);
            float angleDegrees = CalculateImpactAngle(localPoint);
            StickPositionData stickData = knife.StickPositionData;
            Vector2 finalPosition = CalculateStickPosition(stickData.Offset, angleDegrees);
            float finalRotation = CalculateStickRotation(angleDegrees, stickData.RotationOffset);

            knife.transform.localPosition = finalPosition;
            knife.transform.localRotation = Quaternion.Euler(0f, 0f, finalRotation);
        }

        private Vector2 TransformImpactToLocal(Vector2 impactPoint) => transform.InverseTransformPoint(impactPoint);

        private static float CalculateImpactAngle(Vector2 localPoint) => Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

        private Vector2 CalculateStickPosition(float offset, float angleDegrees)
        {
            float finalRadius = CalculateStickRadius(offset);
            float angleRadians = angleDegrees * Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(angleRadians) * finalRadius,
                Mathf.Sin(angleRadians) * finalRadius
            );
        }

        private float CalculateStickRadius(float offset)
        {
            if (_Collider == null)
                return 1f + offset;

            float scaledRadius = _Collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            return scaledRadius + offset;
        }

        private static float CalculateStickRotation(float angleDegrees, float rotationOffset)
        {
            return angleDegrees + KNIFE_ROTATION_FACING_CENTER + rotationOffset;
        }
    }
}
