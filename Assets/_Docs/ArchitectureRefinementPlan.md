# Architecture Refinement Plan - Complete Refactoring Guide

## Executive Summary

This document provides a comprehensive plan to refactor **all scripts** in `Assets/Scripts/Game/` to properly follow Clean Architecture with MVP patterns. This is not just file relocation—it involves splitting mixed-concern classes, creating proper abstractions, and implementing full MVP for entities.

### Core Principles

1. **Domain Layer**: Interfaces, value objects, domain events - **NO Unity lifecycle dependencies** (math types like Vector2/Vector3 are allowed)
2. **App Layer**: Services, use cases, flow controllers - depend only on Domain interfaces
3. **Infrastructure Layer**: Unity implementations, configs, factories, adapters (ScriptableObjects, serialization)
4. **Presentation Layer**: Passive Views + Presenters (MVP pattern)
5. **Persistence Layer**: Data storage implementations

---

## Current Architecture Problems

### Problem 1: Configs Have Mixed Concerns

**Current State:**
```csharp
// TargetConfig.cs - Has domain data mixed with Unity serialization/prefab refs
[Serializable]
public class TargetConfig : ISerializationCallbackReceiver
{
    [field: SerializeField] public TargetView PrefabOverride { get; }  // Prefab ref - NOT allowed in Domain
    [field: SerializeField] public float BaseScale { get; }             // Data
    [field: SerializeField] public Vector3 InitialOffset { get; }       // Data (Vector3 is allowed)
    [field: SerializeField] public int KnivesToBreak { get; }           // Data
}
```

**Solution:** Create domain interfaces + infrastructure implementations
```csharp
// Domain: ITargetConfig.cs - Interface (Vector3 is allowed in Domain)
public interface ITargetConfig
{
    float BaseScale { get; }
    Vector3 InitialOffset { get; }  // Vector3 is fine in Domain
    int KnivesToBreak { get; }
    float InitialRotation { get; }
    IReadOnlyList<ICollectibleConfig> Collectibles { get; }
}

// Infrastructure: TargetConfig.cs - Unity serialization (unchanged, but implements interface)
[Serializable]
public class TargetConfig : ISerializationCallbackReceiver, ITargetConfig { ... }
```

### Problem 2: Views Contain Business Logic

**Current State (KnifeView.cs):**
```csharp
public class KnifeView : MonoBehaviour
{
    private KnifeStateMachine _StateMachine;  // Logic in view!
    
    private void Update()
    {
        _StateMachine?.Update(Time.deltaTime);  // Logic in view!
        CheckOutOfView();                        // Logic in view!
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision routing logic in view!
    }
}
```

**Solution:** Extract logic to Presenter, View becomes passive
```csharp
// Passive View
public class KnifeView : MonoBehaviour, IKnifeView
{
    public Observable<Collider2D> OnTriggerEnter2D { get; }  // Expose as observable
    public void SetVelocity(Vector2 velocity) { ... }        // Just execute commands
    public void PlayAnimation(string name) { ... }           // Just execute commands
}

// Presenter handles all logic
public class KnifePresenter : IDisposable
{
    private readonly IKnifeView _view;
    private readonly KnifeModel _model;
    private readonly KnifeStateMachine _stateMachine;
    
    public void Update(float deltaTime)
    {
        _stateMachine.Update(deltaTime);
        CheckOutOfView();
    }
}
```

### Problem 3: Services Depend on Concrete Types

**Current State (PowerUpService.cs):**
```csharp
public class PowerUpService
{
    private readonly KnifePoolProvider _KnifePoolProvider;  // Concrete dependency
    private readonly PlayerDataService _PlayerDataService;  // Concrete dependency
}
```

**Solution:** Depend on interfaces from Domain
```csharp
// Domain interfaces
public interface IKnifePoolProvider { ... }
public interface IPlayerDataService { ... }

// App service depends on abstractions
public class PowerUpService : IPowerUpService
{
    private readonly IKnifePoolProvider _knifePoolProvider;
    private readonly IPlayerDataService _playerDataService;
}
```

---

## Target Architecture

