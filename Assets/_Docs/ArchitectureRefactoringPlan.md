# Architecture Refactoring Plan

## Overview

A phased approach to refactor the Knife-Toss codebase with testable milestones.

### Goals
1. **Reduce code** - Remove redundant scripts, consolidate logic
2. **Improve readability** - Flat structure, consistent patterns
3. **Decouple entities** - No entity knows about other entities
4. **Assembly separation** - Prevent wrong coupling at compile time
5. **Simplify views** - Views are passive; coordination happens in services
6. **Keep UI MVP** - HUD and popups use Model-View-Presenter pattern

### Implementation Rules (Applied Throughout)
- Add `<summary>` blocks to all public classes, interfaces, and methods
- Never create `.meta` files manually
- Never use `GetComponent<T>()` or `FindObject` methods
- Avoid enums - use static classes with constants
- Never use magic numbers - extract to constants or configs
- Never use IDs for identification
- Never add `sealed` keyword to classes
- No inline comments or debug logs
- Remove replaced scripts after migration

---

# PHASE 1: Delete Dead Code
**Goal**: Remove completely unused/empty code without breaking anything.

## Step 1.1: Delete KnifePoolService.cs
**File**: `Assets/Scripts/Game/Pooling/KnifePoolService.cs`

**Reason**: This class does nothing - Start() and Dispose() are empty.

**Action**: Delete the file.

**Verification**:
- [ ] Delete `KnifePoolService.cs`
- [ ] Open Unity, let it recompile
- [ ] Check Console for errors
- [ ] If errors about missing registration, remove from `KnifeInstallerSO.cs`
- [ ] Play the game, throw knives - should work identically

---

## Step 1.2: Delete Empty Folders
**Folders to delete** (all empty):
```
Assets/Scripts/Game/Entities/Collectible/Interfaces/
Assets/Scripts/Game/Entities/Collectible/Services/
Assets/Scripts/Game/Level/Data/
Assets/Scripts/Game/Level/Events/
Assets/Scripts/Game/Level/States/
Assets/Scripts/Game/DI/
Assets/Scripts/Game/Pooling/Editor/
Assets/Scripts/Game/Feedback/VFX/Data/
Assets/Scripts/Game/Feedback/VFX/Systems/
Assets/Scripts/Game/Flow/Events/
```

**Verification**:
- [ ] Delete folders in Unity (not file explorer)
- [ ] Unity recompiles with no errors
- [ ] Game plays normally

---

# PHASE 2: Merge Prefab Entry Classes
**Goal**: Consolidate 3 nearly identical prefab entry classes into 1 generic.

## Step 2.1: Create Generic PrefabEntry<T>

**Create**: `Assets/Scripts/Game/Configs/Prefabs/PrefabEntry.cs`

```csharp
using System;
using UnityEngine;

namespace Game.Configs.Prefabs
{
    /// <summary>
    /// Generic configuration entry for pooled prefabs.
    /// </summary>
    [Serializable]
    public class PrefabEntry<T> where T : Component
    {
        private const int DefaultWarmupCount = 5;
        private const int DefaultMaxSize = 20;

        [SerializeField] private T _Prefab;
        [SerializeField] private int _WarmupCount = DefaultWarmupCount;
        [SerializeField] private int _MaxSize = DefaultMaxSize;

        /// <summary>
        /// Gets the prefab instance.
        /// </summary>
        public T Prefab => _Prefab;

        /// <summary>
        /// Gets the number of instances to pre-warm the pool.
        /// </summary>
        public int WarmupCount => _WarmupCount;

        /// <summary>
        /// Gets the maximum pool size.
        /// </summary>
        public int MaxSize => _MaxSize;
    }
}
```

## Step 2.2: Update PrefabsConfiguration

**Update**: `Assets/Scripts/Game/Pooling/PrefabsConfiguration.cs`

Replace `KnifePrefabEntry` and `ParticlePrefabEntry` references with `PrefabEntry<KnifeView>` and `PrefabEntry<ParticleSystem>`.

