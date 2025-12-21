# Project Refactoring Plan - Clean Architecture & Reactive MVP

## Executive Summary

This document outlines a comprehensive, phase-by-phase migration plan to refactor the Knife Toss project to a **Scalable Clean Architecture** using **VContainer**, **R3**, and **UniTask** with strict **MVP (Model-View-Presenter)** patterns.

### Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Reactive System** | R3 with SO Event Channels | Keep SO channels for Inspector config, expose `Observable<T>` via R3 |
| **UI Actions** | Command Pattern | Single channel for all UI commands instead of individual void channels |
| **Entity Architecture** | Full MVP | All entities (Knife, Target, Collectible) use MVP pattern |
| **Async** | UniTask only | No coroutines anywhere |
| **PowerUps** | Strategy Pattern | Replace switch statements with `IPowerUpHandler` |
| **DI Scopes** | Root + Game | RootLifetimeScope (persistent) + GameLifetimeScope (per-session) |
| **Installers** | ScriptableObject | Each installer owns its dependencies |

---

## Current State Analysis

### What Already Exists ✓

| Component | Status | Location |
|-----------|--------|----------|
| MVP Core Interfaces | ✓ Complete | `Common/MVP/Core/` |
| BasePresenter (R3) | ✓ Complete | `Common/MVP/Core/BasePresenter.cs` |
| BaseMVPFactorySO | ✓ Complete | `Common/MVP/BaseMVPFactorySO.cs` |
| PopupNavigationService | ✓ Complete | `Common/MVP/Popup/` |
| StageFailPopup MVP | ✓ Complete | `Game/UI/Popups/`, `Game/UI/Presenters/` |
| State Machine Base | ✓ Complete | `Common/Patterns/State/` |
| Feedback Services (Audio/VFX) | ✓ Separated | `Game/Feedback/Audio/`, `Game/Feedback/VFX/` |
| PowerUp System | ✓ Partial | `Game/PowerUps/` (needs strategy pattern) |
| VContainer Installers | ⚠ Needs SO Migration | `Game/Installers/` |
| Event Channels (Action-based) | ⚠ Needs R3 Migration | `Common/Events/` |
| Entity Pooling | ✓ Partial | `Game/Pooling/` |

### What Needs Work ✗

| Component | Issue | Target |
|-----------|-------|--------|
| Event Channels | Action-based, not R3 | Add `AsObservable()` + new R3 channels |
| UI Commands | Many void channels | Single `UICommandChannel` with Command pattern |
| LevelData | Has Unity dependencies | Separate to pure Domain |
| HUD Components | No MVP pattern | Convert to MVP |
| Entity Views | Logic in MonoBehaviours | Convert Knife/Target/Collectible to MVP |
| UIController | Uses Subscribe/Unsubscribe | Convert to R3 streams |
| Common/Pooling | Does not exist | Create generic pooling |
| Domain Layer | Mixed with Application | Separate pure data |
| Installers | Main installer serializes all deps | Convert to ScriptableObject Installers |
| PowerUpService | Switch statements | Convert to Strategy pattern |
| DI Scopes | Single scope | Split to Root + Game scopes |

---

## Target Folder Structure

```
Assets/Scripts/
├── Common/                          # Shared abstractions (Project-agnostic)
│   ├── MVP/                         # ✓ Already exists
│   │   ├── Core/                    # IModel, IView, IPresenter, BasePresenter
│   │   ├── Navigation/              # IUINavigationService
│   │   └── Popup/                   # PopupNavigationService
│   ├── Events/                      # ⚠ Convert to R3
│   │   ├── IReactiveEventChannel.cs # NEW: R3-based interface
│   │   ├── ReactiveEventChannel.cs  # NEW: Subject<T> implementation
│   │   └── VoidReactiveEventChannel.cs # NEW: Subject<Unit> implementation
│   └── Pooling/                     # NEW: Generic pooling
│       ├── IPooledView.cs
│       └── ViewPoolProvider.cs
│
├── Game/                            # Game-specific Logic
│   ├── Domain/                      # NEW: Pure C# Data (NO Unity/R3 refs)
│   │   ├── Level/
│   │   │   └── LevelStats.cs        # Pure stats data
│   │   ├── Stage/
│   │   │   └── StageStats.cs        # Pure stage data
│   │   └── Player/
│   │       └── PlayerStats.cs       # Pure player data
│   │
│   ├── Application/                 # Game Flow & Services
│   │   ├── Flow/                    # Existing flow controllers (moved)
│   │   │   ├── Game/
│   │   │   ├── Level/
│   │   │   └── Stage/
│   │   └── Services/                # NEW: Application services
│   │       ├── ScoreService.cs
│   │       └── ProgressService.cs
│   │
│   ├── Presentation/                # UI & Visuals (MVP)
│   │   ├── Popups/                  # ✓ Exists, needs organization
│   │   │   ├── StageFail/
│   │   │   │   ├── StageFailPopupView.cs
│   │   │   │   ├── StageFailPopupModel.cs
│   │   │   │   └── StageFailPopupPresenter.cs
│   │   │   └── Settings/            # NEW: Future popups
│   │   ├── HUD/                     # Convert to MVP
│   │   │   ├── KnifeMagazine/
│   │   │   ├── StageProgress/
│   │   │   └── Collectibles/
│   │   └── Common/                  # Shared UI Components
│   │       └── BasePopupView.cs
│   │
│   ├── Infrastructure/              # Implementations & Assets
│   │   ├── Events/                  # The actual SO Asset references
│   │   ├── Factories/               # ScriptableObject Factories
│   │   └── DI/                      # ✓ Already exists
│   │
│   └── Installers/                  # ✓ Already exists
```

---

## Architectural Decisions Deep Dive

### 1. R3 Hybrid Approach (Event Channels)

**Decision:** Keep ScriptableObject-based Event Channels but expose `Observable<T>` via R3.

**Rationale:**
- SO channels provide Inspector-based configuration (drag-and-drop)
- R3 provides powerful subscription management (auto-dispose, operators)
- Hybrid gives best of both worlds

**Implementation:**

```csharp
// Existing channel gains R3 capability
public abstract class EventChannel<T> : ScriptableObject, IEventChannel<T>
{
    private readonly Subject<T> _subject = new();
    
    // Existing Action-based API (backwards compatible)
    public void Subscribe(Action<T> handler) { /* existing */ }
    public void Unsubscribe(Action<T> handler) { /* existing */ }
    
    // NEW: R3 Observable
    public Observable<T> AsObservable() => _subject.AsObservable();
    
    public void Raise(T data)
    {
        // Notify both systems
        _subject.OnNext(data);
        // ... existing Action invocation
    }
}
```

**Usage Pattern:**

```csharp
// OLD (still works)
_channel.Subscribe(OnEvent);
_channel.Unsubscribe(OnEvent);

// NEW (preferred)
_channel.AsObservable()
    .Subscribe(OnEvent)
    .AddTo(_disposables);
```

---

### 2. Command Pattern for UI Actions

**Decision:** Replace multiple `VoidEventChannel` assets with a single `UICommandChannel`.

**Current Problem:**
```
Assets/Configurations/EventChannels/UI/
├── RetryLevelClicked.asset
├── RestartStageClicked.asset
└── (more to come...)
```

**Solution:**

