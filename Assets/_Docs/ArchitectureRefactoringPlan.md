# Architecture Refactoring Plan

## Overview

A phased approach to refactor the Knife-Toss codebase with testable milestones.

### Goals
1. **MVP Everywhere** - All entities use Model-View-Presenter pattern
2. **Decouple entities** - No entity knows about other entities
3. **Remove GetComponent** - Use registry pattern instead
4. **Reduce code** - Remove redundant scripts
5. **Assembly separation** - Prevent wrong coupling at compile time

### Implementation Rules
- Add `<summary>` blocks to all public classes, interfaces, and methods
- Never create `.meta` files manually
- Never use `GetComponent<T>()` or `FindObject` methods
- Never use magic numbers - extract to constants or configs
- Never add `sealed` keyword to classes
- No inline comments or debug logs

---

# PHASE 1: MVP for Knife Entity
**Priority**: 🔴 HIGH - This is the core architectural change
**Goal**: Refactor KnifeView to proper MVP with passive view.

## The Problem

`KnifeView.cs` currently contains:
- State machine logic (Update, state transitions)
- Collision detection logic (OnTriggerEnter2D with GetComponent)
- Out-of-view detection logic
- Pool management logic

This violates MVP - the View should be passive.

## The Solution

Split into:
- **KnifeModel** - Data only (state, velocity, flags)
- **KnifeView** - Passive MonoBehaviour (exposes events, receives commands)
- **KnifePresenter** - All logic (state machine, collision handling, bounds checking)

## Step 1.1: Create EntityPresenter Base Class

The existing `BasePresenter` requires `IView<TModel>` with `Show()`/`Hide()` which is for UI.
Create a simpler base for game entities.

**Create**: `Assets/Scripts/Common/MVP/Core/EntityPresenter.cs`

```csharp
using System;
using R3;

namespace Common.MVP.Core
{
    /// <summary>
    /// Base presenter for game entities. Does not require IView constraint.
    /// </summary>
    public abstract class EntityPresenter<TView, TModel> : IDisposable
        where TView : class
        where TModel : IModel
    {
        protected readonly TView View;
        protected readonly TModel Model;
        protected readonly CompositeDisposable Disposables = new();

        protected EntityPresenter(TView view, TModel model)
        {
            View = view;
            Model = model;
        }

        /// <summary>
        /// Called to initialize subscriptions and start the presenter.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called every frame to update the presenter.
        /// </summary>
        public virtual void Tick(float deltaTime) { }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
```

## Step 1.2: Create KnifeModel

**Create**: `Assets/Scripts/Game/Entities/Knife/Models/KnifeModel.cs`

```csharp
using Common.MVP.Core;
using Game.PowerUps.Configs;
using UnityEngine;

namespace Game.Entities.Knife.Models
{
    /// <summary>
    /// Data model for knife state.
    /// </summary>
    public class KnifeModel : IModel
    {
        public bool IsInFlight { get; set; }
        public bool IsStuck { get; set; }
        public bool IsBouncing { get; set; }
        public bool IsPooled { get; set; }
        public bool IsSuperKnife { get; set; }
        public bool HasReportedOutOfView { get; set; }
        public Vector2 Velocity { get; set; }
        public float RotationSpeed { get; set; }
        public PowerUpConfigBase SourcePowerUpConfig { get; set; }

        /// <summary>
        /// Resets model to default pooled state.
        /// </summary>
        public void Reset()
        {
            IsInFlight = false;
            IsStuck = false;
            IsBouncing = false;
            IsPooled = true;
            IsSuperKnife = false;
            HasReportedOutOfView = false;
            Velocity = Vector2.zero;
            RotationSpeed = 0f;
            SourcePowerUpConfig = null;
        }
    }
}
```

## Step 1.3: Refactor KnifeView to Passive

**Modify**: `Assets/Scripts/Game/Entities/Knife/Views/KnifeView.cs`

Remove all logic. Keep only:
- SerializeField references
- Observable events for Unity callbacks
- Simple command methods