**Verification**:
- [ ] Create new PrefabEntry.cs
- [ ] Update PrefabsConfiguration.cs to use generic version
- [ ] Unity recompiles with no errors
- [ ] Select PrefabsConfiguration assets in Inspector - fields still show
- [ ] Play game, throw knives - pooling works
- [ ] Delete old files:
  - `Assets/Scripts/Game/Pooling/PrefabEntry.cs`
  - `Assets/Scripts/Game/Pooling/KnifePrefabEntry.cs`
  - `Assets/Scripts/Game/Pooling/ParticlePrefabEntry.cs`

---

# PHASE 3: Merge Scene References
**Goal**: Consolidate 2 scene reference files into 1.

## Step 3.1: Create Unified SceneReferences

**Create**: `Assets/Scripts/Game/Installers/SceneReferences.cs`

```csharp
using UnityEngine;

namespace Game.Installers
{
    /// <summary>
    /// Provides references to key scene objects for dependency injection.
    /// </summary>
    public class SceneReferences : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private Transform _ThrowerSpawnPoint;
        [SerializeField] private Transform _KnifeThrowerView;

        [Header("UI")]
        [SerializeField] private Transform _HUDRoot;
        [SerializeField] private Transform _PopupRoot;

        /// <summary>
        /// Gets the root transform for spawning level objects.
        /// </summary>
        public Transform LevelRoot => _LevelRoot;

        /// <summary>
        /// Gets the spawn point for the knife thrower.
        /// </summary>
        public Transform ThrowerSpawnPoint => _ThrowerSpawnPoint;

        /// <summary>
        /// Gets the knife thrower view transform.
        /// </summary>
        public Transform KnifeThrowerView => _KnifeThrowerView;

        /// <summary>
        /// Gets the root transform for HUD elements.
        /// </summary>
        public Transform HUDRoot => _HUDRoot;

        /// <summary>
        /// Gets the root transform for popup elements.
        /// </summary>
        public Transform PopupRoot => _PopupRoot;
    }
}
```

## Step 3.2: Update SceneReferencesInstaller

Update `SceneReferencesInstaller.cs` to use the unified `SceneReferences`.

## Step 3.3: Update Scene GameObjects

**Unity Instructions**:
1. Find the GameObject with `LevelSceneReferences` component
2. Find the GameObject with `UISceneReferences` component  
3. Add `SceneReferences` component to the GameInstaller GameObject
4. Assign all references from both old components to the new one
5. Remove old `LevelSceneReferences` and `UISceneReferences` components

**Verification**:
- [ ] Create new SceneReferences.cs
- [ ] Update SceneReferencesInstaller.cs
- [ ] Unity compiles with no errors
- [ ] In scene, set up new component with references
- [ ] Play game, level spawns correctly
- [ ] Delete old files:
  - `Assets/Scripts/Game/Installers/LevelSceneReferences.cs`
  - `Assets/Scripts/Game/Installers/UISceneReferences.cs`

---

# PHASE 4: Consolidate Event Channels (Optional)
**Goal**: Move scattered event channels to single location for easier discovery.
**Priority**: ⚪ LOW - Organizational improvement only

Currently events are scattered:
- `Entities/Knife/Events/` - KnifeImpactEventChannel
- `Entities/Collectible/Events/` - CollectibleHitEventChannel
- `KnifeThrower/Events/` - KnifeThrownEventChannel
- `PowerUps/Events/` - PowerUpActivatedEventChannel, PowerUpUsedEventChannel
- `App/Flow/Events/` - LevelStartedEventChannel, LevelCompletedEventChannel, etc.

## Option A: Move All to Central Events Folder

**Create folder**: `Assets/Scripts/Game/Events/`

Move all event channels and data classes to this folder.

**Pros**: Single place to find all events
**Cons**: Many file moves, namespace changes, potential merge conflicts

## Option B: Keep Current Structure

