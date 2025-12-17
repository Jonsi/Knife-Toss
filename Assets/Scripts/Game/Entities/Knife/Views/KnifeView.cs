using System;
using Common.Collision.Data;
using Common.Collision.Interfaces;
using Common.Events;
using Common.Interfaces;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Interfaces;
using Game.Entities.Knife.States;
using Game.Entities.Target.Data;
using Game.Feedback.Animation;
using Game.Pooling;
using UnityEngine;
using VContainer;

namespace Game.Entities.Knife.Views
{
    /// <summary>
    /// MonoBehaviour wrapper for the knife visual representation.
    /// Uses a state machine to manage knife behavior.
    /// Implements IHitable to participate in the collision system.
    /// </summary>
    public class KnifeView : MonoBehaviour, IKnifeViewContext, IStickable
    {
        private static readonly Type CachedType = typeof(KnifeView);

        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private Transform _VisualRoot;
        [SerializeField] private Collider2D _Collider;
        [SerializeField] private SpriteShatterController _ShatterController;
        
        private KnifeStateMachine _StateMachine;
        private Camera _MainCamera;
        private bool _HasReportedOutOfView;

        private IEventChannel<CollisionData> _CollisionChannel;
        private IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private KnifePoolProvider _KnifePoolProvider;

        /// <summary>
        /// Gets the cached type for efficient collision routing.
        /// </summary>
        public Type HitableType => CachedType;

        /// <summary>
        /// Gets whether the knife is currently in flight.
        /// </summary>
        public bool IsInFlight => _StateMachine?.IsInFlight ?? false;
        
        /// <summary>
        /// Exposes transform for state consumption.
        /// </summary>
        public Transform Transform => transform;

        public Transform VisualRoot => _VisualRoot != null ? _VisualRoot : transform;
        public SpriteRenderer SpriteRenderer => _SpriteRenderer;

        public float RadiusOffset => 0f;
        public float RotationOffset => 0f;
        
        /// <inheritdoc />
        public void ApplySorting(int sortingLayerId, int sortingOrder)
        {
            if (_SpriteRenderer == null)
                return;

            _SpriteRenderer.sortingLayerID = sortingLayerId;
            _SpriteRenderer.sortingOrder = sortingOrder;
        }

        public void OnAttachedToTarget()
        {
            // Optional: Logic when attached to target
        }

        [Inject]
        private void Construct(
            IEventChannel<CollisionData> collisionChannel,
            IEventChannel<KnifeImpactEventData> impactChannel)
        {
            _CollisionChannel = collisionChannel;
            _ImpactChannel = impactChannel;
        }

        private void Awake()
        {
            InitializeStateMachine();
            _MainCamera = Camera.main;
        }
        
        private void Update()
        {
            _StateMachine?.Update(Time.deltaTime);
            CheckOutOfView();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collisionChannel = _CollisionChannel;
            if (collisionChannel == null)
                return;
            
            var hitable = other.GetComponent<IHitable>();
            if (hitable == null)
                return;

            // Calculate impact point as the closest point on the other collider
            Vector2 impactPoint = other.ClosestPoint(transform.position);
            collisionChannel.Raise(new CollisionData(this, hitable, impactPoint));
        }

        /// <summary>
        /// Launches the knife in the specified direction with the given speed.
        /// </summary>
        /// <param name="direction">The direction vector in which to launch the knife.</param>
        /// <param name="speed">The speed at which to launch the knife.</param>
        public void Launch(Vector2 direction, float speed)
        {
            _HasReportedOutOfView = false;
            _StateMachine.InFlight.Launch(direction, speed);
            _StateMachine.SetState(_StateMachine.InFlight);
        }

        /// <summary>
        /// Stops the knife's movement and transitions to the stuck state.
        /// The target will handle positioning the knife.
        /// </summary>
        public void StopMovement()
        {
            _StateMachine.SetState(_StateMachine.Stuck);
        }

        /// <summary>
        /// Transitions the knife to the bounced state.
        /// </summary>
        public void Bounce()
        {
            _StateMachine.Bounced.ConfigureBounce(new Vector2(0f, 6f), 360f);
            _StateMachine.SetState(_StateMachine.Bounced);
        }


        /// <summary>
        /// Resets the knife to its initial loaded state.
        /// </summary>
        public void ResetKnife()
        {
            _HasReportedOutOfView = false;
            _StateMachine.SetState(_StateMachine.Loaded);
        }


        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        private void InitializeStateMachine()
        {
            _StateMachine = new KnifeStateMachine(
                new KnifeLoadedState(this),
                new KnifeInFlightState(this),
                new KnifeStuckState(this),
                new KnifeBouncedState(this),
                new KnifePooledState(this));
        }
        
        public void SetColliderEnabled(bool enabled)
        {
            if (_Collider != null)
                _Collider.enabled = enabled;
        }

        private void CheckOutOfView()
        {
            if (_HasReportedOutOfView)
                return;

            var mainCamera = _MainCamera;
            if (mainCamera == null)
                return;

            if (_StateMachine == null || !_StateMachine.ShouldMonitorOutOfView)
                return;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool isOutOfView = viewportPoint.x < 0f || viewportPoint.x > 1f || viewportPoint.y < 0f || viewportPoint.y > 1f;

            if (!isOutOfView)
                return;

            _HasReportedOutOfView = true;
            _ImpactChannel?.Raise(new KnifeImpactEventData(KnifeImpactType.OutOfView, transform.position));
            ReturnToPool();
        }

        /// <summary>
        /// Plays the configured shatter effect for this knife if available.
        /// </summary>
        public void PlayShatterEffect()
        {
            _ShatterController?.PlayShatter();
        }

        private void ReturnToPool()
        {
            if (_KnifePoolProvider == null)
                return;

            _StateMachine?.SetState(_StateMachine.Pooled);
            _KnifePoolProvider.Release(this);
        }

        internal void AssignPoolProvider(KnifePoolProvider poolProvider)
        {
            _KnifePoolProvider = poolProvider;
        }
    }
}