```csharp
// Base command interface
public interface IUICommand { }

// Specific commands (structs for zero allocation)
public readonly struct RetryLevelCommand : IUICommand { }
public readonly struct RestartStageCommand : IUICommand { }
public readonly struct OpenSettingsCommand : IUICommand { }
public readonly struct ClosePopupCommand : IUICommand { }

// Single channel for all UI commands
[CreateAssetMenu(fileName = "UICommandChannel", menuName = "Knife Toss/Events/UI Command Channel")]
public class UICommandChannel : ReactiveEventChannel<IUICommand> { }
```

**Handler Pattern:**

```csharp
public class UICommandHandler : IStartable, IDisposable
{
    private readonly UICommandChannel _channel;
    private readonly CompositeDisposable _disposables = new();
    
    public void Start()
    {
        _channel.AsObservable()
            .Subscribe(HandleCommand)
            .AddTo(_disposables);
    }
    
    private void HandleCommand(IUICommand command)
    {
        switch (command)
        {
            case RetryLevelCommand: HandleRetry(); break;
            case RestartStageCommand: HandleRestart(); break;
            // ...
        }
    }
}
```

**Benefits:**
- Single asset instead of many
- Type-safe commands with optional payload
- Easier to add new commands
- Centralized command handling

---

### 3. Full Entity MVP Pattern

**Decision:** All gameplay entities (Knife, Target, Collectible) use MVP pattern.

**Structure per Entity:**

```
Game/Entities/Knife/
├── Interfaces/
│   └── IKnifeView.cs          # View interface (passive)
├── Models/
│   └── KnifeModel.cs          # State + ReactiveProperties
├── Views/
│   └── KnifeView.cs           # MonoBehaviour, implements IKnifeView
├── Presenters/
│   └── KnifePresenter.cs      # Logic, subscribes to model/events
└── Factories/
    └── KnifeFactorySO.cs      # Creates presenter with pooling
```

**View Contract (Passive):**

```csharp
public interface IKnifeView : IPooledView
{
    Transform Transform { get; }
    Rigidbody2D Rigidbody { get; }
    
    // Visual state
    void SetSuperKnifeVisual(bool isSuper);
    void PlayStickAnimation();
    void PlayBounceAnimation();
    
    // User input (exposed as Observable)
    Observable<Unit> OnHit { get; }
}
```

**Model (Reactive State):**

```csharp
public class KnifeModel : IModel
{
    public ReactiveProperty<KnifeState> State { get; } = new(KnifeState.Idle);
    public ReactiveProperty<bool> IsSuperKnife { get; } = new(false);
    public ReactiveProperty<float> Speed { get; } = new(0f);
    
    // Pure data, no Unity dependencies
}
```

**Presenter (Logic):**

```csharp
public class KnifePresenter : BasePresenter<IKnifeView, KnifeModel>
{
    public KnifePresenter(IKnifeView view, KnifeModel model) : base(view, model)
    {
        // React to model changes
        Model.IsSuperKnife
            .Subscribe(isSuper => View.SetSuperKnifeVisual(isSuper))
            .AddTo(Disposable);
        
        Model.State
            .Where(s => s == KnifeState.Stuck)
            .Subscribe(_ => View.PlayStickAnimation())
            .AddTo(Disposable);
    }
    
    public void Throw(float speed)
    {
        Model.Speed.Value = speed;
        Model.State.Value = KnifeState.Flying;
    }
}
```

---

### 4. PowerUp Strategy Pattern

**Decision:** Replace switch statements with `IPowerUpHandler` strategy.

**Current Problem (in `PowerUpService.cs`):**

```csharp
switch (_RuntimeState.ActiveConfig)
{
    case SuperKnifeConfig: ExecuteSuperKnife(); break;
    case MultipleKnivesConfig: ExecuteMultipleKnives(); break;
    case CollectiblesBladeConfig: ExecuteCollectiblesBlade(); break;
}
```

**Solution:**

```csharp
public interface IPowerUpHandler
{
    bool CanHandle(PowerUpConfigBase config);
    UniTask Execute(PowerUpExecutionContext context);
}

public class SuperKnifeHandler : IPowerUpHandler
{
    public bool CanHandle(PowerUpConfigBase config) => config is SuperKnifeConfig;
    
    public async UniTask Execute(PowerUpExecutionContext context)
    {
        context.ThrowerView.ThrowLoadedKnife(context.Config.ThrowSpeed);
        await UniTask.CompletedTask;
    }
}

public class PowerUpHandlerResolver
{
    private readonly IReadOnlyList<IPowerUpHandler> _handlers;
    
    public IPowerUpHandler Resolve(PowerUpConfigBase config)
    {
        return _handlers.FirstOrDefault(h => h.CanHandle(config));
    }
}
```

**Refactored Service:**

```csharp
public class PowerUpService
{
    private readonly PowerUpHandlerResolver _resolver;
    
    public async UniTask<bool> TryHandleThrow(PowerUpThrowContext context)
    {
        var handler = _resolver.Resolve(_RuntimeState.ActiveConfig);
        if (handler == null) return false;
        
        await handler.Execute(context);
        ConsumeActive();
        return true;
    }
}
```

**Benefits:**
- Open/Closed Principle: Add new power-ups without modifying existing code
- Each handler is testable in isolation
- Handlers can have their own dependencies injected

---

### 5. DI Scope Hierarchy

**Decision:** Split into `RootLifetimeScope` (persistent) + `GameLifetimeScope` (per-session).

**Current Problem:**
- Single `GameInstaller` holds everything
- Settings, audio, persistence mixed with gameplay

**Solution:**

```
RootLifetimeScope (DontDestroyOnLoad)
├── Settings/Configuration
├── AudioPlayer (persistent)
├── SettingsService
├── PlayerDataService (persistence)
└── Analytics (future)

GameLifetimeScope (Per-scene/session)
├── Level/Stage flow
├── Entity factories
├── UI controllers
├── Feedback services
└── PowerUp system
```

**Implementation:**

```csharp
// Persistent scope - survives scene loads
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private BaseInstallerSO[] _PersistentInstallers;
    
    protected override void Configure(IContainerBuilder builder)
    {
        foreach (var installer in _PersistentInstallers)
            installer.Install(builder);
    }
}

// Per-session scope - created per game session
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private BaseInstallerSO[] _GameInstallers;
    
    protected override void Configure(IContainerBuilder builder)
    {
        foreach (var installer in _GameInstallers)
            installer.Install(builder);
    }
}
```

**Scene Setup:**
1. `RootScene` (loaded once, persistent): Contains `RootLifetimeScope`
2. `GameScene` (loaded per session): Contains `GameLifetimeScope` with `parent = RootLifetimeScope`

---

## Phase-by-Phase Migration Plan

---

### Phase 1: The "Common" Foundation

**Goal:** Establish R3-based reactive infrastructure without breaking existing functionality.

#### Step 1.1: Create R3 Event Channel Templates

Create new R3-based event channels that will eventually replace Action-based ones.

**New Files to Create:**

| File | Purpose |
|------|---------|
| `Common/Events/IReactiveEventChannel.cs` | Interface exposing `Observable<T>` |
| `Common/Events/ReactiveEventChannel.cs` | ScriptableObject with `Subject<T>` |
| `Common/Events/VoidReactiveEventChannel.cs` | ScriptableObject with `Subject<Unit>` |