### Layer Dependency Rules

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION                           │
│  (Views, Presenters, UI Components)                         │
│  Depends on: Domain, App                                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                          APP                                 │
│  (Services, Flow Controllers, Use Cases)                    │
│  Depends on: Domain only                                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        DOMAIN                                │
│  (Interfaces, Value Objects, Domain Events)                 │
│  Depends on: UnityEngine (math types only)                  │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                            │
│  (Configs, Factories, Unity Adapters, Event Channels)       │
│  Depends on: Domain, App                                    │
│  IMPLEMENTS: Domain interfaces                               │
└─────────────────────────────────────────────────────────────┘
```

### Unity Types in Domain - Pragmatic Approach

The Domain layer CAN reference `UnityEngine.dll` for **math types only**. These are just data structs with no Unity lifecycle dependencies.

**ALLOWED in Domain** (pure data/math, no side effects):
- `Vector2`, `Vector3`, `Vector4` - Math structs
- `Quaternion` - Rotation data
- `Color`, `Color32` - Color values
- `Mathf` - Static math utilities
- `Rect`, `Bounds` - Geometry primitives

**NOT ALLOWED in Domain** (engine lifecycle/serialization):
- `MonoBehaviour`, `ScriptableObject` - Engine objects
- `Transform`, `GameObject` - Scene objects
- `[SerializeField]`, `[Header]` - Serialization attributes
- Prefab references - Unity asset system
- Coroutines, `UnityWebRequest` - Async Unity APIs

> **Why this is fine:** Vector2/Vector3/etc are testable without Unity Test Runner. They work in regular unit tests. The real isolation goal is avoiding engine lifecycle, not math types.

### Assembly Definitions

| Assembly | Allowed Dependencies | Contains |
|----------|---------------------|----------|
| `Game.Domain` | UnityEngine (math types only) | Interfaces, pure data, value objects |
| `Game.App` | Game.Domain | Services, flow controllers |
| `Game.Infrastructure` | Game.Domain, Game.App, UnityEngine | Factories, configs, adapters |
| `Game.Presentation` | Game.Domain, Game.App, UnityEngine | Views, presenters |
| `Game.Persistence` | Game.Domain | Storage implementations |

---

## Complete Script Inventory & Refactoring Plan

### Legend
- ✅ **Keep**: No changes needed
- 🔄 **Move**: Relocate to correct layer
- ✂️ **Split**: Extract interface/implementation
- 🔨 **Refactor**: Significant logic changes (MVP)
- 🆕 **Create**: New file needed

---

## Phase 1: Domain Layer - Create Interfaces & Organize Data

### 1.1 Create Domain Interfaces for Configs

| New Interface | Purpose | Implemented By |
|---------------|---------|----------------|
| `Domain/Configs/ITargetConfig.cs` | Target configuration contract | `TargetConfig` |
| `Domain/Configs/ICollectibleConfig.cs` | Collectible configuration contract | `CollectibleConfig` |
| `Domain/Configs/ILevelConfig.cs` | Level configuration contract | `LevelConfig` |
| `Domain/Configs/IStageConfig.cs` | Stage configuration contract | `StageConfig` |
| `Domain/Configs/IPowerUpConfig.cs` | Power-up configuration contract | `PowerUpConfigBase` |
| `Domain/Configs/IKnifeThrowConfig.cs` | Throw configuration contract | `KnifeThrowConfiguration` |

**Example - ITargetConfig.cs:**
```csharp
namespace Game.Domain.Configs
{
    /// <summary>
    /// Contract for target configuration data.
    /// </summary>
    public interface ITargetConfig
    {
        float BaseScale { get; }
        int KnivesToBreak { get; }
        float InitialRotation { get; }
        IReadOnlyList<ICollectibleConfig> Collectibles { get; }
        IReadOnlyList<IPreStuckKnifeConfig> PreStuckKnives { get; }
        ITargetBehaviorSet Behaviors { get; }
    }
}
```

### 1.2 Create Domain Interfaces for Services

| New Interface | Purpose | Implemented By |
|---------------|---------|----------------|
| `Domain/Services/IKnifePoolProvider.cs` | Knife pooling contract | `KnifePoolProvider` |
| `Domain/Services/IParticlePoolProvider.cs` | Particle pooling contract | `ParticlePoolProvider` |
| `Domain/Services/IAudioPlayer.cs` | Audio playback contract | Already exists, move |
| `Domain/Services/IPlayerDataService.cs` | Player data contract | `PlayerDataService` |
| `Domain/Services/IPlayerDataStore.cs` | Storage contract | Already exists, move |

### 1.3 Create Domain Interfaces for Views

| New Interface | Purpose | Implemented By |
|---------------|---------|----------------|
| `Domain/Views/IKnifeView.cs` | Passive knife view contract | `KnifeView` |
| `Domain/Views/ITargetView.cs` | Already exists, enhance | `TargetView` |
| `Domain/Views/ICollectibleView.cs` | Passive collectible view contract | `CollectibleView` |
| `Domain/Views/IKnifeThrowerView.cs` | Already exists, move | `KnifeThrowerView` |

### 1.4 Move Existing Pure Data & Events to Domain

Since Domain can use Unity math types (Vector2, Vector3, etc.), existing event data can move directly without modification.

| File | Purpose | Action |
|------|---------|--------|
| `Entities/Knife/Data/KnifeImpactEventData.cs` | Knife impact data (uses Vector2) | 🔄 Move to `Domain/Knife/` |
| `Entities/Knife/Data/KnifeSpawnRequestData.cs` | Spawn request data | 🔄 Move to `Domain/Knife/` |
| `Entities/Collectible/Data/CollectibleHitEventData.cs` | Collectible hit data (uses Vector2) | 🔄 Move to `Domain/Collectible/` |
| `PowerUps/Data/PowerUpInventoryData.cs` | Already pure | 🔄 Move to `Domain/PowerUp/` |
| `PowerUps/Data/PowerUpRuntimeState.cs` | Runtime state | 🔄 Move to `Domain/PowerUp/` |
| `Domain/Level/LevelStats.cs` | Already pure | ✅ Keep |
| `Domain/Stage/StageStats.cs` | Already pure | ✅ Keep |
| `Domain/Player/PlayerStats.cs` | Already pure | ✅ Keep |

> **Note**: No need to create Position2D/Position3D wrappers - just use Vector2/Vector3 directly.

### 1.5 Replace Enums with Static Classes (per workspace rules)

| Current Enum | New Static Class | Location |
|--------------|------------------|----------|
| `KnifeImpactType` | `KnifeImpactTypes` | `Domain/Knife/KnifeImpactTypes.cs` |

**Example - KnifeImpactTypes.cs:**
```csharp
namespace Game.Domain.Knife
{
    /// <summary>
    /// Types of knife impact outcomes.
    /// </summary>
    public static class KnifeImpactTypes
    {
        public const int Stuck = 0;
        public const int Bounced = 1;
        public const int OutOfView = 2;
    }
}
```

---

## Phase 2: Infrastructure Layer - Implement Interfaces & Organize

### 2.1 Update Configs to Implement Domain Interfaces

| File | Action |
|------|--------|
| `Entities/Target/Data/TargetConfig.cs` | ✂️ Add `: ITargetConfig`, move to `Infrastructure/Configs/` |
| `Entities/Collectible/Data/CollectibleConfig.cs` | ✂️ Add `: ICollectibleConfig`, move to `Infrastructure/Configs/` |
| `Level/Configurations/Level/LevelConfig.cs` | ✂️ Add `: ILevelConfig`, move to `Infrastructure/Configs/` |
| `Level/Configurations/Stage/StageConfig.cs` | ✂️ Add `: IStageConfig`, move to `Infrastructure/Configs/` |
| `PowerUps/Configs/PowerUpConfigBase.cs` | ✂️ Add `: IPowerUpConfig`, move to `Infrastructure/Configs/` |
| `KnifeThrower/Configs/KnifeThrowConfigAsset.cs` | 🔄 Move to `Infrastructure/Configs/` |

### 2.2 Move Pooling to Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Pooling/KnifePoolProvider.cs` | `Infrastructure/Pooling/KnifePoolProvider.cs` | ✂️ Implement `IKnifePoolProvider` |
| `Pooling/ParticlePoolProvider.cs` | `Infrastructure/Pooling/ParticlePoolProvider.cs` | ✂️ Implement `IParticlePoolProvider` |
| `Pooling/PrefabsConfiguration.cs` | `Infrastructure/Pooling/PrefabsConfiguration.cs` | 🔄 Move |
| `Pooling/KnifePoolService.cs` | `Infrastructure/Pooling/KnifePoolService.cs` | 🔄 Move |
| `Pooling/*.cs` | `Infrastructure/Pooling/` | 🔄 Move all remaining |

