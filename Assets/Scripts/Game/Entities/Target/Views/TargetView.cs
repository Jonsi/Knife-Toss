using System;
using System.Collections.Generic;
using Common.Collision.Interfaces;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Data;
using Game.Entities.Target.Data.Behaviors;
using Game.Entities.Target.Feedback;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using UnityEngine;

namespace Game.Entities.Target.Views
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles movement behaviors configured per target.
    /// </summary>
    public class TargetView : MonoBehaviour, ITargetView, IHitable
    {
        private static readonly Type CachedType = typeof(TargetView);
        private const float KNIFE_ROTATION_FACING_CENTER = 90f;

        [SerializeField] private CircleCollider2D _Collider;
        [SerializeField] private TargetHitFeedback _HitFeedback;
        [SerializeField] private TargetShatterController _ShatterController;
        [SerializeField] private Transform _StuckObjectsHolder;
        [SerializeField] private TargetSpawnAnimator _SpawnAnimator;

        private readonly LocalBehaviorRuntime _LocalBehaviorRuntime = new();
        private readonly WorldBehaviorRuntime _WorldBehaviorRuntime = new();
        private readonly List<ILocalTargetBehavior> _LocalBehaviors = new();
        private readonly List<IWorldTargetBehavior> _WorldBehaviors = new();

        private float _CurrentRotation;
        private Transform _WorldCenter;
        private Transform StickParent => _StuckObjectsHolder != null ? _StuckObjectsHolder : transform;
        private bool _HasNotifiedDestroyed;
        private List<KnifeView> _StuckKnives = new();

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

        /// <summary>
        /// Configures the behaviors that drive this target.
        /// </summary>
        /// <param name="behaviorCollection">Runtime behavior set.</param>
        /// <param name="worldCenter">Optional world center reference.</param>
        public void ConfigureBehaviors(TargetBehaviorCollection behaviorCollection, Transform worldCenter)
        {
            _LocalBehaviors.Clear();
            _WorldBehaviors.Clear();

            if (behaviorCollection != null)
            {
                if (behaviorCollection.LocalBehaviors != null)
                    _LocalBehaviors.AddRange(behaviorCollection.LocalBehaviors);
                if (behaviorCollection.WorldBehaviors != null)
                    _WorldBehaviors.AddRange(behaviorCollection.WorldBehaviors);
            }

            _WorldCenter = worldCenter;
            TickBehaviors(0f);
        }

        public void Initialize(TargetConfig config)
        {
            KnivesToBreak = config.KnivesToBreak;
            _CurrentRotation = config.InitialRotation;
            HitsReceived = 0;

            transform.localRotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
        }

        /// <summary>
        /// Triggers the optional spawn animation once the target is fully configured.
        /// </summary>
        public void PlaySpawnAnimation()
        {
            _SpawnAnimator?.PlaySpawnAnimation();
        }

        private void Update() => TickBehaviors(Time.deltaTime);

        private void TickBehaviors(float deltaTime)
        {
            UpdateWorldBehaviors(deltaTime);
            UpdateLocalBehaviors(deltaTime);
        }

        private void UpdateWorldBehaviors(float deltaTime)
        {
            if (_WorldBehaviors.Count == 0)
                return;

            Vector3 center = _WorldCenter != null ? _WorldCenter.position : Vector3.zero;
            _WorldBehaviorRuntime.Begin(deltaTime, center, transform.position);

            for (int i = 0; i < _WorldBehaviors.Count; i++)
            {
                _WorldBehaviors[i].Tick(_WorldBehaviorRuntime);
            }

            transform.position = _WorldBehaviorRuntime.Position;
        }

        private void UpdateLocalBehaviors(float deltaTime)
        {
            if (_LocalBehaviors.Count == 0)
                return;

            _LocalBehaviorRuntime.Begin(deltaTime);
            for (int i = 0; i < _LocalBehaviors.Count; i++)
            {
                _LocalBehaviors[i].Tick(_LocalBehaviorRuntime);
            }

            if (!_LocalBehaviorRuntime.HasRotationDelta)
                return;

            _CurrentRotation += _LocalBehaviorRuntime.RotationDelta;
            _CurrentRotation %= 360f;
            transform.localRotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
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
            if (_HasNotifiedDestroyed)
                return;

            _ShatterController?.PlayShatter();
            _StuckKnives.Clear();
            FinalizeDestruction();
        }

        private void FinalizeDestruction()
        {
            NotifyDestroyed();
            Destroy(gameObject);
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