**Implementation Notes:**
- New channels expose `Observable<T> OnRaised` for subscribers
- Keep `Raise(T data)` method for publishers
- Use `ReplaySubject<T>(bufferSize: 1)` for late subscribers if needed
- Channels are ScriptableObjects for Unity Inspector serialization

#### Step 1.2: Create Common Pooling Infrastructure

**New Files to Create:**

| File | Purpose |
|------|---------|
| `Common/Pooling/IPooledView.cs` | Interface for poolable views |
| `Common/Pooling/ViewPoolProvider.cs` | Generic pool using Unity's ObjectPool |

**Implementation Notes:**
- `IPooledView` defines `OnSpawned()` and `OnDespawned()` lifecycle hooks
- `ViewPoolProvider<TView>` manages pool lifecycle
- Integrate with `BaseMVPFactorySO` for automatic pooling

#### Step 1.3: Enhance Navigation Interfaces

**Files to Update:**

| File | Action |
|------|--------|
| `Common/MVP/Navigation/IUINavigationService.cs` | Create if not exists |
| `Common/MVP/Popup/IPopupNavigationService.cs` | Already exists ✓ |

**New Capabilities:**
- `CloseAll()` - Close entire popup stack
- `CloseByType<T>()` - Close specific popup type
- `ReplaceTop()` - Replace current popup

#### Breaking Changes & Mitigations (Phase 1):
- **None** - All new files, existing code unchanged
- Parallel implementation allows gradual migration

---

### Phase 2: Domain & Infrastructure Separation

**Goal:** Separate pure C# data from Unity-dependent runtime data.

#### Step 2.1: Identify Pure Domain Models

Analyze existing data classes and extract Unity-free portions.

**Analysis of `LevelData.cs`:**
```csharp
// CURRENT: Mixed concerns
public class LevelData
{
    public ILevelSpawner LevelSpawner { get; }     // ✗ Unity dependency
    public StageConfig StageConfig { get; }        // ✓ Could be pure
    public LevelConfig Config { get; }             // ✓ Could be pure
    public int KnifeCount { get; }                 // ✓ Pure
    public int KnivesThrown { get; }               // ✓ Pure
    public List<ITargetView> ActiveTargets { get; } // ✗ Unity dependency
}
```

**Strategy:** Create pure domain DTOs, keep runtime data separate.

**New Files to Create:**

| File | Purpose |
|------|---------|
| `Game/Domain/Level/LevelStats.cs` | Pure level statistics (knives, score) |
| `Game/Domain/Stage/StageStats.cs` | Pure stage progress data |
| `Game/Domain/Player/PlayerStats.cs` | Pure player progress data |

**Files to Update:**

| File | Action |
|------|--------|
| `Game/Flow/Level/LevelData.cs` | Keep as runtime data, reference LevelStats |

#### Step 2.2: Organize Event Channel Assets

Move ScriptableObject event assets to Infrastructure.

**Current Location:** `Assets/Configurations/EventChannels/`
**Target Location:** `Assets/Scripts/Game/Infrastructure/Events/` (code references only)

**Note:** Keep `.asset` files in `Configurations/` - only organize code.

**Files to Move:**

| From | To |
|------|-----|
| `Game/Flow/Events/*.cs` | `Game/App/Flow/Events/` |
| `Game/Entities/*/Events/*.cs` | Keep in entities (entity-specific) |

#### Breaking Changes & Mitigations (Phase 2):
- **Assembly References:** Moving files may break `asmdef` references
  - **Mitigation:** Update assembly definitions before moving
  - Add `Game.Domain.asmdef` with NO Unity references
- **Namespace Updates:** Update namespaces after moves
  - **Mitigation:** Use IDE refactoring tools

---

### Phase 3: The MVP Refactor (UI)

**Goal:** Standardize all UI components under MVP pattern with R3.

#### Step 3.1: Reorganize StageFailPopup (Reference Implementation)

Consolidate MVP triad into single feature folder.

**Files to Move:**

| From | To |
|------|-----|
| `Game/UI/Popups/StageFailPopupView.cs` | `Game/Presentation/Popups/StageFail/` |
| `Game/UI/Popups/StageFailPopupModel.cs` | `Game/Presentation/Popups/StageFail/` |
| `Game/UI/Presenters/StageFailPopupPresenter.cs` | `Game/Presentation/Popups/StageFail/` |
| `Game/UI/Factories/StageFailFactorySO.cs` | `Game/Infrastructure/Factories/` |

#### Step 3.2: Convert HUD Components to MVP

Currently, HUD components are MonoBehaviours with mixed logic.

**Files to Refactor:**

| Current File | New Structure |
|--------------|---------------|
| `KnifeMagazineDisplay.cs` | `KnifeMagazineView.cs` + `KnifeMagazinePresenter.cs` |
| `StageProgressBarView.cs` | `StageProgressView.cs` + `StageProgressPresenter.cs` |
| `CollectibleCountDisplay.cs` | `CollectibleCountView.cs` + `CollectibleCountPresenter.cs` |
| `MuteButtonView.cs` | `MuteButtonView.cs` + `MuteButtonPresenter.cs` |

**MVP Pattern for HUD:**
```
View (MonoBehaviour):
├── Exposes Observable<Unit> for user interactions
├── Provides Render(TModel) method for updates
└── NO logic, purely visual

Presenter (Plain C#):
├── Subscribes to model/event changes via R3
├── Calls View.Render() when model updates
├── Handles user interactions from View
└── Injected via VContainer

Model (Plain C# / ReactiveProperty):
├── Holds state as ReactiveProperty<T>
├── Exposes Observable<T> for changes
└── Pure data, no Unity dependencies
```

#### Step 3.3: Create HUD MVP Infrastructure

**New Files to Create:**

| File | Purpose |
|------|---------|
| `Game/Presentation/HUD/Base/IHudView.cs` | Base interface for HUD views |
| `Game/Presentation/HUD/Base/BaseHudPresenter.cs` | Base presenter for HUD |

#### Breaking Changes & Mitigations (Phase 3):
- **Prefab References:** Moving view scripts breaks prefab references
  - **Mitigation:** Use Unity's "Missing Script" recovery or update GUIDs
  - Alternative: Move in Unity Editor to preserve references
- **Installer Updates:** Factory registrations need updating
  - **Mitigation:** Update `UIInstaller.cs` with new paths
- **Namespace Changes:** All moved files need namespace updates
  - **Mitigation:** Find/replace in batch

---

### Phase 4: Service Integration & R3 Migration

**Goal:** Convert UIController to reactive mediator and complete R3 migration.

#### Step 4.1: Create Reactive UIController

Convert from Subscribe/Unsubscribe pattern to R3 streams.

**File to Refactor:** `Game/UI/Controllers/UIController.cs`

**Current Pattern (Action-based):**
```csharp
private void OnEnable()
{
    _LevelFailedChannel.Subscribe(OnLevelFailed);
}

private void OnDisable()
{
    _LevelFailedChannel.Unsubscribe(OnLevelFailed);
}
```

**Target Pattern (R3-based):**
```csharp
private CompositeDisposable _Disposables = new();

private void Start()
{
    _LevelFailedChannel.OnRaised
        .Subscribe(OnLevelFailed)
        .AddTo(_Disposables);
}

private void OnDestroy()
{
    _Disposables.Dispose();
}
```

#### Step 4.2: Migrate Event Channels to R3