### 2.3 Move Factories to Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Target/Factories/TargetFactory.cs` | `Infrastructure/Factories/TargetFactory.cs` | 🔄 Move |
| `Entities/Target/Factories/TargetBehaviorFactory.cs` | `Infrastructure/Factories/TargetBehaviorFactory.cs` | 🔄 Move |
| `Entities/Collectible/Spawning/CollectibleSpawner.cs` | `Infrastructure/Factories/CollectibleSpawner.cs` | 🔄 Move |
| `Infrastructure/Factories/StageFailFactorySO.cs` | ✅ Keep | Already correct |

### 2.4 Move Event Channels to Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Entities/Knife/Events/KnifeImpactEventChannel.cs` | `Infrastructure/Events/KnifeImpactEventChannel.cs` | 🔄 Move |
| `KnifeThrower/Events/KnifeThrownEventChannel.cs` | `Infrastructure/Events/KnifeThrownEventChannel.cs` | 🔄 Move |
| `Entities/Collectible/Events/CollectibleHitEventChannel.cs` | `Infrastructure/Events/CollectibleHitEventChannel.cs` | 🔄 Move |
| `PowerUps/Events/*.cs` | `Infrastructure/Events/` | 🔄 Move all |
| `App/Flow/Events/*.cs` | ✅ Keep | Already correct location |

