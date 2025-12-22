# Architecture Refactoring Plan

## Complete Script Audit Results

This document is based on a comprehensive audit of every script in the codebase.

---

## SCRIPT INVENTORY

### Scripts to DELETE (Truly Redundant)

| Script | Lines | Reason |
|--------|-------|--------|
| `Game/Pooling/KnifePoolService.cs` | 46 | **Empty** - Start() sets a bool, Dispose() clears it. No actual work. |
| `Game/Presentation/Common/BasePopupView.cs` | 12 | **Empty wrapper** - Just inherits BaseView<T>, adds nothing. |


### Scripts That MUST STAY (Essential)

| Script | Reason |
|--------|--------|
| `KnifePoolProvider.cs` | Real pooling logic - manages ObjectPool, warmup, get/release |
| `ParticlePoolProvider.cs` | Real pooling logic for particle effects |
| `PrefabsConfiguration.cs` | Central prefab registry used everywhere |
| `KnifeView.cs` | Core knife entity (needs refactoring, not deletion) |
| `TargetView.cs` | Core target entity (needs refactoring, not deletion) |
| `CollectibleView.cs` | Core collectible entity (needs refactoring, not deletion) |
| `KnifeThrowerView.cs` | Knife loading/throwing mechanic |
| All State files | Real state machine logic |
| All Installer files | DI registration (needed) |
| All Event channels | Event communication system |
| All Flow controllers | Game flow logic |
| `ThrowService.cs` | Coordinates throwing mechanics |
| `PowerUpService.cs` | Power-up activation/consumption |
| `PlayerDataService.cs` | Save/load logic |

### Scripts to MERGE

| Current Files | Merge Into | Reason |
|---------------|------------|--------|
| `PrefabEntry.cs` + `KnifePrefabEntry.cs` + `ParticlePrefabEntry.cs` | `PrefabEntry<T>.cs` | Nearly identical, differ only by GetComponent call |
| `LevelSceneReferences.cs` + `UISceneReferences.cs` | `SceneReferences.cs` | Both hold scene refs, combined simplifies registration |
| `KnifeAudioFeedbackService.cs` + `CollectibleAudioFeedbackService.cs` + `LevelAudioFeedbackService.cs` | `AudioFeedbackService.cs` | Identical structure, just different event subscriptions |
| `LevelStats.cs` + `PlayerStats.cs` + `StageStats.cs` | Keep separate OR merge | They're pure data, small files - debatable |

### Scripts with GetComponent Violations (Must Fix)

| Script | Line(s) | Violation |
|--------|---------|-----------|
| `KnifePrefabEntry.cs` | 14-19 | `Prefab.GetComponent<KnifeView>()` |
| `ParticlePrefabEntry.cs` | 21 | `Prefab.GetComponent<ParticleSystem>()` |
| `PrefabsConfiguration.cs` | 47-50 | Multiple GetComponent calls |
| `ParticleReturnHandler.cs` | 21 | `GetComponent<ParticleSystem>()` |
| `KnifeHitTargetHandler.cs` | 67 | `GetComponent<CollectibleView>()` |

### Scripts with Entity Coupling Issues (Must Fix)

| Script | Issue |
|--------|-------|
| `TargetView.cs` line 5, 311 | Imports and directly references `KnifeView` |
| `KnifeHitTargetHandler.cs` | Directly manipulates `KnifeView`, `TargetView`, `CollectibleView` |
| `KnifeHitKnifeHandler.cs` | Directly uses `KnifeView` |
| `KnifeHitCollectibleHandler.cs` | Directly uses `KnifeView`, `CollectibleView` |
| `CollectibleSpawner.cs` | Takes `TargetView` parameter |
| `TargetFactory.cs` | Uses `KnifeView` for pre-stuck knives |
| `CollectibleRegistry.cs` | Stores `CollectibleView` directly |
| `PowerUpService.cs` | Uses `KnifeView`, `CollectibleView` directly |
| `ThrowService.cs` | Uses `KnifeView` directly |
| `LevelSpawner.cs` | Casts to `TargetView` |