Gradually replace Action-based channels with R3 versions.

**Migration Strategy (Per Channel):**
1. Create new R3 channel alongside existing
2. Update publishers to raise on both
3. Update subscribers to use R3 version
4. Remove old channel when all subscribers migrated

**Files to Migrate:**

| Current | New |
|---------|-----|
| `VoidEventChannel.cs` | Keep + add `AsObservable()` extension |
| `EventChannel<T>.cs` | Keep + add `AsObservable()` extension |

**Alternative: Adapter Approach**
Instead of replacing, add R3 observable adapter:

```csharp
public static class EventChannelExtensions
{
    public static Observable<T> AsObservable<T>(this EventChannel<T> channel)
    {
        return Observable.Create<T>(observer =>
        {
            Action<T> handler = data => observer.OnNext(data);
            channel.Subscribe(handler);
            return Disposable.Create(() => channel.Unsubscribe(handler));
        });
    }
}
```

#### Step 4.3: Implement UINavigationService

Enhance popup management with proper navigation stack.

**Files to Create/Update:**

| File | Action |
|------|--------|
| `Common/MVP/Navigation/IUINavigationService.cs` | Create interface |
| `Common/MVP/Navigation/UINavigationService.cs` | Create implementation |
| `Common/MVP/Popup/PopupNavigationService.cs` | Inherit from UINavigationService |

**New Capabilities:**
- Popup history with back navigation
- Modal vs non-modal popups
- Transition animations (via UniTask)

#### Breaking Changes & Mitigations (Phase 4):
- **Event Channel API:** Adding `AsObservable()` is non-breaking
- **UIController Changes:** Internal refactor, no external breaks
- **Navigation Service:** Additive changes only

---

### Phase 5: ScriptableObject Installers

**Goal:** Decouple installers from the main LifetimeScope by converting each installer to a ScriptableObject that serializes its own dependencies.

#### Current Problem

`GameInstaller.cs` (LifetimeScope) currently serializes ALL dependencies for the entire project:

```csharp
// CURRENT: GameInstaller holds everything
public class GameInstaller : LifetimeScope
{
    [SerializeField] private GameSettings _GameSettings;
    [SerializeField] private KnifeThrowConfigAsset _KnifeThrowConfig;
    [SerializeField] private StagesConfigAsset _StagesConfig;
    [SerializeField] private CollisionEventChannel _CollisionEventChannel;
    // ... 20+ more serialized fields ...
    
    private UIInstaller CreateUIInstaller()
    {
        return new UIInstaller(_UIController, _StageFailFactory);
    }
}
```

**Issues:**
- GameInstaller becomes a "god object" with 30+ serialized references
- Adding a new feature requires modifying GameInstaller
- Hard to test individual installers in isolation
- Violates Single Responsibility Principle

#### Step 5.1: Create Base ScriptableObject Installer

**New Files to Create:**

| File | Purpose |
|------|---------|
| `Common/DI/BaseInstallerSO.cs` | Abstract ScriptableObject implementing IInstaller |
| `Common/DI/IInstallerSO.cs` | Interface for SO installers |

**Implementation:**

```csharp
using UnityEngine;
using VContainer;

namespace Common.DI
{
    public interface IInstallerSO
    {
        void Install(IContainerBuilder builder);
    }
    
    public abstract class BaseInstallerSO : ScriptableObject, IInstallerSO
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
```

#### Step 5.2: Convert Existing Installers to ScriptableObjects

**Files to Refactor:**

| Current File | New File | Dependencies Moved |
|--------------|----------|-------------------|
| `UIInstaller.cs` | `UIInstallerSO.cs` | UIController ref, StageFailFactory |
| `AudioInstaller.cs` | `AudioInstallerSO.cs` | SoundSettings, AudioPlayer, Event Channels |
| `KnifeInstaller.cs` | `KnifeInstallerSO.cs` | KnifeThrowConfig, KnifeThrowerView, Prefabs |
| `LevelInstaller.cs` | `LevelInstallerSO.cs` | LevelRoot, LevelCompletedChannel, LevelFailedChannel |
| `StageInstaller.cs` | `StageInstallerSO.cs` | StagesConfig, Stage Event Channels |
| `TargetInstaller.cs` | `TargetInstallerSO.cs` | PrefabsConfiguration |
| `CollisionInstaller.cs` | `CollisionInstallerSO.cs` | CollisionEventChannel |
| `PowerUpInstaller.cs` | `PowerUpInstallerSO.cs` | PowerUpCatalog, PowerUp Event Channels |
| `VFXInstaller.cs` | `VFXInstallerSO.cs` | PrefabsConfiguration, KnifeImpactChannel |
| `PersistenceInstaller.cs` | `PersistenceInstallerSO.cs` | Persistence Event Channels |
| `GameFlowInstaller.cs` | `GameFlowInstallerSO.cs` | GameSettings |
| `CollectibleInstaller.cs` | `CollectibleInstallerSO.cs` | CollectibleHitEventChannel |

**Example: UIInstallerSO**

```csharp
using Common.DI;
using Common.MVP.Popup;
using Game.UI.Controllers;
using Game.UI.Factories;
using Game.UI.HUD;
using Game.PowerUps.Views;
using UnityEngine;
using VContainer;

namespace Game.Installers
{
    [CreateAssetMenu(fileName = "UIInstaller", menuName = "Knife Toss/Installers/UI")]
    public class UIInstallerSO : BaseInstallerSO
    {
        [Header("Scene References")]
        [SerializeField] private UIController _Controller;
        
        [Header("Factories")]
        [SerializeField] private StageFailFactorySO _StageFailFactory;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StageFailFactory);
            builder.Register<PopupNavigationService>(Lifetime.Singleton)
                   .As<IPopupNavigationService>();
            builder.RegisterComponent(_Controller);
            builder.RegisterComponentInHierarchy<CollectibleCountDisplay>();
            builder.RegisterComponentInHierarchy<PowerUpHudController>();
        }
    }
}
```

#### Step 5.3: Simplify GameInstaller (LifetimeScope)

**Refactor `GameInstaller.cs`:**

```csharp
using Common.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    public class GameInstaller : LifetimeScope
    {
        [Header("Installers")]
        [SerializeField] private BaseInstallerSO[] _Installers;
        
        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var installer in _Installers)
            {
                if (installer != null)
                {
                    installer.Install(builder);
                }
            }
        }
    }
}
```

#### Step 5.4: Create Installer Assets

**Asset Location:** `Assets/Configurations/Installers/`

**Assets to Create:**

| Asset Name | ScriptableObject Type |
|------------|----------------------|
| `UIInstaller.asset` | `UIInstallerSO` |
| `AudioInstaller.asset` | `AudioInstallerSO` |
| `KnifeInstaller.asset` | `KnifeInstallerSO` |
| `LevelInstaller.asset` | `LevelInstallerSO` |
| `StageInstaller.asset` | `StageInstallerSO` |
| `TargetInstaller.asset` | `TargetInstallerSO` |
| `CollisionInstaller.asset` | `CollisionInstallerSO` |
| `PowerUpInstaller.asset` | `PowerUpInstallerSO` |
| `VFXInstaller.asset` | `VFXInstallerSO` |
| `PersistenceInstaller.asset` | `PersistenceInstallerSO` |
| `GameFlowInstaller.asset` | `GameFlowInstallerSO` |
| `CollectibleInstaller.asset` | `CollectibleInstallerSO` |