### 2.5 Move Input Adapters to Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Input/InputAdapter.cs` | `Infrastructure/Input/InputAdapter.cs` | 🔄 Move |
| `Input/InputSystem_Actions.cs` | `Infrastructure/Input/InputSystem_Actions.cs` | 🔄 Move |

### 2.6 Move Audio Infrastructure

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Audio/Systems/AudioPlayer.cs` | `Infrastructure/Audio/AudioPlayer.cs` | ✂️ Implement `IAudioPlayer` |
| `Feedback/Audio/Data/SoundSettings.cs` | `Infrastructure/Audio/SoundSettings.cs` | 🔄 Move |

### 2.7 Move Animation Configs

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Animation/Data/*.cs` | `Infrastructure/Animation/` | 🔄 Move all |

### 2.8 Move Game Settings

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Settings/GameSettings.cs` | `Infrastructure/Configs/GameSettings.cs` | 🔄 Move |

---

## Phase 3: App Layer - Consolidate Services & Flow

### 3.1 Move Flow Controllers

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Flow/Game/GameData.cs` | `App/Flow/Game/GameData.cs` | 🔄 Move |
| `Flow/Game/GameStateMachine.cs` | `App/Flow/Game/GameStateMachine.cs` | 🔄 Move |
| `Flow/Game/States/*.cs` | `App/Flow/Game/States/` | 🔄 Move all |
| `Flow/Level/LevelData.cs` | `App/Flow/Level/LevelData.cs` | 🔄 Move |
| `Flow/Level/LevelFlowController.cs` | `App/Flow/Level/LevelFlowController.cs` | 🔄 Move |
| `Flow/Level/States/*.cs` | `App/Flow/Level/States/` | 🔄 Move all |
| `Flow/Stage/StageData.cs` | `App/Flow/Stage/StageData.cs` | 🔄 Move |
| `Flow/Stage/StageFlowController.cs` | `App/Flow/Stage/StageFlowController.cs` | 🔄 Move |
| `Flow/Stage/States/*.cs` | `App/Flow/Stage/States/` | 🔄 Move all |
| `Flow/Interfaces/*.cs` | `App/Flow/Interfaces/` | 🔄 Move all |
| `GameFlowController.cs` | `App/Flow/GameFlowController.cs` | 🔄 Move |