### Scripts Using Enums (Violates Rules)

| Script | Enum |
|--------|------|
| `ParticlePrefabEntry.cs` | `ParticlePrefabType` |
| `KnifeImpactType.cs` | `KnifeImpactType` |
| `TargetBehaviorKind.cs` | Likely enum (need to verify) |

---

## PHASE 1: Delete Dead Code
**Est. Time**: 15 minutes

### Step 1.1: Delete KnifePoolService.cs

**File**: `Assets/Scripts/Game/Pooling/KnifePoolService.cs`

**Current Code** (does nothing):
```csharp
public void Start()
{
    if (_IsStarted) return;
    _IsStarted = true;
}

public void Dispose()
{
    if (!_IsStarted) return;
    _IsStarted = false;
}
```

**Action**: Delete file.

**Unity Changes**: Remove registration from any installer if present.

**Verification**:
- [ ] Delete the file
- [ ] Unity compiles
- [ ] Play game, throw knives - works

---

### Step 1.2: Delete BasePopupView.cs

**File**: `Assets/Scripts/Game/Presentation/Common/BasePopupView.cs`

**Current Code** (empty wrapper):
```csharp
public abstract class BasePopupView<TModel> : BaseView<TModel>
    where TModel : IModel
{
}
```

**Action**: Delete file. Update any classes that inherit from it to use `BaseView<TModel>` directly.

**Affected Files**:
- `StageFailPopupView.cs` - change to inherit from `BaseView<TModel>`

**Verification**:
- [ ] Update inheriting classes
- [ ] Delete the file
- [ ] Unity compiles
- [ ] Popup still works

---

### Step 1.3: Delete Empty Folders

Delete all empty folders listed above in Unity (not file explorer).

**Verification**:
- [ ] Delete in Unity
- [ ] Unity compiles
- [ ] Game plays

---

## PHASE 2: Fix GetComponent Violations
**Est. Time**: 1 hour

### Step 2.1: Fix PrefabEntry Classes

**Problem**: `GetComponent` used to get typed prefab from GameObject.

**Solution**: Store typed reference directly in ScriptableObject.

**Create**: `Assets/Scripts/Game/Configs/Prefabs/TypedPrefabEntry.cs`

```csharp
using System;
using UnityEngine;

namespace Game.Configs.Prefabs
{
    /// <summary>
    /// Configuration entry for a typed prefab with pooling settings.
    /// </summary>
    [Serializable]
    public class TypedPrefabEntry<T> where T : Component
    {
        private const int DefaultWarmupCount = 5;
        private const int DefaultMaxPoolSize = 20;

        [SerializeField] private T _Prefab;
        [SerializeField] private int _WarmupCount = DefaultWarmupCount;
        [SerializeField] private int _MaxSize = DefaultMaxPoolSize;

        /// <summary>
        /// Gets the typed prefab reference.
        /// </summary>
        public T Prefab => _Prefab;

        /// <summary>
        /// Gets the pool warmup count.
        /// </summary>
        public int WarmupCount => Mathf.Max(0, _WarmupCount);

        /// <summary>
        /// Gets the maximum pool size.
        /// </summary>
        public int MaxSize => Mathf.Max(1, _MaxSize);
    }
}
```

### Step 2.2: Update PrefabsConfiguration

Replace `KnifePrefabEntry` with `TypedPrefabEntry<KnifeView>` etc.

Remove all `GetComponent` calls - prefab fields are now typed.

**Verification**:
- [ ] Create new TypedPrefabEntry
- [ ] Update PrefabsConfiguration
- [ ] Reassign prefabs in Inspector (now requires specific component)
- [ ] Unity compiles
- [ ] Play game - spawning works

### Step 2.3: Delete Old Prefab Entry Files

After verification:
- [ ] Delete `PrefabEntry.cs`
- [ ] Delete `KnifePrefabEntry.cs`
- [ ] Delete `ParticlePrefabEntry.cs`

---

### Step 2.4: Fix ParticleReturnHandler GetComponent

**File**: `Assets/Scripts/Game/Pooling/ParticleReturnHandler.cs`