#### Step 5.5: Handle Scene References in SO Installers

**Challenge:** ScriptableObjects cannot directly reference scene objects.

**Solutions:**

**Option A: Scene Reference Resolver (Recommended)**
Create a MonoBehaviour that holds scene references and is resolved by the SO installer.

```csharp
public class UISceneReferences : MonoBehaviour
{
    public UIController Controller;
    public Transform PopupRoot;
}

[CreateAssetMenu(fileName = "UIInstaller", menuName = "Knife Toss/Installers/UI")]
public class UIInstallerSO : BaseInstallerSO
{
    [SerializeField] private StageFailFactorySO _StageFailFactory;
    
    public override void Install(IContainerBuilder builder)
    {
        builder.RegisterInstance(_StageFailFactory);
        builder.Register<PopupNavigationService>(Lifetime.Singleton)
               .As<IPopupNavigationService>();
        
        builder.RegisterComponentInHierarchy<UISceneReferences>();
        builder.RegisterBuildCallback(container =>
        {
            var refs = container.Resolve<UISceneReferences>();
            container.Inject(refs.Controller);
        });
    }
}
```

**Option B: Hybrid Approach**
Keep scene references in GameInstaller, pass via Install method.

```csharp
public interface IInstallerSO
{
    void Install(IContainerBuilder builder, IObjectResolver sceneResolver);
}
```

**Option C: RegisterComponentInHierarchy**
Use VContainer's built-in hierarchy search (already used in current installers).

#### Benefits of ScriptableObject Installers

| Benefit | Description |
|---------|-------------|
| **Decoupling** | Each installer owns its dependencies |
| **Modularity** | Add/remove features by adding/removing installer assets |
| **Testability** | Test installers in isolation |
| **Reusability** | Share installers between scenes/projects |
| **Configurability** | Swap implementations via Inspector |
| **Scalability** | New features don't touch GameInstaller |

#### Breaking Changes & Mitigations (Phase 5):
- **GameInstaller Overhaul:** Complete restructure of DI setup
  - **Mitigation:** Create new SOs alongside existing, migrate one-by-one
- **Scene Reference Handling:** SOs can't hold scene refs directly
  - **Mitigation:** Use SceneReferences MonoBehaviour pattern
- **Asset Creation Required:** Each installer needs a `.asset` file
  - **Mitigation:** Create all assets before removing old code

---

## File Change Summary

### New Files to Create

```
Common/
├── DI/                                     # NEW: ScriptableObject Installer base
│   ├── IInstallerSO.cs
│   └── BaseInstallerSO.cs
├── Events/
│   ├── IReactiveEventChannel.cs
│   ├── ReactiveEventChannel.cs
│   ├── VoidReactiveEventChannel.cs
│   └── EventChannelExtensions.cs          # R3 adapter for existing channels
├── Pooling/
│   ├── IPooledView.cs
│   └── ViewPoolProvider.cs
└── MVP/
    └── Navigation/
        ├── IUINavigationService.cs
        └── UINavigationService.cs

Game/
├── Domain/
│   ├── Level/
│   │   └── LevelStats.cs
│   ├── Stage/
│   │   └── StageStats.cs
│   └── Player/
│       └── PlayerStats.cs
├── Presentation/
│   └── HUD/
│       └── Base/
│           ├── IHudView.cs
│           └── BaseHudPresenter.cs
└── Installers/                             # NEW: ScriptableObject Installers
    ├── UIInstallerSO.cs
    ├── AudioInstallerSO.cs
    ├── KnifeInstallerSO.cs
    ├── LevelInstallerSO.cs
    ├── StageInstallerSO.cs
    ├── TargetInstallerSO.cs
    ├── CollisionInstallerSO.cs
    ├── PowerUpInstallerSO.cs
    ├── VFXInstallerSO.cs
    ├── PersistenceInstallerSO.cs
    ├── GameFlowInstallerSO.cs
    ├── CollectibleInstallerSO.cs
    └── SceneReferences/                    # Scene reference holders
        ├── UISceneReferences.cs
        ├── KnifeSceneReferences.cs
        └── LevelSceneReferences.cs

Assets/Configurations/Installers/           # NEW: Installer asset instances
├── UIInstaller.asset
├── AudioInstaller.asset
├── KnifeInstaller.asset
├── LevelInstaller.asset
├── StageInstaller.asset
├── TargetInstaller.asset
├── CollisionInstaller.asset
├── PowerUpInstaller.asset
├── VFXInstaller.asset
├── PersistenceInstaller.asset
├── GameFlowInstaller.asset
└── CollectibleInstaller.asset
```

### Files to Move/Reorganize

| Current Path | New Path | Reason |
|--------------|----------|--------|
| `Game/UI/Popups/StageFailPopupView.cs` | `Game/Presentation/Popups/StageFail/` | MVP organization |
| `Game/UI/Popups/StageFailPopupModel.cs` | `Game/Presentation/Popups/StageFail/` | MVP organization |
| `Game/UI/Presenters/StageFailPopupPresenter.cs` | `Game/Presentation/Popups/StageFail/` | MVP organization |
| `Game/UI/Factories/StageFailFactorySO.cs` | `Game/Infrastructure/Factories/` | Infrastructure layer |
| `Game/UI/Common/BasePopupView.cs` | `Game/Presentation/Common/` | Presentation layer |
| `Game/UI/Controllers/UIController.cs` | `Game/Presentation/Controllers/` | Presentation layer |
| `Game/UI/HUD/*` | `Game/Presentation/HUD/*` | Presentation layer |
| `Game/Flow/*` | `Game/App/Flow/*` | Application layer |

### Files to Refactor (In-Place)

| File | Refactor Type |
|------|---------------|
| `Common/Events/EventChannel.cs` | Add `AsObservable()` method |
| `Common/Events/VoidEventChannel.cs` | Add `AsObservable()` method |
| `Game/UI/Controllers/UIController.cs` | Convert to R3 subscriptions |
| `Game/UI/HUD/KnifeMagazineDisplay.cs` | Extract to MVP pattern |
| `Game/UI/HUD/StageProgressBarView.cs` | Extract to MVP pattern |
| `Game/UI/HUD/CollectibleCountDisplay.cs` | Extract to MVP pattern |
| `Game/UI/HUD/MuteButtonView.cs` | Extract to MVP pattern |
| `Game/Installers/GameInstaller.cs` | Simplify to array of SO installers |

### Files to Delete (After Migration)

| File | Reason |
|------|--------|
| `Game/Installers/UIInstaller.cs` | Replaced by `UIInstallerSO.cs` |
| `Game/Installers/AudioInstaller.cs` | Replaced by `AudioInstallerSO.cs` |
| `Game/Installers/KnifeInstaller.cs` | Replaced by `KnifeInstallerSO.cs` |
| `Game/Installers/LevelInstaller.cs` | Replaced by `LevelInstallerSO.cs` |
| `Game/Installers/StageInstaller.cs` | Replaced by `StageInstallerSO.cs` |
| `Game/Installers/TargetInstaller.cs` | Replaced by `TargetInstallerSO.cs` |
| `Game/Installers/CollisionInstaller.cs` | Replaced by `CollisionInstallerSO.cs` |
| `Game/Installers/PowerUpInstaller.cs` | Replaced by `PowerUpInstallerSO.cs` |
| `Game/Installers/VFXInstaller.cs` | Replaced by `VFXInstallerSO.cs` |
| `Game/Installers/PersistenceInstaller.cs` | Replaced by `PersistenceInstallerSO.cs` |
| `Game/Installers/GameFlowInstaller.cs` | Replaced by `GameFlowInstallerSO.cs` |
| `Game/Installers/CollectibleInstaller.cs` | Replaced by `CollectibleInstallerSO.cs` |