```csharp
using System;
using Common.Interfaces;
using Game.Feedback.Animation;
using R3;
using UnityEngine;

namespace Game.Entities.Knife.Views
{
    /// <summary>
    /// Passive view for knife. Exposes Unity events and receives commands.
    /// </summary>
    public class KnifeView : MonoBehaviour, IStickable
    {
        [SerializeField] private SpriteRenderer _SpriteRenderer;
        [SerializeField] private Transform _VisualRoot;
        [SerializeField] private Collider2D _Collider;
        [SerializeField] private SpriteShatterController _ShatterController;

        private readonly Subject<Collider2D> _OnTriggerEnter = new();
        private readonly Subject<Unit> _OnUpdate = new();

        /// <summary>
        /// Fires when OnTriggerEnter2D is called.
        /// </summary>
        public Observable<Collider2D> OnTriggerEntered => _OnTriggerEnter;

        /// <summary>
        /// Fires every Update frame.
        /// </summary>
        public Observable<Unit> OnUpdated => _OnUpdate;

        public Transform Transform => transform;
        public Transform VisualRoot => _VisualRoot != null ? _VisualRoot : transform;
        public SpriteRenderer SpriteRenderer => _SpriteRenderer;
        public Collider2D Collider => _Collider;
        public float RadiusOffset => 0f;
        public float RotationOffset => 0f;

        private void Update()
        {
            _OnUpdate.OnNext(Unit.Default);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _OnTriggerEnter.OnNext(other);
        }

        private void OnDestroy()
        {
            _OnTriggerEnter.Dispose();
            _OnUpdate.Dispose();
        }

        /// <summary>
        /// Sets the active state of the GameObject.
        /// </summary>
        public void SetActive(bool active) => gameObject.SetActive(active);

        /// <summary>
        /// Sets the collider enabled state.
        /// </summary>
        public void SetColliderEnabled(bool enabled)
        {
            if (_Collider != null)
                _Collider.enabled = enabled;
        }

        /// <summary>
        /// Moves the knife by velocity.
        /// </summary>
        public void Move(Vector2 velocity)
        {
            transform.Translate(velocity);
        }

        /// <summary>
        /// Rotates the knife.
        /// </summary>
        public void Rotate(float angle)
        {
            transform.Rotate(0f, 0f, angle);
        }

        /// <summary>
        /// Plays the shatter effect.
        /// </summary>
        public void PlayShatterEffect()
        {
            _ShatterController?.PlayShatter();
        }

        /// <summary>
        /// Applies sorting layer and order.
        /// </summary>
        public void ApplySorting(int sortingLayerId, int sortingOrder)
        {
            if (_SpriteRenderer == null) return;
            _SpriteRenderer.sortingLayerID = sortingLayerId;
            _SpriteRenderer.sortingOrder = sortingOrder;
        }

        public void OnAttachedToTarget() { }
    }
}
```

## Step 1.4: Create KnifePresenter

**Create**: `Assets/Scripts/Game/Entities/Knife/Presenters/KnifePresenter.cs`

```csharp
using Common.Collision.Data;
using Common.Collision.Interfaces;
using Common.Events;
using Common.MVP.Core;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Models;
using Game.Entities.Knife.Views;
using Game.Pooling;
using Game.Services;
using R3;
using UnityEngine;

namespace Game.Entities.Knife.Presenters
{
    /// <summary>
    /// Presenter containing all knife logic.
    /// </summary>
    public class KnifePresenter : EntityPresenter<KnifeView, KnifeModel>
    {
        private readonly ColliderRegistry _ColliderRegistry;
        private readonly IEventChannel<CollisionData> _CollisionChannel;
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly Camera _MainCamera;
        private KnifePoolProvider _PoolProvider;

        public KnifePresenter(
            KnifeView view,
            KnifeModel model,
            ColliderRegistry colliderRegistry,
            IEventChannel<CollisionData> collisionChannel,
            IEventChannel<KnifeImpactEventData> impactChannel)
            : base(view, model)
        {
            _ColliderRegistry = colliderRegistry;
            _CollisionChannel = collisionChannel;
            _ImpactChannel = impactChannel;
            _MainCamera = Camera.main;
        }

        public override void Initialize()
        {
            _ColliderRegistry.Register(View.Collider, View);

            View.OnTriggerEntered
                .Subscribe(HandleTrigger)
                .AddTo(Disposables);

            View.OnUpdated
                .Subscribe(_ => Tick(Time.deltaTime))
                .AddTo(Disposables);
        }

        public override void Tick(float deltaTime)
        {
            if (Model.IsInFlight)
            {
                UpdateFlight(deltaTime);
                CheckOutOfBounds();
            }
            else if (Model.IsBouncing)
            {
                UpdateBounce(deltaTime);
                CheckOutOfBounds();
            }
        }

        /// <summary>
        /// Launches the knife.
        /// </summary>
        public void Launch(Vector2 direction, float speed)
        {
            Model.IsInFlight = true;
            Model.IsPooled = false;
            Model.HasReportedOutOfView = false;
            Model.Velocity = direction.normalized * speed;
            View.SetColliderEnabled(true);
        }

        /// <summary>
        /// Stops the knife (stuck in target).
        /// </summary>
        public void Stick()
        {
            Model.IsInFlight = false;
            Model.IsStuck = true;
            Model.Velocity = Vector2.zero;
            View.SetColliderEnabled(false);
        }

        /// <summary>
        /// Bounces the knife.
        /// </summary>
        public void Bounce(Vector2 bounceVelocity, float rotationSpeed)
        {
            Model.IsInFlight = false;
            Model.IsBouncing = true;
            Model.Velocity = bounceVelocity;
            Model.RotationSpeed = rotationSpeed;
            View.SetColliderEnabled(false);
        }

        /// <summary>
        /// Returns knife to pool.
        /// </summary>
        public void ReturnToPool()
        {
            Model.Reset();
            _PoolProvider?.Release(View);
        }

        public void AssignPoolProvider(KnifePoolProvider provider)
        {
            _PoolProvider = provider;
        }

        private void HandleTrigger(Collider2D other)
        {
            if (!Model.IsInFlight) return;

            if (!_ColliderRegistry.TryGetHitable(other, out var hitable))
                return;

            Vector2 impactPoint = other.ClosestPoint(View.Transform.position);
            _CollisionChannel?.Raise(new CollisionData(View, hitable, impactPoint));
        }

        private void UpdateFlight(float deltaTime)
        {
            View.Move(Model.Velocity * deltaTime);
        }

        private void UpdateBounce(float deltaTime)
        {
            Model.Velocity += Physics2D.gravity * deltaTime;
            View.Move(Model.Velocity * deltaTime);
            View.Rotate(Model.RotationSpeed * deltaTime);
        }

        private void CheckOutOfBounds()
        {
            if (Model.HasReportedOutOfView) return;
            if (_MainCamera == null) return;

            Vector3 viewportPoint = _MainCamera.WorldToViewportPoint(View.Transform.position);
            bool isOutOfView = viewportPoint.x < 0f || viewportPoint.x > 1f 
                            || viewportPoint.y < 0f || viewportPoint.y > 1f;

            if (!isOutOfView) return;

            Model.HasReportedOutOfView = true;
            _ImpactChannel?.Raise(new KnifeImpactEventData(KnifeImpactType.OutOfView, View.Transform.position));
            ReturnToPool();
        }

        public override void Dispose()
        {
            _ColliderRegistry.Unregister(View.Collider);
            base.Dispose();
        }
    }
}
```