Events stay with their related feature.

**Pros**: Co-located with related code
**Cons**: Need to search multiple folders

## Recommendation

**Skip this phase or defer to Phase 10** (folder reorganization). The current structure works fine - events are found via IDE search.

**Verification**:
- [ ] Decision: consolidate or keep as-is
- [ ] If consolidating, do during Phase 10

---

# PHASE 5: Review Audio Architecture (Already Consolidated)
**Status**: ✅ ALREADY DONE

The audio system is already consolidated:
- `AudioFeedbackService.cs` - Unified service handling all audio events
- `IAudioPlayer.cs` - Interface for audio playback
- `AudioPlayer.cs` - Implementation of audio playback
- `SoundSettings.cs` - Configuration for audio clips

## Optional Cleanup

Consider if `AudioPlayer` / `IAudioPlayer` abstraction is needed, or if `AudioFeedbackService` could directly use `AudioSource`. This is a minor optimization.

**Verification**:
- [ ] Review audio system - confirm it's working
- [ ] Play game, all sounds work (throw, hit, bounce, collect, win, lose)
- [ ] No changes needed unless issues found

---

# PHASE 6: Review Data Context Classes (Keep Separate)
**Status**: ⏭️ SKIP - Current design is correct

The existing data classes serve different scopes:
- `GameData` - Overall session state (stage index, total collected)
- `StageData` - Stage-level state (current level within stage)
- `LevelData` - Level runtime state (knives, targets, spawner, stats)

**Why NOT merge**:
1. Different lifecycles (Game > Stage > Level)
2. `LevelData` has complex dependencies (`ILevelSpawner`, `ActiveTargets`)
3. Merging would violate Single Responsibility Principle
4. Current design enables clean flow controller separation

**Verification**:
- [ ] Review flow controllers - confirm they use appropriate data class
- [ ] No changes needed

---

# PHASE 7: Reduce Installers (Optional)
**Goal**: Merge 12 installers into 5.
**Priority**: ⚪ LOW - Organizational improvement

**Trade-offs**:
- **Pros**: Fewer files, simpler GameInstaller setup
- **Cons**: Larger installer files, harder to find specific registrations

**Recommendation**: Consider keeping current modularity. Many small installers is fine if well-organized. Only consolidate if the number of installers is causing real problems.

## Step 7.1: Create EntityInstaller

**Create**: `Assets/Scripts/Game/Installers/EntityInstallerSO.cs`

Merges: `TargetInstallerSO`, `KnifeInstallerSO`, `CollectibleInstallerSO`

## Step 7.2: Create FlowInstaller

**Create**: `Assets/Scripts/Game/Installers/FlowInstallerSO.cs`

Merges: `GameFlowInstallerSO`, `StageInstallerSO`, `LevelInstallerSO`

## Step 7.3: Create ServiceInstaller

**Create**: `Assets/Scripts/Game/Installers/ServiceInstallerSO.cs`

Merges: `CollisionInstallerSO`, `AudioInstallerSO`, `VFXInstallerSO`, `PowerUpInstallerSO`, `PersistenceInstallerSO`

## Step 7.4: Update GameInstaller

Update to reference new consolidated installers.

**Unity Instructions**:
1. Create new installer ScriptableObject assets for each new installer
2. Configure fields on new installers by copying from old ones
3. Update GameInstaller prefab to reference new installers
4. Delete old installer assets

**Verification** (for each new installer):
- [ ] Create new installer .cs file
- [ ] Unity compiles
- [ ] Create installer asset in Unity
- [ ] Configure all fields
- [ ] Update GameInstaller to use new installer
- [ ] Play test related functionality
- [ ] After all work, delete old installer files

---

# PHASE 8: Decouple TargetView from KnifeView
**Goal**: Remove direct KnifeView reference from TargetView without polluting interfaces.

## The Problem

`TargetView.cs` directly references and casts to `KnifeView`:
```csharp
if (_StuckObjects[i] is KnifeView knife)  // Target knows about Knife - BAD
{
    knife.PlayShatterEffect();
}
```

