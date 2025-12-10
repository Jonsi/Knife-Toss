# KNIFE TOSS — Development Plan

**Version 2.2**  
**Last Updated: December 2024**  
**Based on:** GameDesignDocument.md

---

## Table of Contents

- [Plan Overview](#plan-overview)
- [Phase 0: Foundation](#phase-0-foundation) ✅ COMPLETE
- [Phase 1: Knife Flight](#phase-1-knife-flight) ✅ COMPLETE
- [Phase 2: Target & Sticking](#phase-2-target--sticking) ✅ COMPLETE
- [Phase 3: Game States](#phase-3-game-states) ✅ COMPLETE
- [Phase 4: Level Configuration](#phase-4-level-configuration) ✅ COMPLETE
- [Phase 5: Flow & Progression](#phase-5-flow--progression) ✅ COMPLETE
- [Phase 6: Collectibles & Currencies](#phase-6-collectibles--currencies)
- [Phase 7: Feedback Systems](#phase-7-feedback-systems)
- [Phase 8: User Interface](#phase-8-user-interface)
- [Phase 9: Persistence](#phase-9-persistence)
- [Phase 10: Polish & Performance](#phase-10-polish--performance)
- [Milestones](#milestones)

---

## Plan Overview

### Guiding Principles

The current codebase is **feature-sliced**, not layered. Two assemblies drive the project:
- `Assets/Scripts/Common`: shared primitives (events, collision contracts/router, config provider, state machine pattern, attributes, logging).
- `Assets/Scripts/Game`: gameplay features organized by domain (Installers, Flow, Level configs/catalogs/spawner, Entities/Knife, Entities/Target, KnifeThrower, Input, CollisionHandlers, UI, Settings).

Unity dependencies live where features need them (e.g., MonoBehaviours in feature folders, ScriptableObjects for configs/events). There are no separate Infrastructure/Application/Presentation layers; each feature owns its data, services, and views.

### Pattern Application

| Pattern | Status |
|---------|--------|
| ScriptableObject Configuration | ✅ Implemented |
| MonoBehaviour Wrapper | ✅ Implemented |
| State Pattern | ✅ Implemented |
| Strategy Pattern | ✅ Implemented (IRotationBehavior) |
| Observer Pattern (Event Channels) | ✅ Implemented |
| Command Pattern | Pending (Phase 5) |
| Object Pool Pattern | Pending (Phase 10) |
| Factory Pattern | ✅ Implemented (Phase 4) |
| Composite Pattern | ✅ Implemented (Phase 4) |

### Current Implementation Status

**Completed Systems (codebase reality):**
- VContainer composition root (`GameInstaller`) wiring config assets, event channels, factories, flow controllers, and UI components.
- Input → throw pipeline (`InputAdapter` → `ThrowService` → `KnifeThrowerView`) using `KnifeThrowConfiguration`.
- Knife behavior state machine on `KnifeView` (`Loaded`, `InFlight`, `Stuck`, `Bounced`, `Pooled`).
- Target rotation and sticking (`RotationBehaviorFactory` → `IRotationBehavior`/`ConstantRotationBehavior`, `TargetView.Stick`).
- Collision routing via event channel + router (`CollisionHandlerHandlerRouter`) with typed handlers (`KnifeHitTargetHandler`, `KnifeHitKnifeHandler`).
- Level/state flow (`LevelStateMachine` + `LevelFlowController`, `StageStateMachine` + `StageFlowController`, `GameStateMachine` + `GameFlowController`).
- Data-driven level content (ScriptableObject configs `StagesConfigAsset`/`StageConfig`/`LevelConfig`, `StagesConfigCatalog`, `TargetFactory`, `LevelSpawner`).
- UI fail flow (`LevelFailedEventChannel`, `StageFailPopup`, `UIController`, retry/restart `VoidEventChannel`s).

### Current Codebase Structure (phase 5 baseline)

- `Assets/Scripts/Common`: shared utilities (events, collision contracts/router base, config provider, state machine pattern, attributes, logging).
- `Assets/Scripts/Game`: gameplay features and orchestration:
  - `Installers`: `GameInstaller` composition root.
  - `Flow`: Game/Stage/Level data, state machines, controllers, and flow event channels.
  - `Level`: stage/level configs (`StagesConfigAsset`, `StageConfig`, `LevelConfig`, `TargetConfig`), catalog, and `LevelSpawner`.
  - `Entities/Knife`: knife states, data, views, interfaces, events.
  - `Entities/Target`: target config/data, rotation behavior factory, view.
  - `KnifeThrower`: configs, interfaces, services for throw pipeline.
  - `Input`: input adapter wrapping Unity Input System actions.
  - `CollisionHandlers`: collision handler registrations and per-pair handlers.
  - `UI`: `UIController`, popups, base popup.
  - `Settings`: `GameSettings`.

---

## Phase 0: Foundation ✅ COMPLETE

### Implemented Components

| Component | Location | Status |
|-----------|----------|--------|
| `GameInstaller` | `Assets/Scripts/Game/Installers/` | ✅ Complete |
| `IConfigurationProvider<T>` | `Assets/Scripts/Common/Config/` | ✅ Complete |
| `IEventChannel<T>` + `IEventChannel` (void) | `Assets/Scripts/Common/Events/` | ✅ Complete |
| `EventChannel<T>` + `VoidEventChannel` | `Assets/Scripts/Common/Events/` | ✅ Complete |
| `GameSettings` | `Assets/Scripts/Game/Settings/` | ✅ Complete |

### Configuration Assets Created

| Asset | Location |
|-------|----------|
| `KnifeThrowConfig.asset` | `Configurations/Gameplay/` |
| `StagesConfig.asset` | `Configurations/Gameplay/` |
| Event channel assets (Collision, KnifeImpact, LevelCompleted, LevelFailed, StageCompleted, ReturnToMenu, Retry/Restart void channels) | `Configurations/EventChannels/` |

---

## Phase 1: Knife Flight ✅ COMPLETE

### Implemented Components

| Area | Components | Status |
|------|------------|--------|
| Input & Throw | `InputAdapter` (`IPlayerThrowInputListener`), `ThrowService` (`IThrowService`), `KnifeThrowerView` (`IKnifeThrowerView`) | ✅ Complete |
| Knife State Machine | `KnifeStateMachine`, `KnifeStateBase`, `KnifeLoadedState`, `KnifeInFlightState`, `KnifeStuckState`, `KnifeBouncedState`, `KnifePooledState`, `IKnifeViewContext`, `IKnifeController`, `KnifeView` | ✅ Complete |
| Knife Data & Config | `KnifeThrowConfiguration`, `KnifeThrowConfigAsset`, `KnifeSpawnRequestData`, `KnifeImpactEventData`, `KnifeImpactType`, `KnifeImpactEventChannel` | ✅ Complete |

---

## Phase 2: Target & Sticking ✅ COMPLETE

### Implemented Components

| Area | Components | Status |
|------|------------|--------|
| Target Config & Data | `TargetConfig` (orbit, rotation, knives to break), `StickPositionData` | ✅ Complete |
| Rotation Strategy | `IRotationBehavior`, `ConstantRotationBehavior`, `RotationBehaviorFactory` | ✅ Complete |
| Target View | `TargetView` (`ITargetView`, `IHitable`), stick math and destroy on knife count | ✅ Complete |

**Note:** `TargetView.Stick()` handles local positioning/rotation of knives and increments hit tracking before destruction.

---

## Phase 3: Game States ✅ COMPLETE

### Implemented Components

| Area | Components | Status |
|------|------------|--------|
| Data | `LevelData` (knife counters, active targets, config), `StageData` (stage + level indices), `CollisionData`, `IHitable` | ✅ Complete |
| State Machines | `LevelStateMachine` (`LevelLoadingState`, `LevelPlayingState`, `LevelCompletedState`, `LevelFailedState`), `StageStateMachine` (`StageLoadingState`, `StagePlayingState`, `StageCompletedState`), `GameStateMachine` (`GamePlayingState`) | ✅ Complete |
| Flow Controllers | `LevelFlowController`, `StageFlowController`, `GameFlowController` | ✅ Complete |
| Events/Channels | `LevelCompletedEventChannel`, `LevelFailedEventChannel`, `StageCompletedEventChannel`, `ReturnToMenuEventChannel`, `CollisionEventChannel`, `KnifeImpactEventChannel` | ✅ Complete |
| Collision Routing | `CollisionHandlerHandlerRouter`, `CollisionHandlerRegistrar`, `KnifeHitTargetHandler`, `KnifeHitKnifeHandler` | ✅ Complete |

### Current Flow

```
Player tap → ThrowService → KnifeThrowerView.SpawnKnife()
    → KnifeView.Launch()

Knife in flight collides (KnifeView.OnTrigger)
    → CollisionEventChannel.Raise(CollisionData)
    → CollisionHandlerHandlerRouter routes
        → KnifeHitTargetHandler: Stop knife → TargetView.Stick → KnifeImpactEventChannel(Stuck)
        → KnifeHitKnifeHandler: Bounce knife → KnifeImpactEventChannel(Bounced)

LevelStateMachine
    LevelLoadingState: reset counters, set KnifeCount (sum of target KnivesToBreak + ExtraKnives), spawn targets via LevelSpawner
    LevelPlayingState: listen to KnifeImpact + target destruction; on stuck/bounce/knife depletion → Completed/Failed
    LevelCompletedState/LevelFailedState: raise LevelCompletedEventChannel/LevelFailedEventChannel

StageStateMachine
    StageLoadingState: set CurrentLevelIndex = 0, load level
    StagePlayingState: on LevelCompleted → advance level or StageCompleted; on Retry → LevelFlowController.RetryLevel(); on Restart → reload stage (level index reset)
    StageCompletedState: raise StageCompletedEventChannel

GameStateMachine (GamePlayingState)
    On enter: load StageIndex from GameData; subscribe to StageCompleted/ReturnToMenu
    On StageCompleted: advance stage when available, otherwise wrap to stage 0 and reload
```

---

## Phase 4: Level Configuration ✅ COMPLETE

### Goal

Data-driven level definitions via ScriptableObjects. Levels define target layouts, rotation behaviors, knife counts, and multiple targets.

### Implemented Components

| Area | Components | Status |
|------|------------|--------|
| Data | `StageConfig` (name, `StageLayoutSettings`, levels), `LevelConfig` (`KnifeCount`, `ExtraKnives`, targets), `TargetConfig` (prefab override, base scale, orbit radius/angle/speed, knives to break, rotation speed/direction), `StageLayoutSettings` | ✅ Complete |
| Interfaces | `ILevelCatalog`, `ILevelSpawner` | ✅ Complete |
| Scriptable Objects | `StagesConfigAsset` (default target prefab, stages, OnValidate computes `KnifeCount = sum(KnivesToBreak) + ExtraKnives`) | ✅ Complete |
| Runtime Catalog/Factories | `StagesConfigCatalog`, `TargetFactory`, `RotationBehaviorFactory`, `LevelSpawner` | ✅ Complete |

**Data Structure (current):**
```
StagesConfigAsset
└── List<StageConfig>
    ├── stageName: string
    ├── layoutSettings: StageLayoutSettings (background sprite, target sprite)
    └── List<LevelConfig>
        ├── knifeCount: int (auto-set in asset from targets + extraKnives)
        ├── extraKnives: int
        └── List<TargetConfig>
            ├── prefabOverride: GameObject
            ├── orbitRadius: float
            ├── startingAngle: float
            ├── orbitSpeed: float
            ├── baseScale: float
            ├── knivesToBreak: int
            ├── rotationSpeed: float
            └── rotationDirection: int (1 or -1)
```

---

## Phase 5: Flow & Progression ✅ COMPLETE

### Goal

Level completion advances to next level. Stage completion advances to next stage. Fail shows retry/restart options.

### Why This Order

Flow ties together level loading and session tracking from previous phases.

---

### Implemented Components

| Area | Components | Status |
|------|------------|--------|
| Progression Flow | `StagePlayingState` (advance level on `LevelCompleted`, respond to Retry/Restart `VoidEventChannel`s), `StageCompletedState` (raise `StageCompletedEventChannel`), `GamePlayingState` (advance stage or wrap to stage 0 on `StageCompleted`) | ✅ Complete |
| Retry/Restart UX | `StageFailPopup` (raises Retry/Restart `VoidEventChannel`s), `UIController` (shows popup on `LevelFailedEventChannel`, hides on retry/restart) | ✅ Complete |
| Event Channels | `LevelCompletedEventChannel`, `LevelFailedEventChannel`, `StageCompletedEventChannel`, `ReturnToMenuEventChannel`, Retry/Restart `VoidEventChannel`s (wired in `GameInstaller`) | ✅ Complete |

**Flow Logic (current code):**
```
On LevelCompleted (LevelStateMachine → StagePlayingState):
    if more levels in stage → CurrentLevelIndex++ and LoadLevel
    else → StageCompletedState raises StageCompletedEventChannel

On LevelFailed:
    UIController shows StageFailPopup

On Retry click:
    StagePlayingState → LevelFlowController.RetryLevel() (reload current LevelConfig)

On Restart click:
    StagePlayingState → SetState(Loading) (resets CurrentLevelIndex = 0 then loads first level)

On StageCompleted (GamePlayingState):
    if more stages → increment GameData.CurrentStageIndex and load via StageFlowController
    else → wrap to stage 0 and load
```

---

## Phase 6: Collectibles & Currencies

### Goal

Collectible objects (e.g., tomato currency) attach to targets. Hitting them awards collectible currency. This phase introduces tomato as the primary currency and requires creating the collectible assets and flow to implement the currency/collectibles system end to end.

### Why This Order

Collectibles add depth without changing core mechanics. Uses established patterns.

---

### Planned Components

#### Data & Interfaces

| Component | Location | Purpose |
|-----------|----------|---------|
| `CollectibleDefinition` | `Core/Data/Collectible/` | Config for collectible placement (config, count, spacing) |
| `CollectibleObjectConfig` | `Core/Data/Collectible/` | Prefab reference, particle effect |
| `CollectibleHitEventData` | `Core/Data/Collectible/` | Impact position payload |
| `CollectibleCurrencyDefinition` (tomato) | `Core/Data/Collectible/` | Defines the tomato currency collectible type, reward amount, visuals |
| `ICollectibleTracker` | `Core/Abstractions/` | Interface for tracking collectible currency (e.g., tomatoes) |

**Add to TargetConfig:**
```
collectibles: List<CollectibleDefinition>
collectibleSpacingOffset: float
```

#### Systems

| Component | Location | Purpose |
|-----------|----------|---------|
| `CollectibleSpawner` | `Infrastructure/Leveling/` | Positions collectibles (including tomato currency) on target perimeter |
| `CollectibleHitEventChannel` | `Infrastructure/Events/` | Event channel for collectible hits, including tomato currency pickups |

#### Services

| Component | Location | Purpose |
|-----------|----------|---------|
| `CollectibleService` | `App/Services/` | Tracks tomato currency (and future collectible currencies), listens to hit events, updates tracker |

#### Views & Collision

| Component | Location | Purpose |
|-----------|----------|---------|
| `CollectibleObjectView` | `Presentation/Gameplay/Collectibles/` | MonoBehaviour for a collectible currency object (e.g., tomato); implements `IHitable` |
| `KnifeHitCollectibleHandler` | `Presentation/Gameplay/Collision/` | Handles knife-collectible collision |

**Hit Behavior:**
1. Knife collision detected
2. Mark as hit (prevent double-processing)
3. Disable colliders
4. Spawn particle effect
5. Raise CollectibleHitEvent (include tomato currency payload)
6. Destroy GameObject

---

## Phase 7: Feedback Systems

### Goal

Audio and visual feedback for all game actions. Satisfying, responsive feel.

### Why This Order

Feedback transforms mechanics into experience. Uses event listeners from earlier phases.

---

### Planned Components

#### Data & Interfaces

| Component | Location | Purpose |
|-----------|----------|---------|
| `IAudioPlayer` | `Core/Abstractions/Audio/` | Interface for SFX playback |
| `IMusicPlayer` | `Core/Abstractions/Audio/` | Interface for BGM |
| `SoundSettings` | `Core/Data/Audio/` | ScriptableObject with all audio clips |

#### Systems

| Component | Location | Purpose |
|-----------|----------|---------|
| `AudioPlayer` | `Infrastructure/Audio/` | Implements IAudioPlayer |
| `MusicPlayer` | `Infrastructure/Audio/` | Implements IMusicPlayer |
| `ParticleSpawner` | `Infrastructure/VFX/` | Spawns particle prefabs at positions |

#### Services

| Component | Location | Purpose |
|-----------|----------|---------|
| `AudioFeedbackService` | `App/Services/Audio/` | Listens to events, plays sounds |
| `VFXFeedbackService` | `App/Services/VFX/` | Listens to events, spawns particles |

**Event → Sound Mapping:**
| Event | Sound |
|-------|-------|
| Knife thrown | knife_throw.wav |
| KnifeImpact (Stuck) | hit_target_sound.wav |
| KnifeImpact (Bounced) | knife_hit_knife_ogg.ogg |
| CollectibleHit | hitable_fruit_sound.wav |
| LevelCompleted | win_sound.wav |
| LevelFailed | lose_sound.wav |

#### Views & Animations

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetHitFeedback` | `Presentation/Gameplay/Target/` | DOTween shake + tint on hit |
| `KnifeLoadAnimator` | `Presentation/Gameplay/Knife/` | Slide-up + fade animation |

#### Assets

| Asset | Location | Purpose |
|-------|----------|---------|
| `SoundSettings.asset` | `Configurations/Audio/` | All audio clip references |
| `TargetHitAnimationConfig.asset` | `Configurations/Animations/` | Shake/tint parameters |
| `KnifeBounceAnimationConfig.asset` | `Configurations/Animations/` | Bounce parameters |

---

## Phase 8: User Interface

### Goal

Complete HUD showing game state. Proper fail popup with navigation.

### Why This Order

UI visualizes all systems built so far.

---

### Planned Components

#### Data & Events

| Component | Location | Purpose |
|-----------|----------|---------|
| `ILevelHudData` | `Core/Abstractions/UI/` | Interface exposing HUD data |
| `ButtonClickEventData` | `Core/Data/UI/` | UI sound feedback payload |

#### Services & ViewModels

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelHudViewModel` | `App/Services/UI/` | Provides data to HUD views |
| `SettingsService` | `App/Services/` | Manages mute state |

**ViewModel Properties:**
```
CurrentStage: int
CurrentLevel: int
TotalLevelsInStage: int
KnivesRemaining: int
TotalCollectibles: int
IsMuted: bool
```

#### UI Views

| Component | Location | Purpose |
|-----------|----------|---------|
| `StageProgressBarView` | `Presentation/UI/HUD/` | Level progress dots |
| `KnifeMagazineDisplay` | `Presentation/UI/HUD/` | Remaining knife icons |
| `CollectibleCountDisplay` | `Presentation/UI/HUD/` | Total collectible currency display |
| `StageLabelView` | `Presentation/UI/HUD/` | "STAGE 1" text |
| `MuteButtonView` | `Presentation/UI/HUD/` | Toggle audio |
| `StageFailPopupView` | `Presentation/UI/Popups/` | Retry/Restart buttons |
| `BasePopup` | `Presentation/UI/Common/` | Base class for popups |

**UI Raycast Blocking:**
- Before throw, check if pointer over UI
- If UI element under pointer, suppress throw

---

## Phase 9: Persistence

### Goal

Progress saves between sessions. Settings persist.

### Why This Order

Persistence makes progression meaningful.

---

### Planned Components

#### Data & Interfaces

| Component | Location | Purpose |
|-----------|----------|---------|
| `PlayerData` | `Core/Data/Persistence/` | Root save data structure |
| `PlayerProgressData` | `Core/Data/Persistence/` | Stage/level progress, fruits |
| `PlayerSettingsData` | `Core/Data/Persistence/` | Audio, vibration settings |
| `IPlayerDataStore` | `Core/Abstractions/Persistence/` | Interface for save/load |

**Data Structure:**
```
PlayerData
├── progress: PlayerProgressData
│   ├── currentStageIndex: int
│   ├── highestStageIndex: int
│   └── totalCollectiblesCollected: int
└── settings: PlayerSettingsData
    └── isMuted: bool
```

#### Systems

| Component | Location | Purpose |
|-----------|----------|---------|
| `FilePlayerDataStore` | `Infrastructure/Persistence/` | JSON file read/write |

**File Operations:**
- Path: `Application.persistentDataPath/playerData.json`
- Atomic write: temp file then rename
- Load with fallback to defaults

#### Services

| Component | Location | Purpose |
|-----------|----------|---------|
| `PlayerDataService` | `App/Services/Persistence/` | Manages data lifecycle |

**Save Triggers:**
- Level completed
- Stage completed
- Settings changed
- Collectible collected (debounced)

---

## Phase 10: Polish & Performance

### Goal

Object pooling, additional rotation behaviors, pre-stuck knives, multi-target levels fully polished.

### Why This Phase Last

Optimization and variety build on stable foundation.

---

### Planned Components

#### Data & Behaviors

| Component | Location | Purpose |
|-----------|----------|---------|
| `IPoolable` | `Core/Abstractions/Pooling/` | Interface for pooled objects |
| `PulsatingRotationBehavior` | `Core/Domain/Target/` | Speed oscillates sinusoidally |
| `FlippingRotationBehavior` | `Core/Domain/Target/` | Direction randomly reverses |
| `CurveRotationBehavior` | `Core/Domain/Target/` | Speed follows AnimationCurve |

**Additional Rotation Strategies:**
```
IRotationBehavior
├── ConstantRotationBehavior ✅ (existing)
├── PulsatingRotationBehavior (speed = base + sin(t) * amplitude)
├── FlippingRotationBehavior (random direction changes)
└── CurveRotationBehavior (speed follows curve)
```

#### Systems

| Component | Location | Purpose |
|-----------|----------|---------|
| `ObjectPool<T>` | `Infrastructure/Pooling/` | Generic pool implementation |
| `KnifePool` | `Infrastructure/Pooling/` | Knife-specific pool |
| `ParticlePool` | `Infrastructure/Pooling/` | Particle effect pool |

**Pool Operations:**
- `Warmup(count)`: Pre-instantiate
- `Get()`: Retrieve or create
- `Return(obj)`: Deactivate, re-pool
- Reset state on return

#### Services

| Component | Location | Purpose |
|-----------|----------|---------|
| `ThrowService` | | Use pool instead of Instantiate |
| `VFXFeedbackService` | | Use particle pool |

**Pre-Stuck Knives:**
- Spawn knives already stuck at level start
- Configure via PreStuckKnifeConfig on TargetConfig
- These are obstacles, not throwable

---

## Milestones

### Milestone Summary

| Milestone | After Phase | Playable State |
|-----------|-------------|----------------|
| **PROTOTYPE** | Phase 3 | ✅ Throw, stick, win, fail — core loop works |
| **ALPHA** | Phase 6 | Levels, progression, collectibles |
| **BETA** | Phase 8 | Complete game with UI |
| **RELEASE CANDIDATE** | Phase 10 | Polished, optimized, persistent |

---

### Definition of Done (Per Phase)

A phase is **COMPLETE** when:

1. ✅ All planned components implemented in appropriate feature folders
2. ✅ Patterns applied as specified
3. ✅ No compiler errors or warnings
4. ✅ No runtime exceptions
5. ✅ All test criteria pass in Unity Play Mode
6. ✅ Previous phases still function (regression check)
7. ✅ Code follows project conventions
8. ✅ No violations of implementation rules

---

### Quick Reference: Folder Placement

| If creating... | Place in... |
|----------------|-------------|
| Shared utility, event, state machine, config provider | `Assets/Scripts/Common/` |
| Game flow/data/state machine/controller | `Assets/Scripts/Game/Flow/` |
| Level config/catalog/spawner | `Assets/Scripts/Game/Level/` |
| Knife feature (states, views, data) | `Assets/Scripts/Game/Entities/Knife/` |
| Target feature (configs, factory, rotation behavior, view) | `Assets/Scripts/Game/Entities/Target/` |
| Input adapter or throw pipeline pieces | `Assets/Scripts/Game/Input/` or `Assets/Scripts/Game/KnifeThrower/` |
| Collision handlers and registration | `Assets/Scripts/Game/CollisionHandlers/` |
| UI components/popups/controllers | `Assets/Scripts/Game/UI/` |
| Installer and DI wiring | `Assets/Scripts/Game/Installers/` |
| ScriptableObject config/event assets | `Configurations/Gameplay/` or `Configurations/EventChannels/` |

---

### Existing Event Channels

| Channel | Data Type | Location |
|---------|-----------|----------|
| `CollisionEventChannel` | `CollisionData` | `Configurations/EventChannels/` |
| `KnifeImpactEventChannel` | `KnifeImpactEventData` | `Configurations/EventChannels/` |
| `LevelCompletedEventChannel` | `LevelData` | `Configurations/EventChannels/` |
| `LevelFailedEventChannel` | `LevelData` | `Configurations/EventChannels/` |
| `StageCompletedEventChannel` | `StageData` | `Configurations/EventChannels/` |
| `ReturnToMenuEventChannel` | `StageData` | `Configurations/EventChannels/` |
| Retry/Restart `VoidEventChannel`s | `Void` | `Configurations/EventChannels/` |

---

**END OF DOCUMENT**