---

## Assembly Definition Updates

### New Assembly Definitions

| Assembly | Dependencies |
|----------|--------------|
| `Game.Domain.asmdef` | None (pure C#) |
| `Game.Presentation.asmdef` | Common, Game.Domain, Game.App, R3, UniTask, VContainer |

### Updated Assembly References

| Assembly | Add Reference To |
|----------|------------------|
| `Common.asmdef` | R3, UniTask |
| `Game.asmdef` | Game.Domain, Game.Presentation |

---

## Risk Assessment & Mitigations

| Risk | Severity | Mitigation |
|------|----------|------------|
| Prefab script references break on move | High | Move files via Unity Editor, not file system |
| Assembly reference errors | Medium | Update `.asmdef` files BEFORE moving code |
| Namespace conflicts | Low | Use IDE refactoring for consistent renames |
| Event subscriber memory leaks | Medium | Ensure `CompositeDisposable.Dispose()` called |
| Missing DI registrations | Medium | Update installers alongside file moves |
| Runtime null references | High | Test after each phase, don't batch phases |

---

## Testing Checkpoints

### After Phase 1:
- [ ] New R3 event channels compile without errors
- [ ] Existing Action-based channels still function
- [ ] `ViewPoolProvider` can be instantiated

### After Phase 2:
- [ ] Domain assembly builds with no Unity references
- [ ] `LevelStats` can be created without Unity
- [ ] No broken script references in Unity

### After Phase 3:
- [ ] StageFailPopup still functions correctly
- [ ] All HUD elements display correctly
- [ ] MVP pattern verified with debug logging

### After Phase 4:
- [ ] UIController uses R3 subscriptions
- [ ] No memory leaks on scene reload
- [ ] Popup navigation stack works correctly

### After Phase 5:
- [ ] All SO installers created and assets configured
- [ ] GameInstaller only references SO installer array
- [ ] Scene references properly resolved via SceneReferences components
- [ ] All DI registrations work as before
- [ ] Old class-based installers deleted without errors

---

## Micro-Phase Implementation Plan

Phases are grouped into **tracks** that can run in parallel. Each micro-phase is a focused, completable unit.

---

### 🔧 FOUNDATION TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **F1** | Create `EventChannelExtensions.cs` (R3 adapter for existing channels) | None |
| **F2** | Create `IPooledView` + `ViewPoolProvider` in `Common/Pooling/` | None |
| **F3** | Create `BaseInstallerSO` + `IInstallerSO` in `Common/DI/` | None |
| **F4** | Create `IUINavigationService` interface | None |
| **F5** | Create `UICommandChannel` + Command pattern interfaces | F1 |

---

### 📦 DOMAIN TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **D1** | Create `Game.Domain.asmdef` (no Unity refs, can ref R3) | None |
| **D2** | Extract `LevelStats`, `StageStats`, `PlayerStats` pure data | D1 |
| **D3** | Move flow events to `Game/App/Flow/Events/` | None |
| **D4** | Update `LevelData` to reference `LevelStats` | D2 |

---

### 🎮 ENTITY MVP TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **E1** | Create `IKnifeView`, `KnifeModel`, `KnifePresenter` | F2 |
| **E2** | Refactor `KnifeView` to passive view | E1 |
| **E3** | Create `ITargetView`, `TargetModel`, `TargetPresenter` | F2 |
| **E4** | Refactor `TargetView` to passive view | E3 |
| **E5** | Create `ICollectibleView`, `CollectibleModel`, `CollectiblePresenter` | F2 |
| **E6** | Refactor `CollectibleView` to passive view | E5 |

---

### 🖼️ UI MVP TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **U1** | Reorganize `StageFailPopup` into single folder | None |
| **U2** | Convert `KnifeMagazineDisplay` to MVP | F1 |
| **U3** | Convert `StageProgressBarView` to MVP | F1 |
| **U4** | Convert `CollectibleCountDisplay` to MVP | F1 |
| **U5** | Convert `MuteButtonView` to MVP | F1 |
| **U6** | Convert `PowerUpHudController` to MVP | F1 |
| **U7** | Refactor `UIController` to R3 subscriptions + Command pattern | F1, F5 |

---

### ⚡ POWERUP STRATEGY TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **P1** | Create `IPowerUpHandler` interface | None |
| **P2** | Create `SuperKnifeHandler`, `MultipleKnivesHandler`, `CollectiblesBladeHandler` | P1 |
| **P3** | Create `PowerUpHandlerFactory` (resolves handler by config type) | P2 |
| **P4** | Refactor `PowerUpService` to use handlers instead of switch | P3 |

---

### 💉 INSTALLER TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **I1** | Create `UIInstallerSO` + `UISceneReferences` | F3 |
| **I2** | Create `AudioInstallerSO` + `AudioSceneReferences` | F3 |
| **I3** | Create `KnifeInstallerSO` + `KnifeSceneReferences` | F3 |
| **I4** | Create `LevelInstallerSO` + `LevelSceneReferences` | F3 |
| **I5** | Create remaining installers (Stage, Target, Collision, PowerUp, VFX, Persistence, GameFlow, Collectible) | F3 |
| **I6** | Create `RootLifetimeScope` (persistent services) | I1-I5 |
| **I7** | Simplify `GameInstaller` to array of SO installers | I6 |
| **I8** | Delete old class-based installers | I7 |

---

### 🔄 R3 MIGRATION TRACK

| Phase | Task | Dependencies |
|-------|------|--------------|
| **R1** | Convert `KnifeAudioFeedbackService` to R3 subscriptions | F1 |
| **R2** | Convert `CollectibleAudioFeedbackService` to R3 subscriptions | F1 |
| **R3** | Convert `LevelAudioFeedbackService` to R3 subscriptions | F1 |
| **R4** | Convert `VFXFeedbackService` to R3 subscriptions | F1 |
| **R5** | Convert `PowerUpService` to R3 subscriptions | F1 |
| **R6** | Convert all remaining services to R3 | R1-R5 |

---

### 📊 SUGGESTED EXECUTION ORDER

```
┌─────────────────────────────────────────────────────────────────┐
│ PARALLEL BATCH 1 (No dependencies)                              │
├─────────────────────────────────────────────────────────────────┤
│ F1, F2, F3, F4, D1, U1, P1                                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ PARALLEL BATCH 2 (Foundation complete)                          │
├─────────────────────────────────────────────────────────────────┤
│ F5, D2, D3, E1, E3, E5, U2-U6, P2, I1-I4, R1-R4                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ PARALLEL BATCH 3 (MVP + Strategy complete)                      │
├─────────────────────────────────────────────────────────────────┤
│ D4, E2, E4, E6, U7, P3, P4, I5, R5                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ BATCH 4 (Final integration)                                     │
├─────────────────────────────────────────────────────────────────┤
│ I6, I7, R6                                                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ BATCH 5 (Cleanup)                                               │
├─────────────────────────────────────────────────────────────────┤
│ I8, Delete deprecated files                                     │
└─────────────────────────────────────────────────────────────────┘
```

---

## Phase Checklist

Use this to track progress:

### Foundation Track
- [ ] F1: EventChannelExtensions
- [ ] F2: IPooledView + ViewPoolProvider
- [ ] F3: BaseInstallerSO + IInstallerSO
- [ ] F4: IUINavigationService
- [ ] F5: UICommandChannel + Commands

### Domain Track
- [ ] D1: Game.Domain.asmdef
- [ ] D2: LevelStats, StageStats, PlayerStats
- [ ] D3: Move flow events
- [ ] D4: Update LevelData

### Entity MVP Track
- [ ] E1: Knife MVP interfaces
- [ ] E2: KnifeView passive
- [ ] E3: Target MVP interfaces
- [ ] E4: TargetView passive
- [ ] E5: Collectible MVP interfaces
- [ ] E6: CollectibleView passive

### UI MVP Track
- [ ] U1: Reorganize StageFailPopup
- [ ] U2: KnifeMagazineDisplay MVP
- [ ] U3: StageProgressBarView MVP
- [ ] U4: CollectibleCountDisplay MVP
- [ ] U5: MuteButtonView MVP
- [ ] U6: PowerUpHudController MVP
- [ ] U7: UIController R3 + Commands

### PowerUp Strategy Track
- [ ] P1: IPowerUpHandler interface
- [ ] P2: Handler implementations
- [ ] P3: PowerUpHandlerFactory
- [ ] P4: PowerUpService refactor

### Installer Track
- [ ] I1: UIInstallerSO
- [ ] I2: AudioInstallerSO
- [ ] I3: KnifeInstallerSO
- [ ] I4: LevelInstallerSO
- [ ] I5: Remaining installers
- [ ] I6: RootLifetimeScope
- [ ] I7: Simplify GameInstaller
- [ ] I8: Delete old installers

### R3 Migration Track
- [ ] R1: KnifeAudioFeedbackService
- [ ] R2: CollectibleAudioFeedbackService
- [ ] R3: LevelAudioFeedbackService
- [ ] R4: VFXFeedbackService
- [ ] R5: PowerUpService
- [ ] R6: All remaining services

---

## Appendix: Code Templates

### A. ReactiveEventChannel Template

```csharp
using R3;
using UnityEngine;

namespace Common.Events
{
    public abstract class ReactiveEventChannel<T> : ScriptableObject
    {
        private readonly Subject<T> _subject = new();
        
        public Observable<T> OnRaised => _subject;
        
        public void Raise(T data)
        {
            _subject.OnNext(data);
        }
    }
}
```

### B. EventChannel R3 Extension

```csharp
using R3;
using System;

namespace Common.Events
{
    public static class EventChannelExtensions
    {
        public static Observable<T> AsObservable<T>(this IEventChannel<T> channel)
        {
            return Observable.Create<T>(observer =>
            {
                Action<T> handler = data => observer.OnNext(data);
                channel.Subscribe(handler);
                return Disposable.Create(() => channel.Unsubscribe(handler));
            });
        }
        
        public static Observable<Unit> AsObservable(this VoidEventChannel channel)
        {
            return Observable.Create<Unit>(observer =>
            {
                Action handler = () => observer.OnNext(Unit.Default);
                channel.Subscribe(handler);
                return Disposable.Create(() => channel.Unsubscribe(handler));
            });
        }
    }
}
```

### C. IPooledView Interface

```csharp
namespace Common.Pooling
{
    public interface IPooledView
    {
        void OnSpawned();
        void OnDespawned();
    }
}
```

### D. HUD MVP Base

```csharp
using R3;

namespace Game.Presentation.HUD.Base
{
    public interface IHudView<TData>
    {
        void Render(TData data);
    }
    
    public abstract class BaseHudPresenter<TView, TData> : IDisposable
        where TView : IHudView<TData>
    {
        protected TView View { get; }
        protected CompositeDisposable Disposables { get; } = new();
        
        protected BaseHudPresenter(TView view)
        {
            View = view;
        }
        
        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
```

### E. ScriptableObject Installer Base

```csharp
using UnityEngine;
using VContainer;

namespace Common.DI
{
    /// <summary>
    /// Interface for ScriptableObject-based installers.
    /// </summary>
    public interface IInstallerSO
    {
        void Install(IContainerBuilder builder);
    }
    
    /// <summary>
    /// Base class for ScriptableObject installers that own their dependencies.
    /// Each installer serializes its own configuration and references.
    /// </summary>
    public abstract class BaseInstallerSO : ScriptableObject, IInstallerSO
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
```

### F. Example ScriptableObject Installer

```csharp
using Common.DI;
using Common.MVP.Popup;
using Game.UI.Factories;
using UnityEngine;
using VContainer;

namespace Game.Installers
{
    /// <summary>
    /// UI system installer. Serializes its own factory and service dependencies.
    /// </summary>
    [CreateAssetMenu(fileName = "UIInstaller", menuName = "Knife Toss/Installers/UI")]
    public class UIInstallerSO : BaseInstallerSO
    {
        [Header("Factories")]
        [SerializeField] private StageFailFactorySO _StageFailFactory;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_StageFailFactory);
            builder.Register<PopupNavigationService>(Lifetime.Singleton)
                   .As<IPopupNavigationService>();
            
            // Scene references are resolved via RegisterComponentInHierarchy
            builder.RegisterComponentInHierarchy<UISceneReferences>();
        }
    }
}
```

### G. Scene References Pattern

```csharp
using Game.UI.Controllers;
using UnityEngine;

namespace Game.Installers
{
    /// <summary>
    /// Holds scene-specific references that cannot be serialized in ScriptableObjects.
    /// Place this component in the scene and configure references in the Inspector.
    /// </summary>
    public class UISceneReferences : MonoBehaviour
    {
        [SerializeField] private UIController _Controller;
        [SerializeField] private Transform _PopupRoot;
        
        public UIController Controller => _Controller;
        public Transform PopupRoot => _PopupRoot;
    }
}
```

### H. Simplified GameInstaller (LifetimeScope)

```csharp
using Common.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Main composition root. Only holds references to SO installers.
    /// Each installer is responsible for its own dependencies.
    /// </summary>
    public class GameInstaller : LifetimeScope
    {
        [Header("Installers")]
        [Tooltip("Drag all installer ScriptableObject assets here")]
        [SerializeField] private BaseInstallerSO[] _Installers;
        
        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var installer in _Installers)
            {
                if (installer != null)
                {
                    installer.Install(builder);
                }
            }
        }
    }
}
```

### I. UI Command Pattern

```csharp
using R3;
using UnityEngine;

namespace Common.Commands
{
    /// <summary>
    /// Marker interface for UI commands.
    /// </summary>
    public interface IUICommand { }
}

namespace Game.UI.Commands
{
    /// <summary>
    /// Command to retry the current level.
    /// </summary>
    public readonly struct RetryLevelCommand : IUICommand { }
    
    /// <summary>
    /// Command to restart the current stage from the beginning.
    /// </summary>
    public readonly struct RestartStageCommand : IUICommand { }
    
    /// <summary>
    /// Command to open the settings popup.
    /// </summary>
    public readonly struct OpenSettingsCommand : IUICommand { }
    
    /// <summary>
    /// Command to close the topmost popup.
    /// </summary>
    public readonly struct ClosePopupCommand : IUICommand { }
    
    /// <summary>
    /// Command to return to main menu.
    /// </summary>
    public readonly struct ReturnToMenuCommand : IUICommand { }
}

namespace Common.Commands
{
    /// <summary>
    /// ScriptableObject channel for UI commands.
    /// </summary>
    [CreateAssetMenu(fileName = "UICommandChannel", menuName = "Knife Toss/Events/UI Command Channel")]
    public class UICommandChannel : ScriptableObject
    {
        private readonly Subject<IUICommand> _subject = new();
        
        /// <summary>
        /// Observable stream of UI commands.
        /// </summary>
        public Observable<IUICommand> OnCommand => _subject;
        
        /// <summary>
        /// Raises a UI command.
        /// </summary>
        public void Raise<T>(T command) where T : struct, IUICommand
        {
            _subject.OnNext(command);
        }
    }
}
```

### J. PowerUp Strategy Pattern

```csharp
using Cysharp.Threading.Tasks;
using Game.PowerUps.Configs;

namespace Game.PowerUps.Handlers
{
    /// <summary>
    /// Context passed to power-up handlers during execution.
    /// </summary>
    public class PowerUpExecutionContext
    {
        public PowerUpConfigBase Config { get; }
        public IKnifeThrowerView ThrowerView { get; }
        
        public PowerUpExecutionContext(PowerUpConfigBase config, IKnifeThrowerView throwerView)
        {
            Config = config;
            ThrowerView = throwerView;
        }
    }
    
    /// <summary>
    /// Strategy interface for power-up execution.
    /// </summary>
    public interface IPowerUpHandler
    {
        /// <summary>
        /// Checks if this handler can process the given config.
        /// </summary>
        bool CanHandle(PowerUpConfigBase config);
        
        /// <summary>
        /// Executes the power-up effect.
        /// </summary>
        UniTask Execute(PowerUpExecutionContext context);
    }
    
    /// <summary>
    /// Handler for SuperKnife power-up.
    /// </summary>
    public class SuperKnifeHandler : IPowerUpHandler
    {
        public bool CanHandle(PowerUpConfigBase config) => config is SuperKnifeConfig;
        
        public async UniTask Execute(PowerUpExecutionContext context)
        {
            var config = (SuperKnifeConfig)context.Config;
            context.ThrowerView.ThrowLoadedKnife(config.ThrowSpeed);
            await UniTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// Handler for MultipleKnives power-up.
    /// </summary>
    public class MultipleKnivesHandler : IPowerUpHandler
    {
        public bool CanHandle(PowerUpConfigBase config) => config is MultipleKnivesConfig;
        
        public async UniTask Execute(PowerUpExecutionContext context)
        {
            var config = (MultipleKnivesConfig)context.Config;
            context.ThrowerView.ThrowLoadedKnives(config.ThrowSpeed);
            await UniTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// Resolves the appropriate handler for a power-up config.
    /// </summary>
    public class PowerUpHandlerResolver
    {
        private readonly IReadOnlyList<IPowerUpHandler> _handlers;
        
        public PowerUpHandlerResolver(IEnumerable<IPowerUpHandler> handlers)
        {
            _handlers = handlers.ToList();
        }
        
        /// <summary>
        /// Finds the handler that can process the given config.
        /// </summary>
        public IPowerUpHandler Resolve(PowerUpConfigBase config)
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].CanHandle(config))
                    return _handlers[i];
            }
            return null;
        }
    }
}
```

### K. Entity MVP Example (Knife)

```csharp
using R3;
using UnityEngine;
using Common.MVP.Core;
using Common.Pooling;

namespace Game.Entities.Knife.Interfaces
{
    /// <summary>
    /// Passive view interface for knife entity.
    /// </summary>
    public interface IKnifeView : IPooledView
    {
        Transform Transform { get; }
        Rigidbody2D Rigidbody { get; }
        Collider2D Collider { get; }
        
        void SetSuperKnifeVisual(bool isSuper);
        void PlayStickAnimation();
        void PlayBounceAnimation();
        void SetParent(Transform parent);
        void SetVelocity(Vector2 velocity);
        void StopPhysics();
    }
}

namespace Game.Entities.Knife.Models
{
    /// <summary>
    /// Reactive model for knife state.
    /// </summary>
    public class KnifeModel : IModel
    {
        public ReactiveProperty<KnifeState> State { get; } = new(KnifeState.Idle);
        public ReactiveProperty<bool> IsSuperKnife { get; } = new(false);
        public ReactiveProperty<float> ThrowSpeed { get; } = new(0f);
        
        public void Reset()
        {
            State.Value = KnifeState.Idle;
            IsSuperKnife.Value = false;
            ThrowSpeed.Value = 0f;
        }
    }
    
    public static class KnifeState
    {
        public const int Idle = 0;
        public const int Loaded = 1;
        public const int Flying = 2;
        public const int Stuck = 3;
        public const int Bouncing = 4;
    }
}

namespace Game.Entities.Knife.Presenters
{
    /// <summary>
    /// Presenter handling knife logic and view updates.
    /// </summary>
    public class KnifePresenter : BasePresenter<IKnifeView, KnifeModel>
    {
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;
        
        public KnifePresenter(
            IKnifeView view,
            KnifeModel model,
            IEventChannel<KnifeImpactEventData> impactChannel) : base(view, model)
        {
            _impactChannel = impactChannel;
            BindModelToView();
        }
        
        private void BindModelToView()
        {
            Model.IsSuperKnife
                .Subscribe(isSuper => View.SetSuperKnifeVisual(isSuper))
                .AddTo(Disposable);
            
            Model.State
                .Pairwise()
                .Subscribe(pair => OnStateChanged(pair.Previous, pair.Current))
                .AddTo(Disposable);
        }
        
        private void OnStateChanged(int previousState, int newState)
        {
            if (newState == KnifeState.Stuck)
            {
                View.StopPhysics();
                View.PlayStickAnimation();
            }
            else if (newState == KnifeState.Bouncing)
            {
                View.PlayBounceAnimation();
            }
        }
        
        public void Throw(float speed)
        {
            Model.ThrowSpeed.Value = speed;
            Model.State.Value = KnifeState.Flying;
            View.SetVelocity(Vector2.up * speed);
        }
        
        public void OnHitTarget(Transform target)
        {
            Model.State.Value = KnifeState.Stuck;
            View.SetParent(target);
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck));
        }
        
        public void OnHitKnife()
        {
            Model.State.Value = KnifeState.Bouncing;
            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced));
        }
    }
}
```

### L. Root Lifetime Scope

```csharp
using Common.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Installers
{
    /// <summary>
    /// Persistent scope that survives scene loads.
    /// Contains settings, audio, persistence, and analytics.
    /// </summary>
    public class RootLifetimeScope : LifetimeScope
    {
        [Header("Persistent Installers")]
        [SerializeField] private BaseInstallerSO[] _PersistentInstallers;
        
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var installer in _PersistentInstallers)
            {
                if (installer != null)
                {
                    installer.Install(builder);
                }
            }
        }
    }
}
```

---

*Document Version: 2.0*
*Created: 2024*
*Last Updated: Added micro-phases, Command pattern, PowerUp strategy, Entity MVP, DI scopes*