## The Solution

**Move the responsibility to a service.** Target shouldn't decide what happens to stuck objects when it's destroyed - that's the job of a destruction handler.

## Step 8.1: Expose Stuck Objects from TargetView

Add a simple getter to `TargetView.cs`:
```csharp
/// <summary>
/// Gets the stuck objects for external processing.
/// </summary>
public IReadOnlyList<IStickable> GetStuckObjects() => _StuckObjects;
```

## Step 8.2: Create TargetDestructionHandler

**Create**: `Assets/Scripts/Game/Services/TargetDestructionHandler.cs`

```csharp
using System.Collections.Generic;
using Common.Interfaces;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Views;

namespace Game.Services
{
    /// <summary>
    /// Handles stuck object cleanup when targets are destroyed.
    /// This service owns the knowledge of what to do with each stuck object type.
    /// </summary>
    public class TargetDestructionHandler
    {
        /// <summary>
        /// Processes all stuck objects before target destruction.
        /// </summary>
        public void ProcessStuckObjects(IReadOnlyList<IStickable> stuckObjects)
        {
            if (stuckObjects == null) return;
            
            for (int i = 0; i < stuckObjects.Count; i++)
            {
                ProcessStuckObject(stuckObjects[i]);
            }
        }
        
        private void ProcessStuckObject(IStickable stickable)
        {
            if (stickable == null) return;
            
            if (stickable is KnifeView knife)
            {
                knife.PlayShatterEffect();
            }
        }
    }
}
```

**Note**: The type check `is KnifeView` is now in the SERVICE, not in TargetView. This is the correct place for it - services coordinate between entities.

## Step 8.3: Update TargetView

1. Inject `TargetDestructionHandler` 
2. Remove `PlayStuckKnifeShatters()` method entirely
3. In `DestroyTarget()`, call `_DestructionHandler.ProcessStuckObjects(GetStuckObjects())`
4. Remove the `using Game.Entities.Knife.Views;` import

## Step 8.4: Register in Installer

Add to appropriate installer:
```csharp
builder.Register<TargetDestructionHandler>(Lifetime.Singleton);
```

**Verification**:
- [ ] Add GetStuckObjects() to TargetView
- [ ] Create TargetDestructionHandler
- [ ] Update TargetView to use handler
- [ ] Remove KnifeView import from TargetView
- [ ] Register handler in installer
- [ ] Unity compiles
- [ ] Play game, destroy target → knives shatter

---

# PHASE 9: Remove GetComponent Calls
**Goal**: Eliminate all GetComponent usage per project rules.

## Step 9.1: Update KnifeHitTargetHandler

The handler currently uses `GetComponent<CollectibleView>()`. 

**Solution**: Collectibles should register themselves with a lookup service.

**Create**: `Assets/Scripts/Game/Services/Collision/ColliderRegistry.cs`

```csharp
using System.Collections.Generic;
using Common.Collision.Interfaces;
using UnityEngine;

namespace Game.Services.Collision
{
    /// <summary>
    /// Registry that maps colliders to their hitable owners.
    /// Eliminates need for GetComponent calls during collision detection.
    /// </summary>
    public class ColliderRegistry
    {
        private readonly Dictionary<Collider2D, IHitable> _ColliderToHitable = new();

        /// <summary>
        /// Registers a collider with its hitable owner.
        /// </summary>
        public void Register(Collider2D collider, IHitable hitable)
        {
            if (collider != null && hitable != null)
            {
                _ColliderToHitable[collider] = hitable;
            }
        }

        /// <summary>
        /// Unregisters a collider.
        /// </summary>
        public void Unregister(Collider2D collider)
        {
            if (collider != null)
            {
                _ColliderToHitable.Remove(collider);
            }
        }

        /// <summary>
        /// Attempts to get the hitable for a collider.
        /// </summary>
        public bool TryGetHitable(Collider2D collider, out IHitable hitable)
        {
            return _ColliderToHitable.TryGetValue(collider, out hitable);
        }
    }
}
```