**Problem**: Uses `GetComponent<ParticleSystem>()` in callback.

**Solution**: Cache reference on initialization.

```csharp
public class ParticleReturnHandler : MonoBehaviour
{
    private ParticlePoolProvider _Provider;
    private ParticleSystem _ParticleSystem;

    /// <summary>
    /// Initializes the handler with pool provider and particle system reference.
    /// </summary>
    public void Initialize(ParticlePoolProvider provider, ParticleSystem particleSystem)
    {
        _Provider = provider;
        _ParticleSystem = particleSystem;
    }

    private void OnParticleSystemStopped()
    {
        _Provider?.Release(_ParticleSystem);
    }
}
```

**Update** `ParticlePoolProvider.CreateParticle()` to pass the ParticleSystem reference.

**Verification**:
- [ ] Update ParticleReturnHandler
- [ ] Update ParticlePoolProvider
- [ ] Unity compiles
- [ ] Play game - particles return to pool

---

### Step 2.5: Fix KnifeHitTargetHandler GetComponent

**File**: `Assets/Scripts/Game/CollisionHandlers/KnifeHitTargetHandler.cs`

**Problem**: Line 67 uses `GetComponent<CollectibleView>()`.

**Solution**: Use ColliderRegistry (created later in Phase 9).

**For now**: Mark as TODO - will be fixed in Phase 9.

---

## PHASE 3: Merge Scene References
**Est. Time**: 30 minutes

### Step 3.1: Create SceneReferences

**Create**: `Assets/Scripts/Game/Installers/SceneReferences.cs`

```csharp
using Game.Entities.Knife.Views;
using Game.Presentation.HUD.Collectibles;
using Game.Presentation.HUD.KnifeMagazine;
using Game.Presentation.HUD.MuteButton;
using Game.Presentation.HUD.PowerUps;
using Game.Presentation.HUD.StageProgress;
using Game.UI.Controllers;
using UnityEngine;

namespace Game.Installers
{
    /// <summary>
    /// Unified scene references for dependency injection.
    /// </summary>
    public class SceneReferences : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;

        [Header("UI - Controllers")]
        [SerializeField] private UIController _UIController;

        [Header("UI - HUD Views")]
        [SerializeField] private KnifeMagazineView _KnifeMagazineView;
        [SerializeField] private StageProgressView _StageProgressView;
        [SerializeField] private CollectibleCountView _CollectibleCountView;
        [SerializeField] private MuteButtonView _MuteButtonView;
        [SerializeField] private PowerUpHudView _PowerUpHudView;

        /// <summary>
        /// Gets the level root transform.
        /// </summary>
        public Transform LevelRoot => _LevelRoot;

        /// <summary>
        /// Gets the knife thrower view.
        /// </summary>
        public KnifeThrowerView KnifeThrowerView => _KnifeThrowerView;

        /// <summary>
        /// Gets the UI controller.
        /// </summary>
        public UIController UIController => _UIController;

        /// <summary>
        /// Gets the knife magazine view.
        /// </summary>
        public KnifeMagazineView KnifeMagazineView => _KnifeMagazineView;

        /// <summary>
        /// Gets the stage progress view.
        /// </summary>
        public StageProgressView StageProgressView => _StageProgressView;

        /// <summary>
        /// Gets the collectible count view.
        /// </summary>
        public CollectibleCountView CollectibleCountView => _CollectibleCountView;

        /// <summary>
        /// Gets the mute button view.
        /// </summary>
        public MuteButtonView MuteButtonView => _MuteButtonView;

        /// <summary>
        /// Gets the power-up HUD view.
        /// </summary>
        public PowerUpHudView PowerUpHudView => _PowerUpHudView;
    }
}
```

### Step 3.2: Update SceneReferencesInstaller

Update to use single SceneReferences instead of two separate classes.

### Step 3.3: Unity Changes

1. Add SceneReferences component to GameInstaller GameObject
2. Copy all references from LevelSceneReferences and UISceneReferences
3. Update SceneReferencesInstaller to reference new SceneReferences
4. Remove old LevelSceneReferences and UISceneReferences components

