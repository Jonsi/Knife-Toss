# KNIFE TOSS — Development Plan

**Version 2.1**  
**Last Updated: December 2024**  
**Based on:** GameDesignDocument.md

---

## Table of Contents

1. [Plan Overview](#1-plan-overview)
2. [Phase 0: Foundation](#2-phase-0-foundation) ✅ COMPLETE
3. [Phase 1: Knife Flight](#3-phase-1-knife-flight) ✅ COMPLETE
4. [Phase 2: Target & Sticking](#4-phase-2-target--sticking) ✅ COMPLETE
5. [Phase 3: Game States](#5-phase-3-game-states) ✅ COMPLETE
6. [Phase 4: Level Configuration](#6-phase-4-level-configuration)
7. [Phase 5: Flow & Progression](#7-phase-5-flow--progression)
8. [Phase 6: Collectibles](#8-phase-6-collectibles)
9. [Phase 7: Feedback Systems](#9-phase-7-feedback-systems)
10. [Phase 8: User Interface](#10-phase-8-user-interface)
11. [Phase 9: Persistence](#11-phase-9-persistence)
12. [Phase 10: Polish & Performance](#12-phase-10-polish--performance)
13. [Milestones](#13-milestones)

---

## 1. Plan Overview

### 1.1 Guiding Principles

This plan follows the **Layered Architecture** already established in the codebase:

```
┌─────────────────────────────────────────────────────────────┐
│  PRESENTATION   │  MonoBehaviours, Views, Input Handlers   │
├─────────────────────────────────────────────────────────────┤
│  APPLICATION    │  Services, Coordinators, Use Cases        │
├─────────────────────────────────────────────────────────────┤
│  CORE           │  Interfaces, Domain Models, Events        │
├─────────────────────────────────────────────────────────────┤
│  INFRASTRUCTURE │  Implementations, Spawners, Persistence   │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Layer Notation

| Symbol | Layer | Unity Dependencies |
|--------|-------|-------------------|
| 🟦 | Core | ✅ Yes |
| 🟩 | Infrastructure | ✅ Yes |
| 🟨 | Application | ✅ Yes |
| 🟧 | Presentation | ✅ Yes |

### 1.3 Pattern Application

| Pattern | Status |
|---------|--------|
| ScriptableObject Configuration | ✅ Implemented |
| MonoBehaviour Wrapper | ✅ Implemented |
| State Pattern | ✅ Implemented |
| Strategy Pattern | ✅ Implemented (IRotationBehavior) |
| Observer Pattern (Event Channels) | ✅ Implemented |
| Command Pattern | Pending (Phase 5) |
| Object Pool Pattern | Pending (Phase 10) |
| Factory Pattern | Pending (Phase 4) |
| Composite Pattern | Pending (Phase 4) |

### 1.4 Current Implementation Status

**Completed Systems:**
- VContainer dependency injection (GameInstaller)
- Input system (InputAdapter, ThrowService)
- Knife state machine (Loaded, InFlight, Stuck, Bounced, Pooled)
- Target rotation (IRotationBehavior, ConstantRotationBehavior)
- Collision routing (CollisionRouter, CollisionProxy, typed handlers)
- Level session tracking (LevelSession, LevelSessionService)
- Event channels (ScriptableObject-based decoupled events)
- Game state machine (Loading, Playing, Completed, Failed)

---

## 2. Phase 0: Foundation ✅ COMPLETE

### Implemented Components

| Component | Location | Status |
|-----------|----------|--------|
| `GameInstaller` | `Bootstrapper/` | ✅ Complete |
| `IConfigurationProvider<T>` | `Core/Abstractions/Config/` | ✅ Complete |
| `IEventChannel<T>` | `Core/Abstractions/Events/` | ✅ Complete |
| `EventChannel<T>` | `Infrastructure/Events/` | ✅ Complete |
| `GameSettings` | `Infrastructure/Settings/` | ✅ Complete |

### Configuration Assets Created

| Asset | Location |
|-------|----------|
| `KnifeThrowConfig.asset` | `Configurations/Gameplay/` |
| `LevelRulesConfig.asset` | `Configurations/Gameplay/` |
| `TargetConfig.asset` | `Configurations/Gameplay/` |
| Event channel assets | `Configurations/EventChannels/` |

---

## 3. Phase 1: Knife Flight ✅ COMPLETE

### Implemented Components

#### Core Layer 🟦

| Component | Location | Status |
|-----------|----------|--------|
| `IState` | `Core/Abstractions/State/` | ✅ Complete |
| `IKnifeViewContext` | `Core/Abstractions/Gameplay/Knife/` | ✅ Complete |
| `IPlayerThrowInputListener` | `Core/Abstractions/Input/` | ✅ Complete |
| `IThrowService` | `Core/Abstractions/Throwing/` | ✅ Complete |
| `StateMachine` | `Core/Domain/` | ✅ Complete |
| `KnifeStateMachine` | `Core/Domain/Knife/` | ✅ Complete |
| `KnifeStateBase` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifeLoadedState` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifeInFlightState` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifeStuckState` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifeBouncedState` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifePooledState` | `Core/Domain/Knife/States/` | ✅ Complete |
| `KnifeThrowConfiguration` | `Core/Data/Throwing/` | ✅ Complete |
| `KnifeSpawnRequestData` | `Core/Data/Knife/` | ✅ Complete |

#### Infrastructure Layer 🟩

| Component | Location | Status |
|-----------|----------|--------|
| `InputAdapter` | `Infrastructure/Input/` | ✅ Complete |
| `KnifeThrowConfigAsset` | `Infrastructure/Leveling/` | ✅ Complete |
| `KnifeSpawnRequestEventChannel` | `Infrastructure/Events/` | ✅ Complete |

#### Application Layer 🟨

| Component | Location | Status |
|-----------|----------|--------|
| `ThrowService` | `App/Services/Throwing/` | ✅ Complete |

#### Presentation Layer 🟧

| Component | Location | Status |
|-----------|----------|--------|
| `KnifeView` | `Presentation/Gameplay/Knife/` | ✅ Complete |
| `KnifeThrowerView` | `Presentation/Gameplay/Knife/` | ✅ Complete |

---

## 4. Phase 2: Target & Sticking ✅ COMPLETE

### Implemented Components

#### Core Layer 🟦

| Component | Location | Status |
|-----------|----------|--------|
| `IRotationBehavior` | `Core/Abstractions/Gameplay/Target/` | ✅ Complete |
| `ITargetView` | `Core/Abstractions/Gameplay/Target/` | ✅ Complete |
| `ConstantRotationBehavior` | `Core/Domain/Target/` | ✅ Complete |
| `TargetConfiguration` | `Core/Data/Target/` | ✅ Complete |
| `StickPositionData` | `Core/Data/Target/` | ✅ Complete |

#### Infrastructure Layer 🟩

| Component | Location | Status |
|-----------|----------|--------|
| `TargetConfigAsset` | `Infrastructure/Leveling/` | ✅ Complete |

#### Presentation Layer 🟧

| Component | Location | Status |
|-----------|----------|--------|
| `TargetView` | `Presentation/Gameplay/Target/` | ✅ Complete |

**Note:** Sticking logic is handled directly in `TargetView.Stick()` method, which calculates local position and rotation based on impact point.

---

## 5. Phase 3: Game States ✅ COMPLETE

### Implemented Components

#### Core Layer 🟦

| Component | Location | Status |
|-----------|----------|--------|
| `GameStateMachine` | `Core/Domain/` | ✅ Complete |
| `LevelStateBase` | `Core/Domain/Level/` | ✅ Complete |
| `LevelLoadingState` | `Core/Domain/Level/` | ✅ Complete |
| `LevelPlayingState` | `Core/Domain/Level/` | ✅ Complete |
| `LevelCompletedState` | `Core/Domain/Level/` | ✅ Complete |
| `LevelFailedState` | `Core/Domain/Level/` | ✅ Complete |
| `LevelSession` | `Core/Domain/Level/` | ✅ Complete |
| `LevelRules` | `Core/Data/Level/` | ✅ Complete |
| `KnifeImpactType` | `Core/Data/Knife/` | ✅ Complete |
| `KnifeImpactEventData` | `Core/Data/Knife/` | ✅ Complete |
| `LevelStateChangedEventData` | `Core/Data/Level/` | ✅ Complete |
| `ILevelSessionService` | `Core/Abstractions/Gameplay/Session/` | ✅ Complete |
| `ICollisionHandler` | `Core/Abstractions/Collision/` | ✅ Complete |
| `ICollisionRouter` | `Core/Abstractions/Collision/` | ✅ Complete |
| `IHitable` | `Core/Abstractions/Collision/` | ✅ Complete |
| `CollisionHandler<THit, THitBy>` | `Core/Domain/Collision/` | ✅ Complete |
| `CollisionData` | `Core/Data/Collision/` | ✅ Complete |

#### Infrastructure Layer 🟩

| Component | Location | Status |
|-----------|----------|--------|
| `LevelRulesConfigAsset` | `Infrastructure/Leveling/` | ✅ Complete |
| `KnifeImpactEventChannel` | `Infrastructure/Events/` | ✅ Complete |
| `LevelStateChangedEventChannel` | `Infrastructure/Events/` | ✅ Complete |
| `CollisionEventChannel` | `Infrastructure/Events/` | ✅ Complete |

#### Application Layer 🟨

| Component | Location | Status |
|-----------|----------|--------|
| `LevelSessionService` | `App/Services/Session/` | ✅ Complete |
| `LevelSessionLogger` | `App/Services/Session/` | ✅ Complete |
| `GameFlowController` | `App/Services/Flow/` | ✅ Complete |
| `CollisionRouter` | `App/Services/Collision/` | ✅ Complete |
| `CollisionProxy` | `App/Services/Collision/` | ✅ Complete |

#### Presentation Layer 🟧

| Component | Location | Status |
|-----------|----------|--------|
| `KnifeHitTargetHandler` | `Presentation/Gameplay/Collision/` | ✅ Complete |
| `KnifeHitKnifeHandler` | `Presentation/Gameplay/Collision/` | ✅ Complete |
| `CollisionHandlerRegistrar` | `Presentation/Gameplay/Collision/` | ✅ Complete |

### Current Flow

```
Player tap → ThrowService.HandleThrowRequested()
    → LevelSessionService.TryRegisterThrow()
    → KnifeSpawnRequestEventChannel.Raise()
    → KnifeThrowerView.SpawnKnife()
    → KnifeView.Launch()

Collision detected → CollisionEventChannel.Raise()
    → CollisionProxy → CollisionRouter.Route()
    → KnifeHitTargetHandler.HandleCollision()
        → KnifeView.StopMovement()
        → TargetView.Stick()
        → LevelSessionService.RegisterStick()
            → KnifeImpactEventChannel.Raise()
            → LevelStateChangedEventChannel.Raise() (if state changed)
```

---

## 6. Phase 4: Level Configuration

### Goal

Data-driven level definitions via ScriptableObjects. Levels define target layouts, rotation behaviors, knife counts, and pre-stuck knives.

### Why This Order

Foundation phases are complete. Level configuration enables content creation without code changes and establishes the data structures for progression.

---

### Deliverables by Layer

#### 6.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelData` | `Core/Data/Level/` | Single level definition (target group, knife count override) |
| `StageData` | `Core/Data/Level/` | Collection of levels plus visual theme reference |
| `TargetDefinition` | `Core/Data/Target/` | Per-target config (position, scale, rotation type/speed, knivesToBreak, pre-stuck knives, hitables) |
| `TargetGroupConfig` | `Core/Data/Target/` | Composite list of TargetDefinition |
| `RotationConfig` | `Core/Data/Target/` | Rotation behavior parameters (type, speed, amplitude, frequency, etc.) |
| `PreStuckKnifeConfig` | `Core/Data/Knife/` | Count, spacing, offset for pre-placed knives |
| `ILevelCatalog` | `Core/Abstractions/Gameplay/` | Interface for stage/level retrieval |

**Data Structure:**
```
StageData
├── stageName: string
├── layoutSettings: StageLayoutSettings (background, target sprite)
└── levels: List<LevelData>

LevelData
├── knives: int (0 = auto-calculate from targets)
└── targetGroup: TargetGroupConfig

TargetGroupConfig
└── targets: List<TargetDefinition>

TargetDefinition
├── prefab: GameObject (optional override)
├── orbitRadius: float
├── startingAngle: float
├── orbitDegreesPerSecond: float
├── baseScale: float
├── knivesToBreak: int
├── rotation: RotationConfig
├── preStuckKnives: PreStuckKnifeConfig
└── hitableObjects: List<HitableDefinition>
```

#### 6.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `StagesConfigAsset` | `Infrastructure/Leveling/` | ScriptableObject holding all StageData |
| `StagesConfigCatalog` | `Infrastructure/Leveling/` | Implements ILevelCatalog, reads StagesConfigAsset |
| `TargetFactory` | `Infrastructure/Leveling/` | Creates targets from TargetDefinition, assigns rotation behavior |
| `LevelSpawner` | `Infrastructure/Leveling/` | Clears level, spawns targets under level root |
| `RotationBehaviorFactory` | `Infrastructure/Leveling/` | Creates IRotationBehavior from RotationConfig |
| `TargetGeometry` | `Infrastructure/Leveling/` | Utility for surface point calculations |

#### 6.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `RuntimeLevelRulesProvider` | `App/Services/Session/` | Mutable IConfigurationProvider<LevelRules> set per level |
| `LevelSetupService` | `App/Services/` | Orchestrates level load: catalog → spawner → session start |
| `TargetTrackingService` | `App/Services/` | Tracks multiple targets, reports all destroyed → level complete |

**Level Load Sequence:**
1. Resolve LevelData from catalog (stage index, level index)
2. Derive LevelRules from target group (knife count, total knivesToBreak)
3. Set RuntimeLevelRulesProvider
4. Clear existing level via LevelSpawner
5. Spawn targets via TargetFactory
6. Spawn pre-stuck knives
7. Start LevelSession

#### 6.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| Update `TargetView` | | Support per-target knivesToBreak threshold, destruction callback |
| `TargetHitTracker` | `Presentation/Gameplay/Target/` | Tracks hits per target, raises destroyed event |

#### 6.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `StagesConfig.asset` | `Configurations/Gameplay/` | Main content definition |
| `StageLayoutSettings_*.asset` | `Configurations/StagesLayout/` | Per-stage visuals |

---

### Test Criteria

- [ ] StagesConfig.asset defines multiple stages with multiple levels
- [ ] Loading level 0,0 spawns correct target layout
- [ ] Switching levels changes rotation speed and target count
- [ ] Knife count derives from target group if not explicitly set
- [ ] Multiple targets spawn and track independently
- [ ] Destroying all targets completes level
- [ ] Pre-stuck knives appear at level start

---

## 7. Phase 5: Flow & Progression

### Goal

Level completion advances to next level. Stage completion advances to next stage. Fail shows retry/restart options.

### Why This Order

Flow ties together level loading and session tracking from previous phases.

---

### Deliverables by Layer

#### 7.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `ILevelCommand` | `Core/Abstractions/` | Command interface with Execute() |
| `RetryLevelCommand` | `Core/Domain/Level/` | Reload current level |
| `NextLevelCommand` | `Core/Domain/Level/` | Advance to next level |
| `RestartStageCommand` | `Core/Domain/Level/` | Reset to stage level 1 |
| `LevelCommandEventData` | `Core/Data/Level/` | Payload with command type |

#### 7.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelCommandEventChannel` | `Infrastructure/Events/` | Command event channel |

#### 7.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelFlowCoordinator` | `App/Services/Flow/` | Listens to level events, executes flow |
| `StageProgressTracker` | `App/Services/Flow/` | Tracks current stage/level indices |

**Flow Coordinator Logic:**
```
On LevelCompleted:
├── If more levels in stage → Load next level
├── If last level in stage → Advance stage, load stage's level 0
└── If last stage → Show completion (future)

On LevelFailed:
└── Show fail popup (player chooses Retry or RestartStage)
```

#### 7.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `StageFailPopup` | `Presentation/UI/Popups/` | Retry/RestartStage buttons |

---

### Test Criteria

- [ ] Complete level 1 → Auto-loads level 2
- [ ] Complete all levels in stage → Loads next stage level 1
- [ ] Fail level → Popup appears
- [ ] Click Retry → Same level reloads
- [ ] Click Restart Stage → Stage level 1 loads
- [ ] Game pauses on fail (Time.timeScale = 0)
- [ ] Game resumes on retry/restart

---

## 8. Phase 6: Collectibles

### Goal

Hitable objects (tomatoes) attach to targets. Hitting them awards fruit currency.

### Why This Order

Collectibles add depth without changing core mechanics. Uses established patterns.

---

### Deliverables by Layer

#### 8.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableDefinition` | `Core/Data/Hitable/` | Config for hitable placement (config, count, spacing) |
| `HitableObjectConfig` | `Core/Data/Hitable/` | Prefab reference, particle effect |
| `HitableHitEventData` | `Core/Data/Hitable/` | Impact position payload |
| `IFruitTracker` | `Core/Abstractions/` | Interface for fruit counting |

**Add to TargetDefinition:**
```
hitableObjects: List<HitableDefinition>
hitableObjectSpacingOffset: float
```

#### 8.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableSpawner` | `Infrastructure/Leveling/` | Positions hitables on target perimeter |
| `HitableHitEventChannel` | `Infrastructure/Events/` | Event channel for hitable hits |

#### 8.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `FruitService` | `App/Services/` | Tracks fruit count, listens to hit events |

#### 8.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableView` | `Presentation/Gameplay/Hitables/` | MonoBehaviour for tomato |
| `KnifeHitHitableHandler` | `Presentation/Gameplay/Collision/` | Handles knife-hitable collision |

**Hit Behavior:**
1. Knife collision detected
2. Mark as hit (prevent double-processing)
3. Disable colliders
4. Spawn particle effect
5. Raise HitableHitEvent
6. Destroy GameObject

---

### Test Criteria

- [ ] Tomatoes spawn at configured angles on target
- [ ] Tomatoes rotate with target (parented)
- [ ] Knife hits tomato → Tomato destroyed
- [ ] HitableHitEvent raised
- [ ] FruitService count increments
- [ ] Knife continues to target after hitting tomato
- [ ] Tomato hit does NOT count toward target threshold

---

## 9. Phase 7: Feedback Systems

### Goal

Audio and visual feedback for all game actions. Satisfying, responsive feel.

### Why This Order

Feedback transforms mechanics into experience. Uses event listeners from earlier phases.

---

### Deliverables by Layer

#### 9.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `IAudioPlayer` | `Core/Abstractions/Audio/` | Interface for SFX playback |
| `IMusicPlayer` | `Core/Abstractions/Audio/` | Interface for BGM |
| `SoundSettings` | `Core/Data/Audio/` | ScriptableObject with all audio clips |

#### 9.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `AudioPlayer` | `Infrastructure/Audio/` | Implements IAudioPlayer |
| `MusicPlayer` | `Infrastructure/Audio/` | Implements IMusicPlayer |
| `ParticleSpawner` | `Infrastructure/VFX/` | Spawns particle prefabs at positions |

#### 9.3 Application Layer 🟨

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
| HitableHit | hitable_fruit_sound.wav |
| LevelCompleted | win_sound.wav |
| LevelFailed | lose_sound.wav |

#### 9.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetHitFeedback` | `Presentation/Gameplay/Target/` | DOTween shake + tint on hit |
| `KnifeLoadAnimator` | `Presentation/Gameplay/Knife/` | Slide-up + fade animation |

#### 9.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `SoundSettings.asset` | `Configurations/Audio/` | All audio clip references |
| `TargetHitAnimationConfig.asset` | `Configurations/Animations/` | Shake/tint parameters |
| `KnifeBounceAnimationConfig.asset` | `Configurations/Animations/` | Bounce parameters |

---

### Test Criteria

- [ ] Throw knife → Hear throw sound
- [ ] Knife sticks → Hear wood thunk + see splinters
- [ ] Target shakes and flashes on hit
- [ ] Knife hits knife → Hear metal clang
- [ ] Hit tomato → Hear splat + see juice particles
- [ ] Background music plays and loops
- [ ] All parameters configurable via ScriptableObjects

---

## 10. Phase 8: User Interface

### Goal

Complete HUD showing game state. Proper fail popup with navigation.

### Why This Order

UI visualizes all systems built so far.

---

### Deliverables by Layer

#### 10.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `ILevelHudData` | `Core/Abstractions/UI/` | Interface exposing HUD data |
| `ButtonClickEventData` | `Core/Data/UI/` | UI sound feedback payload |

#### 10.2 Application Layer 🟨

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
TotalFruits: int
IsMuted: bool
```

#### 10.3 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `StageProgressBarView` | `Presentation/UI/HUD/` | Level progress dots |
| `KnifeMagazineDisplay` | `Presentation/UI/HUD/` | Remaining knife icons |
| `FruitCountDisplay` | `Presentation/UI/HUD/` | Total fruits display |
| `StageLabelView` | `Presentation/UI/HUD/` | "STAGE 1" text |
| `MuteButtonView` | `Presentation/UI/HUD/` | Toggle audio |
| `StageFailPopupView` | `Presentation/UI/Popups/` | Retry/Restart buttons |
| `BasePopup` | `Presentation/UI/Common/` | Base class for popups |

**UI Raycast Blocking:**
- Before throw, check if pointer over UI
- If UI element under pointer, suppress throw

---

### Test Criteria

- [ ] HUD shows "STAGE X" correctly
- [ ] Progress bar shows correct number of dots
- [ ] Current level dot is highlighted
- [ ] Knife magazine shows correct count
- [ ] Each throw reduces visible knife icons
- [ ] Fruit counter shows total, updates on collect
- [ ] Mute button toggles audio on/off
- [ ] Fail popup has working Retry button
- [ ] Fail popup has working Restart Stage button
- [ ] Tapping UI buttons doesn't throw knife

---

## 11. Phase 9: Persistence

### Goal

Progress saves between sessions. Settings persist.

### Why This Order

Persistence makes progression meaningful.

---

### Deliverables by Layer

#### 11.1 Core Layer 🟦

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
│   └── totalFruitsCollected: int
└── settings: PlayerSettingsData
    └── isMuted: bool
```

#### 11.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `FilePlayerDataStore` | `Infrastructure/Persistence/` | JSON file read/write |

**File Operations:**
- Path: `Application.persistentDataPath/playerData.json`
- Atomic write: temp file then rename
- Load with fallback to defaults

#### 11.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `PlayerDataService` | `App/Services/Persistence/` | Manages data lifecycle |

**Save Triggers:**
- Level completed
- Stage completed
- Settings changed
- Fruit collected (debounced)

---

### Test Criteria

- [ ] Complete a level → Progress saved
- [ ] Close and reopen game → Resumes at correct level
- [ ] Toggle mute → Setting persists after restart
- [ ] Collect fruits → Total persists after restart
- [ ] Delete save file → Game starts fresh
- [ ] Corrupt save file → Game handles gracefully

---

## 12. Phase 10: Polish & Performance

### Goal

Object pooling, additional rotation behaviors, pre-stuck knives, multi-target levels fully polished.

### Why This Phase Last

Optimization and variety build on stable foundation.

---

### Deliverables by Layer

#### 12.1 Core Layer 🟦

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

#### 12.2 Infrastructure Layer 🟩

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

#### 12.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| Update `ThrowService` | | Use pool instead of Instantiate |
| Update `VFXFeedbackService` | | Use particle pool |

**Pre-Stuck Knives:**
- Spawn knives already stuck at level start
- Configure via PreStuckKnifeConfig on TargetDefinition
- These are obstacles, not throwable

---

### Test Criteria

#### Pooling
- [ ] Rapid throwing shows no frame drops
- [ ] Profiler shows minimal GC during gameplay
- [ ] Knives correctly reset when returned to pool

#### Rotation Behaviors
- [ ] Level with Pulsating → Speed visibly oscillates
- [ ] Level with Flipping → Direction randomly reverses
- [ ] Level with Curve → Speed follows pattern

#### Pre-Stuck Knives
- [ ] Level with pre-stuck knives → They appear at start
- [ ] They rotate with target
- [ ] Hitting them causes fail

#### Multi-Target
- [ ] Level with 2 targets → Both appear
- [ ] Destroy one → Other remains
- [ ] Destroy all → Level complete

---

## 13. Milestones

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

1. ✅ All layer components implemented in correct locations
2. ✅ Patterns applied as specified
3. ✅ No compiler errors or warnings
4. ✅ No runtime exceptions
5. ✅ All test criteria pass in Unity Play Mode
6. ✅ Previous phases still function (regression check)
7. ✅ Code follows project conventions
8. ✅ No violations of implementation rules

---

### Quick Reference: Layer Placement

| If creating... | Place in... |
|----------------|-------------|
| Interface | 🟦 Core/Abstractions/ |
| Data struct/class | 🟦 Core/Data/ |
| Pure C# logic | 🟦 Core/Domain/ |
| ScriptableObject event channel | 🟩 Infrastructure/Events/ |
| ScriptableObject config asset | 🟩 Infrastructure/Leveling/ |
| Unity system wrapper | 🟩 Infrastructure/ |
| Spawner/Factory | 🟩 Infrastructure/Leveling/ |
| File I/O | 🟩 Infrastructure/Persistence/ |
| Service orchestrating features | 🟨 App/Services/ |
| VContainer installer | Bootstrapper/ |
| MonoBehaviour | 🟧 Presentation/ |
| UI component | 🟧 Presentation/UI/ |

---

### Existing Event Channels

| Channel | Data Type | Location |
|---------|-----------|----------|
| `KnifeSpawnRequestEventChannel` | `KnifeSpawnRequestData` | `Configurations/EventChannels/` |
| `CollisionEventChannel` | `CollisionData` | `Configurations/EventChannels/` |
| `KnifeImpactEventChannel` | `KnifeImpactEventData` | `Configurations/EventChannels/` |
| `LevelStateChangedEventChannel` | `LevelStateChangedEventData` | `Configurations/EventChannels/` |

---

**END OF DOCUMENT**