### 3.2 Move & Refactor Services

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `KnifeThrower/Services/ThrowService.cs` | `App/Services/ThrowService.cs` | ✂️ Depend on interfaces |
| `PowerUps/Services/PowerUpService.cs` | `App/Services/PowerUpService.cs` | ✂️ Depend on interfaces |
| `PowerUps/Services/CollectibleRegistry.cs` | `App/Services/CollectibleRegistry.cs` | 🔄 Move |
| `Level/Spawning/LevelSpawner.cs` | `App/Services/LevelSpawner.cs` | ✂️ Depend on interfaces |
| `UI/Services/SettingsService.cs` | `App/Services/SettingsService.cs` | 🔄 Move |

### 3.3 Move Collision Handlers to App

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `CollisionHandlers/CollisionHandlerRegistrar.cs` | `App/Collision/CollisionHandlerRegistrar.cs` | 🔄 Move |
| `CollisionHandlers/KnifeHitTargetHandler.cs` | `App/Collision/KnifeHitTargetHandler.cs` | 🔄 Move |
| `CollisionHandlers/KnifeHitKnifeHandler.cs` | `App/Collision/KnifeHitKnifeHandler.cs` | 🔄 Move |
| `CollisionHandlers/KnifeHitCollectibleHandler.cs` | `App/Collision/KnifeHitCollectibleHandler.cs` | 🔄 Move |

### 3.4 Move Service Interfaces

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `KnifeThrower/Interfaces/IThrowService.cs` | `App/Services/Interfaces/IThrowService.cs` | 🔄 Move |
| `KnifeThrower/Interfaces/IPlayerThrowInputListener.cs` | `App/Services/Interfaces/IPlayerThrowInputListener.cs` | 🔄 Move |
| `PowerUps/Interfaces/IPowerUpService.cs` | `App/Services/Interfaces/IPowerUpService.cs` | 🔄 Move |
| `Level/Interfaces/ILevelSpawner.cs` | `App/Services/Interfaces/ILevelSpawner.cs` | 🔄 Move |
| `Level/Interfaces/ILevelCatalog.cs` | `App/Services/Interfaces/ILevelCatalog.cs` | 🔄 Move |

### 3.5 Move Feedback Services to App (they're application logic)

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Feedback/Audio/Services/KnifeAudioFeedbackService.cs` | `App/Services/Feedback/KnifeAudioFeedbackService.cs` | ✂️ Depend on `IAudioPlayer` |
| `Feedback/Audio/Services/CollectibleAudioFeedbackService.cs` | `App/Services/Feedback/CollectibleAudioFeedbackService.cs` | ✂️ Depend on `IAudioPlayer` |
| `Feedback/Audio/Services/LevelAudioFeedbackService.cs` | `App/Services/Feedback/LevelAudioFeedbackService.cs` | ✂️ Depend on `IAudioPlayer` |
| `Feedback/VFX/Services/VFXFeedbackService.cs` | `App/Services/Feedback/VFXFeedbackService.cs` | 🔄 Move |

---

## Phase 4: Presentation Layer - Implement Full MVP

### 4.1 Knife MVP Refactoring

**Current Structure:**
```
Entities/Knife/
├── Views/KnifeView.cs         # Contains logic + visuals
├── States/                    # State machine
└── Feedback/                  # Animations
```

**Target Structure:**
```
Presentation/Entities/Knife/
├── Interfaces/
│   └── IKnifeView.cs          # 🆕 Passive view interface
├── Models/
│   └── KnifeModel.cs          # 🆕 Reactive state
├── Views/
│   └── KnifeView.cs           # 🔨 Refactor to passive
├── Presenters/
│   └── KnifePresenter.cs      # 🆕 All logic here
├── States/                    # 🔄 Move, used by presenter
└── Feedback/                  # 🔄 Move
```

**KnifeView Refactoring Details:**

| Remove from KnifeView | Move to KnifePresenter |
|-----------------------|------------------------|
| `_StateMachine` field | Presenter owns state machine |
| `Update()` logic | `Presenter.Tick(deltaTime)` |
| `CheckOutOfView()` | Presenter handles |
| `OnTriggerEnter2D` logic | Expose as `Observable<Collider2D>` |
| Collision channel communication | Presenter subscribes/raises |
| Pool return logic | Presenter handles |

**New IKnifeView.cs:**
```csharp
namespace Game.Presentation.Entities.Knife.Interfaces
{
    public interface IKnifeView
    {
        Transform Transform { get; }
        SpriteRenderer SpriteRenderer { get; }
        