## Step 1.5: Create ColliderRegistry

**Create**: `Assets/Scripts/Game/Services/ColliderRegistry.cs`

```csharp
using System.Collections.Generic;
using Common.Collision.Interfaces;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Maps colliders to their IHitable owners. Eliminates GetComponent calls.
    /// </summary>
    public class ColliderRegistry
    {
        private readonly Dictionary<Collider2D, IHitable> _Map = new();

        public void Register(Collider2D collider, IHitable hitable)
        {
            if (collider != null && hitable != null)
                _Map[collider] = hitable;
        }

        public void Unregister(Collider2D collider)
        {
            if (collider != null)
                _Map.Remove(collider);
        }

        public bool TryGetHitable(Collider2D collider, out IHitable hitable)
        {
            return _Map.TryGetValue(collider, out hitable);
        }
    }
}
```

## Step 1.6: Update KnifePoolProvider

Update to create presenter along with view:

```csharp
public KnifeView Get(PowerUpConfigBase config = null)
{
    var view = GetPool(config).Get();
    var model = new KnifeModel();
    var presenter = new KnifePresenter(view, model, _ColliderRegistry, _CollisionChannel, _ImpactChannel);
    presenter.AssignPoolProvider(this);
    presenter.Initialize();
    // Store presenter reference if needed for disposal
    return view;
}
```

## Step 1.7: Update Installer

Register `ColliderRegistry` as singleton:

```csharp
builder.Register<ColliderRegistry>(Lifetime.Singleton);
```

## Step 1.8: Delete Old Files

Delete these files after migration:
- `Assets/Scripts/Game/Entities/Knife/States/` (entire folder - state machine replaced by presenter)
- `Assets/Scripts/Game/Entities/Knife/Interfaces/IKnifeViewContext.cs`

**Verification**:
- [ ] Create EntityPresenter base class
- [ ] Create KnifeModel
- [ ] Refactor KnifeView to passive
- [ ] Create KnifePresenter
- [ ] Create ColliderRegistry
- [ ] Update KnifePoolProvider
- [ ] Update installer
- [ ] Unity compiles
- [ ] Play game: throw knife → flies
- [ ] Play game: knife hits target → sticks
- [ ] Play game: knife hits knife → bounces
- [ ] Play game: knife goes off screen → returns to pool
- [ ] Delete old state machine files

---

# PHASE 2: MVP for Target Entity
**Goal**: Refactor TargetView to MVP pattern.

