using System;
using System.Collections.Generic;
using Common.Collision.Interfaces;
using Common.Interfaces;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Data;
using Game.Entities.Target.Data.Behaviors;
using Game.Entities.Target.Feedback;
using Game.Entities.Target.Interfaces;
using Game.Entities.Target.Logic;
using Game.Entities.Target.Services;
using Game.Feedback.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Entities.Target.Views
{
    /// <summary>
    /// MonoBehaviour wrapper for the target visual representation.
    /// Handles movement behaviors configured per target.
    /// </summary>
    public class TargetView : MonoBehaviour, ITargetView, IHitable
    {
        private static readonly Type CachedType = typeof(TargetView);

        [SerializeField] private CircleCollider2D _Collider;
        [SerializeField] private TargetHitFeedback _HitFeedback;
        [SerializeField] private TargetCrackReveal _CrackReveal;
        [SerializeField] private SpriteShatterController _ShatterController;
        [SerializeField] private Transform _StuckObjectsHolder;
        [SerializeField] private TargetSpawnAnimator _SpawnAnimator;
        [SerializeField] private SpriteRenderer[] _SpriteRenderers = Array.Empty<SpriteRenderer>();

        private readonly LocalBehaviorRuntime _LocalBehaviorRuntime = new();
        private readonly WorldBehaviorRuntime _WorldBehaviorRuntime = new();
        private readonly List<ILocalTargetBehavior> _LocalBehaviors = new();
        private readonly List<IWorldTargetBehavior> _WorldBehaviors = new();

        private float _CurrentRotation;
        private float _SpeedMultiplier = 1f;
        private float _LastRotationDelta;
        private Vector3 _InitialWorldPosition;
        private bool _HasInitialWorldPosition;
        private Transform StickParent => _StuckObjectsHolder != null ? _StuckObjectsHolder : transform;
        private bool _HasNotifiedDestroyed;
        private List<IStickable> _StuckObjects = new();
        private const int SortingOrderStride = 10;
        [Tooltip("Sorting order offset for stuck objects relative to the target. Set to 1 to render on top.")]
        [SerializeField] private int _StickableSortingOffset = -1;
        private int _BaseSortingOrder;
        private readonly StickPositionService _StickPositionService = new();
        private int _SortingLayerId;

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
                    return _Collider.radius * GetEffectiveScale();
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
        /// Sets the initial world position that world behaviors should use as their anchor.
        /// </summary>
        /// <param name="worldPosition">Initial world position.</param>
        public void SetInitialWorldPosition(Vector3 worldPosition)
        {
            _InitialWorldPosition = worldPosition;
            _HasInitialWorldPosition = true;
        }

        /// <summary>
        /// Configures the behaviors that drive this target.
        /// </summary>
        /// <param name="behaviorCollection">Runtime behavior set.</param>
        public void ConfigureBehaviors(TargetBehaviorCollection behaviorCollection)
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

            if (!_HasInitialWorldPosition)
            {
                SetInitialWorldPosition(transform.position);
            }

            TickBehaviors(0f);
        }

        /// <summary>
        /// Applies the render sorting order derived from the level configuration index.
        /// </summary>
        /// <param name="orderIndex">Index within the level target list.</param>
        public void SetSortingOrder(int orderIndex)
        {
            ResolveCrackReveal();
            _BaseSortingOrder = Mathf.Max(0, orderIndex) * SortingOrderStride;
            _SortingLayerId = ResolveSortingLayerId();
            ApplySortingOrderToRenderers();
            _CrackReveal?.SetSortingOrder(_SortingLayerId, _BaseSortingOrder);
            UpdateStuckObjectsSorting();
        }

        public void SetSprite(Sprite sprite)
        {
            if (sprite == null) return;
            
            foreach (var renderer in _SpriteRenderers)
            {
                if (renderer != null)
                {
                    renderer.sprite = sprite;
                }
            }
        }

        public void Initialize(TargetConfig config)
        {
            ResolveCrackReveal();
            KnivesToBreak = config.KnivesToBreak;
            _CurrentRotation = config.InitialRotation;
            HitsReceived = 0;
            _CrackReveal?.ResetReveal();

            transform.localRotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
        }

        /// <summary>
        /// Triggers the optional spawn animation once the target is fully configured.
        /// </summary>
        public void PlaySpawnAnimation()
        {
            _SpawnAnimator?.PlaySpawnAnimation();
        }

        private void Awake()
        {
            ResolveCrackReveal();
        }

        private void OnEnable()
        {
            if (_SpawnAnimator != null)
            {
                _SpawnAnimator.OnAnimationComplete += EnablePhysicsAfterSpawn;
            }
        }

        private void OnDisable()
        {
            if (_SpawnAnimator != null)
            {
                _SpawnAnimator.OnAnimationComplete -= EnablePhysicsAfterSpawn;
            }
        }

        private void Update()
        {
            TickBehaviors(Time.deltaTime);
        }

        private void TickBehaviors(float deltaTime)
        {
            UpdateWorldBehaviors(deltaTime);
            UpdateLocalBehaviors(deltaTime);
        }

        private void UpdateWorldBehaviors(float deltaTime)
        {
            if (_WorldBehaviors.Count == 0)
                return;

            if (!_HasInitialWorldPosition)
            {
                SetInitialWorldPosition(transform.position);
            }

            _WorldBehaviorRuntime.Begin(deltaTime, _InitialWorldPosition, transform.position, true, _SpeedMultiplier);

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

            _LocalBehaviorRuntime.Begin(deltaTime, _SpeedMultiplier);
            for (int i = 0; i < _LocalBehaviors.Count; i++)
            {
                _LocalBehaviors[i].Tick(_LocalBehaviorRuntime);
            }

            if (!_LocalBehaviorRuntime.HasRotationDelta)
                return;

            _LastRotationDelta = _LocalBehaviorRuntime.RotationDelta;
            _CurrentRotation += _LastRotationDelta;
            _CurrentRotation %= 360f;
            transform.localRotation = Quaternion.Euler(0f, 0f, _CurrentRotation);
        }

        /// <summary>
        /// Applies a speed multiplier to target behaviors.
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _SpeedMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// Clears all behavior speed modifiers.
        /// </summary>
        public void ClearRotationModifiers()
        {
            _SpeedMultiplier = 1f;
        }

        /// <summary>
        /// Sticks an object to this target at the specified position and rotation.
        /// </summary>
        /// <param name="stickable">The object to stick to this target.</param>
        /// <param name="impactPoint">The world position where the impact occurred.</param>
        public void Stick(IStickable stickable, Vector2 impactPoint)
        {
            if (stickable == null)
                return;

            AttachStickable(stickable);
            ApplyStickTransform(stickable, impactPoint);
            _StuckObjects.Add(stickable);

            RegisterHit();
        }

        private void RegisterHit()
        {
            ResolveCrackReveal();
            HitsReceived++;
            _CrackReveal?.SetHitProgress(HitsReceived, KnivesToBreak);

            if (HitsReceived >= KnivesToBreak)
            {
                DestroyTarget();
            }
        }

        /// <summary>
        /// Places a stickable object at the specified angle without affecting hit counts.
        /// </summary>
        /// <param name="stickable">Object to position.</param>
        /// <param name="angleDegrees">Angle in degrees around the target.</param>
        public void PlaceStickable(IStickable stickable, float angleDegrees)
        {
            if (stickable == null)
                return;

            AttachStickable(stickable);
            
            ApplyPlacementTransform(stickable, angleDegrees);
            CheckForLayoutGroupInterference();
            DisablePhysicsForSpawn(stickable);

            _StuckObjects.Add(stickable);
        }

        private void ApplyPlacementTransform(IStickable stickable, float angleDegrees)
        {
            float effectiveScale = GetEffectiveScale();
            float colliderRadius = _Collider != null ? _Collider.radius : 1f;
            
            Vector2 finalPosition = _StickPositionService.CalculatePosition(
                colliderRadius,
                effectiveScale,
                angleDegrees,
                stickable.RadiusOffset
            );

            Quaternion finalRotation = _StickPositionService.CalculateRotation(
                angleDegrees,
                stickable.RotationOffset
            );

            stickable.Transform.localPosition = finalPosition;
            stickable.Transform.localRotation = finalRotation;
        }

        private void CheckForLayoutGroupInterference()
        {
            var layoutGroup = StickParent.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                Debug.LogWarning($"[TargetView] StickParent '{StickParent.name}' has a LayoutGroup! This may override knife positions.");
            }
        }

        private void DisablePhysicsForSpawn(IStickable stickable)
        {
            var rb = stickable.Transform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = stickable.Transform.position; // Force sync
            }
        }

        private void EnablePhysicsAfterSpawn()
        {
            for (int i = 0; i < _StuckObjects.Count; i++)
            {
                var stickable = _StuckObjects[i];
                if (stickable == null || stickable.Transform == null) continue;

                var rb = stickable.Transform.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.simulated = true;
                }
            }
        }

        /// <summary>
        /// Forces destruction of the target regardless of current hit counts.
        /// </summary>
        public void ForceDestroy()
        {
            DestroyTarget();
        }

        private void DestroyTarget()
        {
            if (_HasNotifiedDestroyed)
                return;

            PlayStuckKnifeShatters();
            _ShatterController?.PlayShatter();
            _StuckObjects.Clear();
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

        private void AttachStickable(IStickable stickable)
        {
            stickable.Transform.SetParent(StickParent);
            stickable.ApplySorting(_SortingLayerId, _BaseSortingOrder + _StickableSortingOffset);
        }

        private void PlayStuckKnifeShatters()
        {
            for (int i = 0; i < _StuckObjects.Count; i++)
            {
                if (_StuckObjects[i] is KnifeView knife)
                {
                    knife.PlayShatterEffect();
                }
            }
        }

        private void ApplyStickTransform(IStickable stickable, Vector2 impactPoint)
        {
            Vector2 localPoint = TransformImpactToLocal(impactPoint);
            float angleDegrees = CalculateImpactAngle(localPoint);
            
            Vector2 finalPosition = _StickPositionService.CalculatePosition(
                _Collider != null ? _Collider.radius : 1f,
                GetEffectiveScale(),
                angleDegrees,
                stickable.RadiusOffset
            );

            Quaternion finalRotation = _StickPositionService.CalculateRotation(
                angleDegrees,
                stickable.RotationOffset
            );

            stickable.Transform.localPosition = finalPosition;
            stickable.Transform.localRotation = finalRotation;
        }

        private float GetEffectiveScale()
        {
            float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            if (scale < 0.001f)
            {
                scale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            }
            if (scale < 0.001f)
            {
                scale = 1f;
            }
            return scale;
        }

        private Vector2 TransformImpactToLocal(Vector2 impactPoint) => transform.InverseTransformPoint(impactPoint);

        private static float CalculateImpactAngle(Vector2 localPoint) => Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

        private void ApplySortingOrderToRenderers()
        {
            if (_SpriteRenderers == null)
                return;

            for (int i = 0; i < _SpriteRenderers.Length; i++)
            {
                if (_SpriteRenderers[i] != null)
                    _SpriteRenderers[i].sortingOrder = _BaseSortingOrder;
            }
        }

        private void UpdateStuckObjectsSorting()
        {
            for (int i = 0; i < _StuckObjects.Count; i++)
            {
                _StuckObjects[i]?.ApplySorting(_SortingLayerId, _BaseSortingOrder + _StickableSortingOffset);
            }
        }

        private int ResolveSortingLayerId()
        {
            if (_SpriteRenderers == null)
                return 0;

            for (int i = 0; i < _SpriteRenderers.Length; i++)
            {
                if (_SpriteRenderers[i] != null)
                    return _SpriteRenderers[i].sortingLayerID;
            }

            return 0;
        }

        private void ResolveCrackReveal()
        {
            if (_CrackReveal == null)
            {
                _CrackReveal = GetComponentInChildren<TargetCrackReveal>(true);
            }
        }
    }
}