## Step 9.2: Register Colliders in Views

Update `KnifeView`, `TargetView`, `CollectibleView` to register their colliders on Awake and unregister on OnDestroy.

## Step 9.3: Update Collision Handlers

Update `KnifeHitTargetHandler` and other handlers to use `ColliderRegistry` instead of `GetComponent`.

**Verification**:
- [ ] Create ColliderRegistry.cs
- [ ] Update views to register colliders
- [ ] Update collision handlers
- [ ] Remove all GetComponent calls
- [ ] Grep for "GetComponent" - should find none in Game assembly
- [ ] Play game, collisions still work correctly

---

# PHASE 10: Create New Folder Structure
**Goal**: Reorganize into the target structure.

## Step 10.1: Create Top-Level Folders

Create these folders in `Assets/Scripts/`:
```
Entities/
UI/
Services/
Flow/
Configs/
Factories/
Events/
```

## Step 10.2: Move Files in Batches

**Batch 1 - Entities**:
Move `Game/Entities/` contents to `Entities/`

**Batch 2 - Services**:
Move service-related files to `Services/`

**Batch 3 - Configs**:
Move all config files to `Configs/`

**Batch 4 - Flow**:
Move `Game/Flow/` contents to `Flow/`

**Move files in Unity** (not file explorer) to preserve references.

**Verification** (after each batch):
- [ ] Move batch of files in Unity
- [ ] Update namespaces
- [ ] Unity recompiles
- [ ] Play test related functionality

---

# PHASE 11: Simplify Entity Architecture
**Goal**: Extract logic from views into services. Keep views simple, keep existing state machines.

## Why NOT Full MVP for Game Entities?

The existing MVP pattern (`IView<TModel>` with `Show`/`Hide`) is designed for **UI elements**. Game entities are different:
- They have physics, collisions, state machines
- They're pooled (don't use Show/Hide)
- They interact with Unity's Update loop

**Instead**: Extract cross-cutting concerns to **services**. Keep the existing state machine pattern (it works).

## Step 11.1: Extract Collision Handling from KnifeView

The `OnTriggerEnter2D` method in KnifeView uses `GetComponent<IHitable>()` - this is a violation.

**Solution**: Create a collision dispatcher service that views subscribe to.

**Create**: `Assets/Scripts/Game/Services/CollisionDispatcher.cs`

```csharp
using System;
using Common.Collision.Data;
using Common.Collision.Interfaces;
using Common.Events;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Dispatches collision events. Views report raw trigger events,
    /// this service resolves IHitable and raises collision events.
    /// </summary>
    public class CollisionDispatcher
    {
        private readonly IEventChannel<CollisionData> _CollisionChannel;
        private readonly ColliderRegistry _ColliderRegistry;

        public CollisionDispatcher(
            IEventChannel<CollisionData> collisionChannel,
            ColliderRegistry colliderRegistry)
        {
            _CollisionChannel = collisionChannel;
            _ColliderRegistry = colliderRegistry;
        }

        /// <summary>
        /// Called by views when they detect a trigger collision.
        /// </summary>
        public void ReportCollision(IHitable source, Collider2D otherCollider, Vector2 impactPoint)
        {
            if (!_ColliderRegistry.TryGetHitable(otherCollider, out var target))
                return;

            _CollisionChannel?.Raise(new CollisionData(source, target, impactPoint));
        }
    }
}
```

## Step 11.2: Update KnifeView.OnTriggerEnter2D

Replace GetComponent with CollisionDispatcher call:

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (_CollisionDispatcher == null) return;
    Vector2 impactPoint = other.ClosestPoint(transform.position);
    _CollisionDispatcher.ReportCollision(this, other, impactPoint);
}
```

Add `CollisionDispatcher` to the `Construct` method injection.

## Step 11.3: Extract Out-of-View Check to Service

The out-of-view check in KnifeView's Update can be moved to `KnifeOutOfBoundsService`:

```csharp
public class KnifeOutOfBoundsService : ITickable
{
    private readonly List<KnifeView> _ActiveKnives = new();
    private readonly Camera _MainCamera;
    private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;