        Observable<Collider2D> OnTriggerEntered { get; }
        
        void SetActive(bool isActive);
        void SetColliderEnabled(bool enabled);
        void SetParent(Transform parent);
        void SetLocalPosition(Vector3 position);
        void SetLocalRotation(Quaternion rotation);
        void SetVelocity(Vector2 velocity);
        void PlayShatterEffect();
        void ResetVisuals();
    }
}
```

**New KnifeModel.cs:**
```csharp
namespace Game.Presentation.Entities.Knife.Models
{
    public class KnifeModel
    {
        public ReactiveProperty<int> State { get; } = new(KnifeStates.Pooled);
        public ReactiveProperty<bool> IsSuperKnife { get; } = new(false);
        public ReactiveProperty<float> ThrowSpeed { get; } = new(0f);
        
        public void Reset()
        {
            State.Value = KnifeStates.Pooled;
            IsSuperKnife.Value = false;
            ThrowSpeed.Value = 0f;
        }
    }
}
```

**New KnifePresenter.cs:**
```csharp
namespace Game.Presentation.Entities.Knife.Presenters
{
    public class KnifePresenter : IDisposable
    {
        private readonly IKnifeView _view;
        private readonly KnifeModel _model;
        private readonly KnifeStateMachine _stateMachine;
        private readonly IEventChannel<CollisionData> _collisionChannel;
        private readonly CompositeDisposable _disposables = new();
        
        public KnifePresenter(
            IKnifeView view,
            KnifeModel model,
            IEventChannel<CollisionData> collisionChannel)
        {
            _view = view;
            _model = model;
            _collisionChannel = collisionChannel;
            
            InitializeStateMachine();
            BindView();
            BindModel();
        }
        
        private void BindView()
        {
            _view.OnTriggerEntered
                .Subscribe(HandleTriggerEnter)
                .AddTo(_disposables);
        }
        
        private void BindModel()
        {
            _model.IsSuperKnife
                .Subscribe(UpdateSuperKnifeVisuals)
                .AddTo(_disposables);
        }
        