**Verification**:
- [ ] Create SceneReferences
- [ ] Update SceneReferencesInstaller
- [ ] Configure in Unity
- [ ] Delete old files: `LevelSceneReferences.cs`, `UISceneReferences.cs`
- [ ] Unity compiles
- [ ] Play game - everything works

---

## PHASE 4: Merge Audio Services
**Est. Time**: 45 minutes

### Step 4.1: Create Unified AudioFeedbackService

**Create**: `Assets/Scripts/Game/Services/Audio/AudioFeedbackService.cs`

This merges:
- `KnifeAudioFeedbackService.cs`
- `CollectibleAudioFeedbackService.cs`
- `LevelAudioFeedbackService.cs`

```csharp
using System;
using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Knife.Data;
using Game.Feedback.Audio.Data;
using Game.Feedback.Audio.Interfaces;
using Game.Flow.Level;
using VContainer.Unity;

namespace Game.Services.Audio
{
    /// <summary>
    /// Unified service that plays audio feedback for all game events.
    /// </summary>
    public class AudioFeedbackService : IStartable, IDisposable
    {
        private readonly IAudioPlayer _AudioPlayer;
        private readonly SoundSettings _SoundSettings;
        private readonly IEventChannel _KnifeThrownChannel;
        private readonly IEventChannel<KnifeImpactEventData> _KnifeImpactChannel;
        private readonly IEventChannel<CollectibleHitEventData> _CollectibleHitChannel;
        private readonly IEventChannel<LevelData> _LevelCompletedChannel;
        private readonly IEventChannel<LevelData> _LevelFailedChannel;
        private bool _IsStarted;

        /// <summary>
        /// Initializes the audio feedback service.
        /// </summary>
        public AudioFeedbackService(
            IAudioPlayer audioPlayer,
            SoundSettings soundSettings,
            IEventChannel knifeThrownChannel,
            IEventChannel<KnifeImpactEventData> knifeImpactChannel,
            IEventChannel<CollectibleHitEventData> collectibleHitChannel,
            IEventChannel<LevelData> levelCompletedChannel,
            IEventChannel<LevelData> levelFailedChannel)
        {
            _AudioPlayer = audioPlayer;
            _SoundSettings = soundSettings;
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifeImpactChannel = knifeImpactChannel;
            _CollectibleHitChannel = collectibleHitChannel;
            _LevelCompletedChannel = levelCompletedChannel;
            _LevelFailedChannel = levelFailedChannel;
        }

        /// <summary>
        /// Subscribes to game events.
        /// </summary>
        public void Start()
        {
            if (_IsStarted)
                return;

            _KnifeThrownChannel?.Subscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Subscribe(OnKnifeImpact);
            _CollectibleHitChannel?.Subscribe(OnCollectibleHit);
            _LevelCompletedChannel?.Subscribe(OnLevelCompleted);
            _LevelFailedChannel?.Subscribe(OnLevelFailed);
            _IsStarted = true;
        }

        /// <summary>
        /// Unsubscribes from game events.
        /// </summary>
        public void Dispose()
        {
            if (!_IsStarted)
                return;

            _KnifeThrownChannel?.Unsubscribe(OnKnifeThrown);
            _KnifeImpactChannel?.Unsubscribe(OnKnifeImpact);
            _CollectibleHitChannel?.Unsubscribe(OnCollectibleHit);
            _LevelCompletedChannel?.Unsubscribe(OnLevelCompleted);
            _LevelFailedChannel?.Unsubscribe(OnLevelFailed);
            _IsStarted = false;
        }

        private void OnKnifeThrown()
        {
            Play(_SoundSettings?.KnifeThrowClip);
        }

        private void OnKnifeImpact(KnifeImpactEventData data)
        {
            var clip = data.ImpactType == KnifeImpactType.Stuck
                ? _SoundSettings?.HitTargetClip
                : _SoundSettings?.HitKnifeClip;
            Play(clip);
        }

        private void OnCollectibleHit(CollectibleHitEventData _)
        {
            Play(_SoundSettings?.HitCollectibleClip);
        }

        private void OnLevelCompleted(LevelData _)
        {
            Play(_SoundSettings?.WinClip);
        }

        private void OnLevelFailed(LevelData _)
        {
            Play(_SoundSettings?.LoseClip);
        }

        private void Play(UnityEngine.AudioClip clip)
        {
            if (clip != null)
            {
                _AudioPlayer?.PlayOneShot(clip);
            }
        }
    }
}
```