    public void Tick()
    {
        for (int i = _ActiveKnives.Count - 1; i >= 0; i--)
        {
            CheckKnife(_ActiveKnives[i]);
        }
    }

    public void Track(KnifeView knife) => _ActiveKnives.Add(knife);
    public void Untrack(KnifeView knife) => _ActiveKnives.Remove(knife);
}
```

**However** - this adds complexity. **Simpler approach**: Keep the check in KnifeView but have it call an injected service method. Not everything needs to be extracted.

## Step 11.4: Keep State Machines

The `KnifeStateMachine`, `KnifeInFlightState`, etc. work well. Keep them. The state pattern is good architecture.

**Verification**:
- [ ] Create CollisionDispatcher
- [ ] Update KnifeView to use CollisionDispatcher  
- [ ] Remove GetComponent from KnifeView.OnTriggerEnter2D
- [ ] Unity compiles
- [ ] Play game, collisions still work
- [ ] Knife hits target → sticks
- [ ] Knife hits knife → bounces

---

# PHASE 12: Simplify TargetView
**Goal**: Remove embedded logic from TargetView, use services for coordination.

## Step 12.1: Move Hit Tracking to Service

Create `TargetHitService` that tracks hits per target instead of each TargetView tracking its own hits:

```csharp
public class TargetHitService
{
    private readonly Dictionary<TargetView, int> _HitCounts = new();

    public void RegisterHit(TargetView target)
    {
        if (!_HitCounts.TryGetValue(target, out var count))
            count = 0;
        _HitCounts[target] = count + 1;
    }

    public int GetHitCount(TargetView target) 
        => _HitCounts.TryGetValue(target, out var count) ? count : 0;

    public void Clear(TargetView target) 
        => _HitCounts.Remove(target);
}
```

**Alternative (simpler)**: Keep hit tracking in TargetView - it's local state that makes sense there.

## Step 12.2: Keep Behavior Runtime in View

The `LocalBehaviorRuntime` and `WorldBehaviorRuntime` are fine in TargetView. They're configuration-driven movement behaviors that belong with the view.

## Step 12.3: Extract Destruction Logic

Already handled in Phase 8 with `TargetDestructionHandler`.

**Verification**:
- [ ] Review TargetView logic
- [ ] Move only cross-cutting concerns to services
- [ ] Keep local state/behavior in view
- [ ] Play game, targets work correctly

---

# PHASE 13: Simplify CollectibleView
**Goal**: Remove GetComponent usage, simplify hit handling.

## Step 13.1: CollectibleView Already Uses IHitable

CollectibleView implements `IHitable` - good. It already participates in the collision system properly.

## Step 13.2: Register Collider

In `Awake`, register with `ColliderRegistry`:
```csharp
private void Awake()
{
    _ColliderRegistry.Register(_Collider, this);
}