        public void Tick(float deltaTime)
        {
            _stateMachine.Update(deltaTime);
            CheckOutOfView();
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
```

### 4.2 Target MVP Refactoring

**Current Structure:**
```
Entities/Target/
├── Views/TargetView.cs        # Contains hit logic, destruction logic
├── Logic/                     # Behavior runtimes
├── RotationBehaviors/         # Rotation implementations
├── WorldBehaviors/            # World movement implementations
├── Feedback/                  # Visual feedback
└── Services/StickPositionService.cs
```

**Target Structure:**
```
Presentation/Entities/Target/
├── Interfaces/
│   └── ITargetView.cs         # ✅ Already exists, enhance
├── Models/
│   └── TargetModel.cs         # 🆕 Reactive state
├── Views/
│   └── TargetView.cs          # 🔨 Refactor to passive
├── Presenters/
│   └── TargetPresenter.cs     # 🆕 All logic here
├── Behaviors/                 # 🔄 Move
│   ├── LocalBehaviorRuntime.cs
│   ├── WorldBehaviorRuntime.cs
│   ├── Rotation/
│   └── World/
├── Feedback/                  # 🔄 Move
└── Services/                  # 🔄 Move
```

**TargetView Refactoring Details:**

| Remove from TargetView | Move to TargetPresenter |
|------------------------|------------------------|
| `HitsReceived` tracking | `TargetModel.HitsReceived` |
| `KnivesToBreak` logic | Presenter checks |
| `RegisterHit()` | Presenter handles |
| `DestroyTarget()` decision | Presenter decides, view executes |
| Behavior tick logic | Presenter owns runtimes |
| `OnDestroyed` event | Presenter raises domain event |

**New TargetModel.cs:**
```csharp
namespace Game.Presentation.Entities.Target.Models
{
    public class TargetModel
    {
        public ReactiveProperty<int> HitsReceived { get; } = new(0);
        public ReactiveProperty<int> KnivesToBreak { get; } = new(5);
        public ReactiveProperty<float> Rotation { get; } = new(0f);
        public ReactiveProperty<bool> IsDestroyed { get; } = new(false);
        
        public bool ShouldDestroy => HitsReceived.Value >= KnivesToBreak.Value;
        
        public void RegisterHit()
        {
            HitsReceived.Value++;
        }
        
        public void Reset(int knivesToBreak, float initialRotation)
        {
            HitsReceived.Value = 0;
            KnivesToBreak.Value = knivesToBreak;
            Rotation.Value = initialRotation;
            IsDestroyed.Value = false;
        }
    }
}
```

### 4.3 Collectible MVP Refactoring

**Current Structure:**
```
Entities/Collectible/
├── Views/CollectibleView.cs   # Contains hit logic
└── Data/                      # Configs
```

**Target Structure:**
```
Presentation/Entities/Collectible/
├── Interfaces/
│   └── ICollectibleView.cs    # 🆕 Passive view interface
├── Models/
│   └── CollectibleModel.cs    # 🆕 Reactive state
├── Views/
│   └── CollectibleView.cs     # 🔨 Refactor to passive
└── Presenters/
    └── CollectiblePresenter.cs # 🆕 All logic here
```

**CollectibleView Refactoring Details:**

| Remove from CollectibleView | Move to CollectiblePresenter |
|-----------------------------|------------------------------|
| `_IsHit` flag | `CollectibleModel.IsHit` |
| `MarkAsHit()` logic | Presenter handles |
| `Value` property logic | Model holds value |
| Registry interaction | Presenter handles |

### 4.4 KnifeThrower MVP Refactoring

**Current Structure:**
```
Entities/Knife/Views/KnifeThrowerView.cs  # In wrong location
KnifeThrower/                              # Scattered
```

**Target Structure:**
```
Presentation/Entities/KnifeThrower/
├── Interfaces/
│   └── IKnifeThrowerView.cs   # 🔄 Move from KnifeThrower/Interfaces
├── Models/
│   └── KnifeThrowerModel.cs   # 🆕 Loaded knives state
├── Views/
│   └── KnifeThrowerView.cs    # 🔄 Move + refactor
└── Presenters/
    └── KnifeThrowerPresenter.cs # 🆕 Loading/throwing logic
```

### 4.5 Move UI Components to Presentation

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `UI/Controllers/UIController.cs` | `Presentation/Controllers/UIController.cs` | 🔄 Move |
| `UI/Interfaces/ILevelHudData.cs` | `Presentation/HUD/Interfaces/ILevelHudData.cs` | 🔄 Move |
| `UI/ViewModels/LevelHudViewModel.cs` | `Presentation/HUD/ViewModels/LevelHudViewModel.cs` | 🔄 Move |
| `PowerUps/Views/PowerUpButtonView.cs` | `Presentation/HUD/PowerUps/PowerUpButtonView.cs` | 🔄 Move |

---

## Phase 5: Persistence Layer Organization

| Current Path | Target Path | Action |
|-------------|-------------|--------|
| `Persistence/Data/*.cs` | ✅ Keep | Already correct |
| `Persistence/Interfaces/*.cs` | ✅ Keep | Already correct |
| `Persistence/Services/PlayerDataService.cs` | ✂️ Implement `IPlayerDataService` | Add interface |
| `Persistence/Storage/*.cs` | ✅ Keep | Already correct |

---

## Phase 6: Cleanup & Deletion

### Delete Empty Folders After Migration

```
Game/CollisionHandlers/        # Moved to App/Collision/
Game/Entities/                 # Moved to Presentation/Entities/
Game/Feedback/                 # Split to App/Services/ and Infrastructure/
Game/Flow/                     # Moved to App/Flow/
Game/Input/                    # Moved to Infrastructure/Input/
Game/KnifeThrower/             # Split between App/ and Presentation/
Game/Level/                    # Split between App/ and Infrastructure/
Game/Pooling/                  # Moved to Infrastructure/Pooling/
Game/PowerUps/                 # Split between App/, Domain/, Infrastructure/
Game/Settings/                 # Moved to Infrastructure/Configs/
Game/UI/                       # Moved to Presentation/
```

---

## Execution Order

### Recommended Sequence

```
1. FOUNDATION (Week 1)
   ├── Create folder structure
   ├── Create assembly definitions
   └── Create Domain interfaces (no breaking changes)

2. INFRASTRUCTURE (Week 2)
   ├── Add interface implementations to configs
   ├── Move pooling, factories, events
   └── Move input, audio adapters

3. APP LAYER (Week 3)
   ├── Move flow controllers
   ├── Move & refactor services
   └── Update service dependencies to use interfaces

4. PRESENTATION - MVP (Week 4-5)
   ├── Create Knife MVP (Model, Presenter, refactor View)
   ├── Create Target MVP (Model, Presenter, refactor View)
   ├── Create Collectible MVP (Model, Presenter, refactor View)
   ├── Create KnifeThrower MVP
   └── Move UI components

5. CLEANUP (Week 6)
   ├── Delete empty folders
   ├── Update all installers
   └── Full testing
```

---

## File Creation Summary

### New Files to Create

**Domain Layer (15 files):**
```
Domain/
├── Configs/
│   ├── ITargetConfig.cs
│   ├── ICollectibleConfig.cs
│   ├── ILevelConfig.cs
│   ├── IStageConfig.cs
│   ├── IPowerUpConfig.cs
│   ├── IKnifeThrowConfig.cs
│   ├── IPreStuckKnifeConfig.cs
│   └── ITargetBehaviorSet.cs
├── Services/
│   ├── IKnifePoolProvider.cs
│   └── IParticlePoolProvider.cs
├── Views/
│   └── IKnifeView.cs
└── Knife/
    └── KnifeImpactTypes.cs (replaces enum)
```

> **Note**: No Position2D/Position3D - use Unity's Vector2/Vector3 directly in Domain.

**Presentation Layer - MVP (12 files):**
```
Presentation/Entities/
├── Knife/
│   ├── Models/KnifeModel.cs
│   └── Presenters/KnifePresenter.cs
├── Target/
│   ├── Models/TargetModel.cs
│   └── Presenters/TargetPresenter.cs
├── Collectible/
│   ├── Interfaces/ICollectibleView.cs
│   ├── Models/CollectibleModel.cs
│   └── Presenters/CollectiblePresenter.cs
└── KnifeThrower/
    ├── Models/KnifeThrowerModel.cs
    └── Presenters/KnifeThrowerPresenter.cs
```

---

## Checklist

### Phase 1: Domain Interfaces
- [ ] Create all config interfaces
- [ ] Create service interfaces
- [ ] Create view interfaces
- [ ] Move existing pure data (event data, stats)
- [ ] Create KnifeImpactTypes static class (replaces enum)

### Phase 2: Infrastructure
- [ ] Update configs to implement interfaces
- [ ] Move pooling with interface implementations
- [ ] Move factories
- [ ] Move event channels
- [ ] Move input adapters
- [ ] Move audio infrastructure

### Phase 3: App Layer
- [ ] Move all flow controllers
- [ ] Move all services
- [ ] Update services to depend on interfaces
- [ ] Move collision handlers
- [ ] Move feedback services

### Phase 4: Presentation MVP
- [ ] Create KnifeModel, KnifePresenter, refactor KnifeView
- [ ] Create TargetModel, TargetPresenter, refactor TargetView
- [ ] Create CollectibleModel, CollectiblePresenter, refactor CollectibleView
- [ ] Create KnifeThrower MVP
- [ ] Move UI components

### Phase 5: Cleanup
- [ ] Delete empty folders
- [ ] Update all installers
- [ ] Update assembly references
- [ ] Full integration testing

---

## Notes

1. **DO NOT change namespaces** during moves - keeps existing references working
2. **DO NOT move meta files** - Unity handles them automatically
3. **Preserve XML summaries** - Do not delete any existing documentation
4. **Test after each phase** - Don't batch multiple phases
5. **Update installers** - DI registrations must reflect new interface types

---

*Document Version: 2.0*
*Created: December 2024*
*Complete refactoring with proper interface separation and MVP implementation*