## Step 2.1: Create TargetModel

```csharp
public class TargetModel : IModel
{
    public int HitsRequired { get; set; }
    public int HitsTaken { get; set; }
    public bool IsDestroyed { get; set; }
    public List<IStickable> StuckObjects { get; } = new();
}
```

## Step 2.2: Refactor TargetView to Passive

- Remove `PlayStuckKnifeShatters()` and its KnifeView dependency
- Remove hit tracking logic
- Expose Unity events as observables
- Keep behavior runtimes (they're config-driven animation)

## Step 2.3: Create TargetPresenter

- Owns hit tracking
- Handles destruction logic
- Coordinates with stuck objects via presenter (not view)

**Verification**:
- [ ] Create TargetModel
- [ ] Refactor TargetView
- [ ] Create TargetPresenter
- [ ] Remove KnifeView import from TargetView
- [ ] Unity compiles
- [ ] Play game: target spawns
- [ ] Play game: knife sticks to target
- [ ] Play game: target destroys after enough hits
- [ ] Play game: knives shatter when target destroyed

---

# PHASE 3: MVP for Collectible Entity
**Goal**: Refactor CollectibleView to MVP pattern.

## Step 3.1: Create CollectibleModel

```csharp
public class CollectibleModel : IModel
{
    public bool IsCollected { get; set; }
}
```

## Step 3.2: Refactor CollectibleView to Passive

- Remove any logic
- Expose collision event
- Register with ColliderRegistry

## Step 3.3: Create CollectiblePresenter

- Handles collection logic
- Raises collection event

**Verification**:
- [ ] Create CollectibleModel
- [ ] Refactor CollectibleView
- [ ] Create CollectiblePresenter
- [ ] Unity compiles
- [ ] Play game: collectible spawns on target
- [ ] Play game: knife hits collectible → collected

---

# PHASE 4: MVP for KnifeThrower Entity
**Goal**: Refactor KnifeThrowerView to MVP pattern.

## Step 4.1: Create ThrowerModel

```csharp
public class ThrowerModel : IModel
{
    public bool CanThrow { get; set; }
    public int KnivesToThrow { get; set; }
}
```

## Step 4.2: Refactor KnifeThrowerView to Passive

- Remove throw logic
- Expose input events
- Keep spawn point references

## Step 4.3: Create ThrowerPresenter

- Owns throw logic
- Coordinates with KnifePoolProvider
- Responds to input

**Verification**:
- [ ] Create ThrowerModel
- [ ] Refactor KnifeThrowerView
- [ ] Create ThrowerPresenter
- [ ] Unity compiles
- [ ] Play game: input → throw → knife flies

---

# PHASE 5: Delete Dead Code
**Goal**: Remove unused code.

- Delete `KnifePoolService.cs` (empty)
- Delete empty folders
- Delete old state machine files (if not done in Phase 1)

**Verification**:
- [ ] Delete files
- [ ] Unity compiles
- [ ] Game works

---

# PHASE 6: Folder Reorganization
**Goal**: Clean up folder structure.

Create top-level folders:
```
Assets/Scripts/
├── Common/
├── Entities/
│   ├── Knife/
│   │   ├── Models/
│   │   ├── Views/
│   │   └── Presenters/
│   ├── Target/
│   ├── Collectible/
│   └── Thrower/
├── Services/
├── Flow/
├── UI/
├── Configs/
└── Installers/
```

**Verification**:
- [ ] Move files in Unity
- [ ] Update namespaces
- [ ] Unity compiles
- [ ] Game works

---

# PHASE 7: Assembly Definitions
**Goal**: Enforce dependencies at compile time.

Create .asmdef files that prevent wrong dependencies.

**Verification**:
- [ ] Create .asmdef files
- [ ] Unity compiles
- [ ] Wrong dependencies cause compile errors

---

# Summary

| Phase | Description | Est. Time |
|-------|-------------|-----------|
| 1 | MVP for Knife | 3 hours |
| 2 | MVP for Target | 2 hours |
| 3 | MVP for Collectible | 1 hour |
| 4 | MVP for Thrower | 1 hour |
| 5 | Delete dead code | 15 min |
| 6 | Folder reorganization | 1 hour |
| 7 | Assembly definitions | 1 hour |
| **TOTAL** | | **~9 hours** |

## Key Principles

1. **MVP for entities** - Model (data), View (passive), Presenter (logic)
2. **No GetComponent** - Use ColliderRegistry
3. **No view interfaces** - Use concrete views in presenters
4. **Decoupling via presenters** - Entities don't know each other
5. **Use existing base classes** - EntityPresenter extends the pattern

---

*Document Version: 4.0*
*Created: December 2024*