private void OnDestroy()
{
    _ColliderRegistry?.Unregister(_Collider);
}
```

## Step 13.3: Remove Any GetComponent Calls

Verify no GetComponent usage in CollectibleView or related handlers.

**Verification**:
- [ ] CollectibleView registers collider
- [ ] No GetComponent in collision path
- [ ] Play game, collectibles work
- [ ] Knife hits collectible → collected

---

# PHASE 14: Simplify KnifeThrowerView  
**Goal**: Review and simplify thrower logic.

## Step 14.1: Review Current Implementation

Check if KnifeThrowerView has embedded logic that should be in services.

## Step 14.2: Input Should Go Through Service

If input handling is in the view, move to `InputService` or `ThrowService`:

```csharp
public class ThrowService
{
    public void RequestThrow() { /* coordinate knife spawn and throw */ }
}
```

## Step 14.3: Keep View Passive

KnifeThrowerView should:
- Expose spawn point transforms
- Show visual feedback (aim line, etc.)
- NOT contain throw logic

**Verification**:
- [ ] Review KnifeThrowerView
- [ ] Extract throw coordination to service if embedded
- [ ] Play game, throwing works
- [ ] Input → throw → knife flies

---

# PHASE 15: Create Assembly Definitions
**Goal**: Enforce dependency rules at compile time.

## Step 15.1: Create Assembly Definition Files

**Create these .asmdef files**:

1. `Assets/Scripts/Common/Common.asmdef` - No dependencies
2. `Assets/Scripts/Entities/Entities.asmdef` - References: Common
3. `Assets/Scripts/Events/Events.asmdef` - References: Common
4. `Assets/Scripts/Configs/Configs.asmdef` - References: Common
5. `Assets/Scripts/Services/Services.asmdef` - References: Common, Entities, Events, Configs
6. `Assets/Scripts/UI/UI.asmdef` - References: Common, Services, Events
7. `Assets/Scripts/Flow/Flow.asmdef` - References: Common, Services, Events
8. `Assets/Scripts/Factories/Factories.asmdef` - References: Common, Entities, Configs, Services
9. `Assets/Scripts/Game/Game.asmdef` - References: All of above

**Verification**:
- [ ] Create each .asmdef file
- [ ] Unity recompiles after each
- [ ] Any errors indicate wrong dependencies - fix them
- [ ] Try adding wrong dependency - compiler should reject

---

# PHASE 16: Final Cleanup
**Goal**: Remove all obsolete files and empty folders.

## Step 16.1: Delete Obsolete Files

Delete any remaining files that were replaced but not yet deleted.

## Step 16.2: Delete Empty Folders

Delete any empty folders remaining after file moves.

## Step 16.3: Update Prefab References

**Unity Instructions**:
1. Check all prefabs for missing script references
2. Reassign any broken references
3. Save all prefabs

## Step 16.4: Final Testing

**Verification**:
- [ ] Full playthrough of game
- [ ] All UI elements work
- [ ] All popups show correctly
- [ ] Progress saves correctly
- [ ] Power-ups work
- [ ] No console errors

---

# Summary

| Phase | Description | Est. Time |
|-------|-------------|-----------|
| 1 | Delete dead code | 15 min |
| 2 | Merge prefab entries | 30 min |
| 3 | Merge scene references | 30 min |
| 4 | Consolidate events (optional) | 0-1 hour |
| 5 | Review audio (already done) | 10 min |
| 6 | Review data contexts (skip) | 10 min |
| 7 | Reduce installers (optional) | 0-2 hours |
| 8 | Decouple Target-Knife (service approach) | 30 min |
| 9 | Remove GetComponent (ColliderRegistry) | 1 hour |
| 10 | Reorganize folders | 2 hours |
| 11 | Simplify KnifeView (CollisionDispatcher) | 1 hour |
| 12 | Simplify TargetView (review & cleanup) | 30 min |
| 13 | Simplify CollectibleView (register collider) | 30 min |
| 14 | Simplify KnifeThrowerView (review) | 30 min |
| 15 | Assembly definitions | 1 hour |
| 16 | Final cleanup | 1 hour |
| **TOTAL** | | **~8-12 hours** |

## Key Principles Applied

1. **No interface pollution** - Use existing interfaces (`IStickable`, `IHitable`), don't create parallel hierarchies
2. **Services for cross-cutting concerns** - `TargetDestructionHandler`, `CollisionDispatcher`, `ColliderRegistry`
3. **Keep what works** - State machines, behavior runtimes are good patterns
4. **UI MVP stays for UI** - `IView<TModel>` pattern for popups, HUD elements
5. **Entity views stay simple** - Remove GetComponent, inject services, keep local state

---

*Document Version: 3.0*
*Created: December 2024*