### Step 4.2: Update AudioInstallerSO

Replace three service registrations with one.

### Step 4.3: Delete Old Files

After verification:
- [ ] Delete `KnifeAudioFeedbackService.cs`
- [ ] Delete `CollectibleAudioFeedbackService.cs`
- [ ] Delete `LevelAudioFeedbackService.cs`

**Verification**:
- [ ] Create AudioFeedbackService
- [ ] Update AudioInstallerSO
- [ ] Unity compiles
- [ ] Play game - all sounds work
- [ ] Delete old files

---

## PHASE 5: Replace Enums with Static Classes
**Est. Time**: 30 minutes

### Step 5.1: Replace KnifeImpactType

**Current**: `Assets/Scripts/Game/Entities/Knife/Data/KnifeImpactType.cs`

**Replace with**:
```csharp
namespace Game.Entities.Knife.Data
{
    /// <summary>
    /// Defines knife impact outcome types.
    /// </summary>
    public static class KnifeImpactType
    {
        public const int Stuck = 0;
        public const int Bounced = 1;
        public const int OutOfView = 2;
    }
}
```

Update all usages from `KnifeImpactType.Stuck` (works same way).

### Step 5.2: Replace ParticlePrefabType

**Current**: In `ParticlePrefabEntry.cs`

**Replace with**:
```csharp
namespace Game.Pooling
{
    /// <summary>
    /// Defines particle prefab types.
    /// </summary>
    public static class ParticlePrefabType
    {
        public const int KnifeHitTarget = 0;
        public const int CollectibleHit = 1;
    }
}
```

Update `ParticlePoolProvider.cs` to use `int` instead of enum.

**Verification**:
- [ ] Replace KnifeImpactType
- [ ] Replace ParticlePrefabType
- [ ] Unity compiles
- [ ] Play game - impacts still work

---

## PHASE 6: Decouple TargetView from KnifeView
**Est. Time**: 45 minutes

### Step 6.1: Add OnParentDestroyed to IStickable

**Update**: `Assets/Scripts/Common/Interfaces/IStickable.cs`

```csharp
using UnityEngine;

namespace Common.Interfaces
{
    /// <summary>
    /// Contract for objects that can be stuck to targets.
    /// </summary>
    public interface IStickable
    {
        /// <summary>
        /// Gets the transform.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Gets the radial offset from surface.
        /// </summary>
        float RadiusOffset { get; }

        /// <summary>
        /// Gets the rotation offset when stuck.
        /// </summary>
        float RotationOffset { get; }

        /// <summary>
        /// Applies sorting layer settings.
        /// </summary>
        void ApplySorting(int sortingLayerId, int sortingOrder);

        /// <summary>
        /// Called when this object is attached to a target.
        /// </summary>
        void OnAttachedToTarget();

        /// <summary>
        /// Called when the parent target is destroyed.
        /// </summary>
        void OnParentDestroyed();
    }
}
```

### Step 6.2: Implement in KnifeView

```csharp
public void OnParentDestroyed()
{
    PlayShatterEffect();
}
```

### Step 6.3: Implement in CollectibleView

```csharp
public void OnParentDestroyed()
{
    // Collectibles are destroyed with target
}
```

### Step 6.4: Fix TargetView

Remove `using Game.Entities.Knife.Views;`

Replace `PlayStuckKnifeShatters()` method:
```csharp
private void NotifyStuckObjectsOfDestruction()
{
    for (int i = 0; i < _StuckObjects.Count; i++)
    {
        _StuckObjects[i]?.OnParentDestroyed();
    }
}
```

**Verification**:
- [ ] Update IStickable
- [ ] Implement in KnifeView
- [ ] Implement in CollectibleView
- [ ] Fix TargetView
- [ ] Remove KnifeView import from TargetView
- [ ] Unity compiles
- [ ] Play game - target destruction still shatters knives

---

## PHASE 7: Move BaseInstallerSO
**Est. Time**: 10 minutes

**Current**: `Assets/Scripts/Game/DI/BaseInstallerSO.cs`

**Move to**: `Assets/Scripts/Game/Installers/BaseInstallerSO.cs`

Update namespace from `Game.DI` to `Game.Installers`.

Update all installer files to use new namespace.

Then delete empty `Game/DI/` folder.

**Verification**:
- [ ] Move file
- [ ] Update namespace
- [ ] Update using statements in all installers
- [ ] Delete empty folder
- [ ] Unity compiles

---

## PHASE 8: Create ColliderRegistry (Fix GetComponent)
**Est. Time**: 1 hour

### Step 8.1: Create ColliderRegistry

**Create**: `Assets/Scripts/Game/Services/Collision/ColliderRegistry.cs`

```csharp
using System.Collections.Generic;
using Common.Collision.Interfaces;
using UnityEngine;

namespace Game.Services.Collision
{
    /// <summary>
    /// Maps colliders to their IHitable owners for collision lookups.
    /// </summary>
    public class ColliderRegistry
    {
        private readonly Dictionary<Collider2D, IHitable> _Lookup = new();

        /// <summary>
        /// Registers a collider mapping.
        /// </summary>
        public void Register(Collider2D collider, IHitable hitable)
        {
            if (collider != null && hitable != null)
            {
                _Lookup[collider] = hitable;
            }
        }

        /// <summary>
        /// Unregisters a collider.
        /// </summary>
        public void Unregister(Collider2D collider)
        {
            if (collider != null)
            {
                _Lookup.Remove(collider);
            }
        }

        /// <summary>
        /// Tries to get the IHitable for a collider.
        /// </summary>
        public bool TryGet(Collider2D collider, out IHitable hitable)
        {
            return _Lookup.TryGetValue(collider, out hitable);
        }
    }
}
```

### Step 8.2: Register in CollisionInstallerSO

```csharp
builder.Register<ColliderRegistry>(Lifetime.Singleton);
```

### Step 8.3: Update Views to Register Colliders

Each view (KnifeView, TargetView, CollectibleView) registers its collider on Awake/initialization.

### Step 8.4: Update KnifeHitTargetHandler

Replace GetComponent call with ColliderRegistry lookup.

**Verification**:
- [ ] Create ColliderRegistry
- [ ] Update installer
- [ ] Update views to register
- [ ] Update KnifeHitTargetHandler
- [ ] Unity compiles
- [ ] Play game - collisions work

---

## PHASE 9-16: MVP Refactoring & Folder Reorganization

(These phases remain as originally planned - MVP for each entity and folder reorganization)

---

## Summary Statistics

### Before Refactoring
- **Empty/Dead Scripts**: 2 files (58 lines)
- **Empty Folders**: 17 folders
- **GetComponent Violations**: 5 files
- **Entity Coupling Issues**: 10 files
- **Enum Violations**: 2-3 files

### After Refactoring
- **Files Deleted**: 5 (3 prefab entries + KnifePoolService + BasePopupView)
- **Files Merged**: 5 → 2 (scene refs, audio services)
- **Violations Fixed**: All GetComponent and coupling issues resolved
- **Structure**: Flat, category-based

---

## Verification Checklist Per Phase

After each phase:
- [ ] Unity compiles with no errors
- [ ] Play game from start
- [ ] Throw knives - they fly
- [ ] Hit target - knife sticks
- [ ] Hit knife - knife bounces
- [ ] Collect collectibles - counter updates
- [ ] Complete level - next level loads
- [ ] Fail level - popup shows
- [ ] Audio plays correctly
- [ ] VFX spawn correctly

---

*Document Version: 3.0 - Based on complete script audit*
*Created: December 2024*
