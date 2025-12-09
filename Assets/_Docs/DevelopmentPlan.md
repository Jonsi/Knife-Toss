# KNIFE TOSS — Development Plan

**Version 2.0**  
**Last Updated: December 2024**  
**Based on:** Architecture.md, GameDesignDocument.md

---

## Table of Contents

1. [Plan Overview](#1-plan-overview)
2. [Phase 0: Foundation](#2-phase-0-foundation)
3. [Phase 1: Knife Flight](#3-phase-1-knife-flight)
4. [Phase 2: Target & Sticking](#4-phase-2-target--sticking)
5. [Phase 3: Game States](#5-phase-3-game-states)
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

This plan follows the **Layered Architecture** defined in Architecture.md:

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
| 🟦 | Core | ❌ None — Pure C# |
| 🟩 | Infrastructure | ✅ Yes |
| 🟨 | Application | ✅ Yes |
| 🟧 | Presentation | ✅ Yes |

### 1.3 Pattern Application

Each phase identifies which **Design Patterns** from Architecture.md are introduced:

| Pattern | First Introduced |
|---------|------------------|
| ScriptableObject Configuration | Phase 0 |
| MonoBehaviour Wrapper | Phase 1 |
| State Pattern | Phase 1 |
| Strategy Pattern | Phase 2 |
| Observer Pattern (Event Channels) | Phase 3 |
| Command Pattern | Phase 5 |
| Object Pool Pattern | Phase 10 |
| Factory Pattern | Phase 4 |
| Composite Pattern | Phase 4 |

---

## 2. Phase 0: Foundation

### Goal
Establish project structure, assembly boundaries, and dependency injection framework.

### Why This Phase First
Without proper foundations, technical debt accumulates immediately. This phase ensures all subsequent code follows architectural rules from day one.

---

### Deliverables

#### 2.1 Folder Structure Creation

Create the following empty folders:

```
Assets/Scripts/
├── Core/
│   ├── Abstractions/
│   ├── Data/
│   ├── Domain/
│   └── Events/
├── Infrastructure/
│   ├── Audio/
│   ├── Input/
│   ├── Leveling/
│   ├── Persistence/
│   └── Pooling/
├── App/
│   ├── Installers/
│   └── Services/
├── Presentation/
│   ├── Gameplay/
│   │   ├── Knife/
│   │   ├── Target/
│   │   └── Hitables/
│   ├── UI/
│   │   ├── HUD/
│   │   ├── Popups/
│   │   └── Common/
│   └── VFX/
└── Editor/
    ├── Tools/
    └── Inspectors/
```

#### 2.2 Assembly Definitions

| Assembly | Location | References |
|----------|----------|------------|
| `KnifeToss.Core` | `Scripts/Core/` | None |
| `KnifeToss.Infrastructure` | `Scripts/Infrastructure/` | Core |
| `KnifeToss.App` | `Scripts/App/` | Core, Infrastructure |
| `KnifeToss.Presentation` | `Scripts/Presentation/` | Core, App |
| `KnifeToss.Editor` | `Scripts/Editor/` | All + Editor |

#### 2.3 VContainer Setup

| Component | Layer | Location | Purpose |
|-----------|-------|----------|---------|
| `GameplayInstaller` | 🟨 App | `App/Installers/` | LifetimeScope, registers all dependencies |

**Scene Setup:**
- Create `GameplayInstaller` GameObject in scene
- Attach VContainer LifetimeScope component
- This becomes the composition root

#### 2.4 Scene Configuration

- Main scene with 2D camera (portrait orientation)
- Basic background sprite placeholder
- GameplayInstaller at scene root

---

### Patterns Introduced

| Pattern | Application |
|---------|-------------|
| **ScriptableObject Configuration** | Prepare `Configurations/` folder structure |

---

### Test Criteria

- [ ] All folders exist with correct hierarchy
- [ ] Each assembly definition compiles independently
- [ ] Core assembly has NO Unity references
- [ ] Play Mode starts without exceptions
- [ ] VContainer LifetimeScope initializes (verify in console)
- [ ] Attempting to reference Presentation from Core fails compilation

---

## 3. Phase 1: Knife Flight

### Goal
Player taps, knife spawns and flies upward. Knife has clear state transitions.

### Why This Order
The knife is the player's primary interaction. Establishing the throw-flight loop early allows testing the core feel.

---

### Deliverables by Layer

#### 3.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `KnifeState` | `Core/Domain/` | Enum: Loaded, InFlight, Stuck, Bounced, Pooled |
| `KnifeData` | `Core/Data/` | Struct: speed, embed offset |
| `IKnifeController` | `Core/Abstractions/` | Interface for knife logic |

**KnifeState Enum:**
```
Loaded → InFlight → Stuck
                  → Bounced → Pooled
```

#### 3.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `InputAdapter` | `Infrastructure/Input/` | Wraps Unity Input System, implements interface |

#### 3.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `IThrowInputPort` | (interface in Core) | Port for input |
| `ThrowService` | `App/Services/` | Coordinates throw action |

**Registration in GameplayInstaller:**
- Register `InputAdapter` as `IThrowInputPort`
- Register `ThrowService`

#### 3.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `KnifeView` | `Presentation/Gameplay/Knife/` | MonoBehaviour, visual representation |
| `KnifeThrowerView` | `Presentation/Gameplay/Knife/` | Handles spawning, receives input |

**MonoBehaviour Wrapper Pattern:**
- `KnifeView` is thin — only Unity specifics (Rigidbody2D, SpriteRenderer)
- Business logic (state transitions) delegated to services/pure C#

#### 3.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `KnifeConfig.asset` | `Configurations/Gameplay/` | Throw speed, spawn position offset |

#### 3.6 Prefabs

| Prefab | Contents |
|--------|----------|
| `Knife.prefab` | Sprite, Rigidbody2D, Collider2D (trigger), KnifeView |

---

### Patterns Applied

| Pattern | Application |
|---------|-------------|
| **MonoBehaviour Wrapper** | KnifeView (presentation) separate from logic |
| **State Pattern** | KnifeState enum with clear transitions |
| **ScriptableObject Configuration** | KnifeConfig for throw parameters |

---

### Test Criteria

- [ ] Tap/click screen → Knife spawns at launcher position
- [ ] Knife flies straight up at configured speed
- [ ] Knife state transitions: Loaded → InFlight
- [ ] Speed is read from ScriptableObject (change value, see different speed)
- [ ] Multiple rapid taps spawn multiple knives
- [ ] No exceptions in console

---

## 4. Phase 2: Target & Sticking

### Goal
Rotating target in scene. Knives stick to target surface and rotate with it.

### Why This Order
Target is the second core element. Sticking mechanic is fundamental to all gameplay.

---

### Deliverables by Layer

#### 4.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `IRotationBehavior` | `Core/Abstractions/` | Strategy interface for rotation |
| `ConstantRotationBehavior` | `Core/Domain/` | Fixed speed rotation logic |
| `TargetData` | `Core/Data/` | Struct: radius, rotation config |

**Strategy Pattern Interface:**
```
IRotationBehavior
├── GetRotationDelta(deltaTime) → float
└── (implementations are interchangeable)
```

#### 4.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetGeometry` | `Infrastructure/Leveling/` | Surface point calculation, stick positioning |

**Sticking Algorithm (pure math):**
1. Get target circle center and radius
2. Calculate direction from center to knife position
3. Find surface point on perimeter
4. Calculate knife rotation to face center
5. Return position and rotation for sticking

#### 4.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetService` | `App/Services/` | Manages target state, tracks stuck knives |

#### 4.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetView` | `Presentation/Gameplay/Target/` | MonoBehaviour, applies rotation |
| `RotatingTargetBehavior` | `Presentation/Gameplay/Target/` | Applies IRotationBehavior each frame |

**Knife Sticking in Presentation:**
- On collision detected, call Infrastructure for math
- Parent knife transform to target
- Set knife Rigidbody2D to Kinematic
- Update KnifeState to Stuck

#### 4.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `TargetConfig.asset` | `Configurations/Gameplay/` | Default rotation speed, size |

#### 4.6 Prefabs

| Prefab | Contents |
|--------|----------|
| `Target.prefab` | Sprite, CircleCollider2D, TargetView, RotatingTargetBehavior |

---

### Patterns Applied

| Pattern | Application |
|---------|-------------|
| **Strategy Pattern** | IRotationBehavior with ConstantRotationBehavior |
| **MonoBehaviour Wrapper** | TargetView thin, rotation logic in pure C# |

---

### Test Criteria

- [ ] Target appears in scene, rotates continuously
- [ ] Rotation speed matches ScriptableObject value
- [ ] Throw knife at target → Knife sticks at contact point
- [ ] Stuck knife rotates with target (parented correctly)
- [ ] Knife tip points toward target center
- [ ] Multiple knives can stick at different positions
- [ ] Change rotation speed in config → Target rotates faster/slower

---

## 5. Phase 3: Game States

### Goal
Win when enough knives stick. Fail when knife hits knife. Clear state communication via events.

### Why This Order
Win/fail states complete the core loop. Event channels establish cross-system communication pattern.

---

### Deliverables by Layer

#### 5.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `StateMachine` | `Core/Domain/` | Abstract state machine manager |
| `GameStateMachine` | `Core/Domain/` | Composes level states (loading/playing/completed/failed) |
| `LevelStateBase` | `Core/Domain/` | Base class for level states |
| `LevelLoadingState` | `Core/Domain/` | Pre-play state |
| `LevelPlayingState` | `Core/Domain/` | Active play state |
| `LevelCompletedState` | `Core/Domain/` | Win state |
| `LevelFailedState` | `Core/Domain/` | Fail state |
| `LevelRules` | `Core/Domain/` | Knife count + stick threshold |
| `LevelSession` | `Core/Domain/` | Tracks throws, sticks, transitions via state machine |
| `KnifeImpactType` | `Core/Domain/` | Stuck vs Bounced |
| `KnifeImpactEventData` | `Core/Data/` | Impact payload (type + point) |
| `LevelStateChangedEventData` | `Core/Data/` | Previous/new state ids (string) |

**LevelSession Responsibilities:**
- Track knives thrown, knives stuck
- Track win/fail state via state classes (no enums)
- Expose current state id (string)
- Raise events on state changes

**Event Channel Pattern (ScriptableObject):**
```
KnifeImpactEvent : ScriptableObject
├── OnKnifeStuck (Action<Vector3>)
├── OnKnifeBounced (Action<Vector3>)
└── Raise methods
```

#### 5.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `KnifeImpactEventChannel` | `Infrastructure/Events/` | Raises impact payloads |
| `LevelStateChangedEventChannel` | `Infrastructure/Events/` | Raises state changes |
| `LevelRulesConfigAsset` | `Infrastructure/Leveling/` | Provides LevelRules (knife count, stick threshold) |

#### 5.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelSessionService` | `App/Services/` | Manages LevelSession lifecycle, raises events |
| `GameFlowController` | `App/Services/` | Starts level session (future: stage/level control) |
| `ThrowService` update | `App/Services/` | Blocks throws when session disallows, registers throws |

**Collision Flow:**
```
KnifeView detects collision
    → CollisionRouter → Handler
        → If Target: stop, stick, increment count, check win via LevelSessionService
        → If Knife: bounce, trigger fail via LevelSessionService, pause Time.timeScale
        → Events raised from LevelSessionService
```

#### 5.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `KnifeHitTargetHandler` | `Presentation/Gameplay/Collision/` | Uses LevelSessionService for stick tracking |
| `KnifeHitKnifeHandler` | `Presentation/Gameplay/Collision/` | Uses LevelSessionService for bounce/fail + pause |
| `KnifeView` bounce state | `Presentation/Gameplay/Knife/States/` | Falls/rotates on fail, collider disabled |
| `KnifeStateMachine` | `Core/Domain/` | Composes knife states; managed outside view |

#### 5.5 Configurations (Event Channel Instances)

| Asset | Location | Purpose |
|-------|----------|---------|
| `KnifeImpactEventChannel.asset` | `Configurations/EventChannels/` | Instance of knife impact events |
| `LevelStateChangedEventChannel.asset` | `Configurations/EventChannels/` | Instance of level state events |
| `LevelRulesConfig.asset` | `Configurations/Gameplay/` | Knife count + stick threshold |

---

### Patterns Applied

| Pattern | Application |
|---------|-------------|
| **Observer Pattern** | ScriptableObject event channels |
| **State Pattern** | State classes + generic state machine (level, knife) |

---

### Test Criteria

- [ ] Knife sticks in target → KnifeStuck event raised (log in console)
- [ ] Stick configured threshold → LevelCompleted event raised
- [ ] Knife hits stuck knife → KnifeBounced event raised
- [ ] Knife hits knife → LevelFailed event raised
- [ ] Bounced knife falls with gravity and rotation
- [ ] LevelSession correctly tracks: thrown count, stuck count, state
- [ ] Throws are blocked when no knives remain
- [ ] Game pauses on fail (Time.timeScale = 0)

---

## 6. Phase 4: Level Configuration

### Goal
Levels are defined in ScriptableObjects and loaded through a factory-driven pipeline that configures targets, rules, and knife counts per level.

### Why This Order
Data-driven levels unlock content creation without code changes and introduce Factory/Composite patterns used by later phases.

---

### Deliverables by Layer

#### 6.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelData` | `Core/Data/` | Single level definition (knife count, stick threshold, target group) |
| `StageData` | `Core/Data/` | Collection of levels plus presentation theme hooks |
| `TargetDefinition` | `Core/Data/` | Per-target configuration (position, scale, rotation behavior type/speed, knivesToBreak) |
| `TargetGroupConfig` | `Core/Data/` | Composite of all targets in a level |
| `ILevelCatalog` | `Core/Abstractions/` | Interface exposing StageData/LevelData retrieval |

#### 6.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `StagesConfigCatalog` | `Infrastructure/Leveling/` | Implements `ILevelCatalog`, reads `StagesConfig.asset`, caches StageData |
| `TargetFactory` | `Infrastructure/Leveling/` | Builds targets from `TargetDefinition`, resolves rotation strategy, applies transforms |
| `LevelSpawner` | `Infrastructure/Leveling/` | Clears prior level, spawns targets via factory under a level root |

#### 6.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelSetupService` | `App/Services/` | Orchestrates level load using `ILevelCatalog`, `LevelSpawner`, `LevelSessionService`, and throw/target configuration |

**Level Load Sequence:**
1. Resolve `LevelData` from catalog (stage index, level index).
2. Derive `LevelRules` (knife count, stick threshold) and configure thrower.
3. Clear current level objects via `LevelSpawner`.
4. Spawn targets through `TargetFactory` into level root; apply rotation strategies.
5. Initialize `LevelSession` with rules and target group data.
6. Transition to Playing state.

#### 6.4 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `StagesConfig.asset` | `Configurations/Gameplay/` | ScriptableObject containing stages/levels/targets data |

**Create Test Content:**
- Stage 1 with 3–4 levels that vary rotation speed/type, knife count, and stick threshold.
- At least one level with multiple targets to exercise composite spawning.

---

### Patterns Applied

| Pattern | Application |
|---------|-------------|
| **Factory Pattern** | `TargetFactory` constructs targets from `TargetDefinition` |
| **Composite Pattern** | `TargetGroupConfig` aggregates multiple targets per level |
| **ScriptableObject Configuration** | `StagesConfig.asset` drives catalog data |

---

### Test Criteria

- [ ] `StagesConfig.asset` defines multiple stages/levels with varying parameters.
- [ ] `LevelSetupService.LoadLevel(0, 0)` loads the first level with correct target layout.
- [ ] Loading a different level changes rotation speed/type and target placement accordingly.
- [ ] Knife count and stick threshold follow the selected `LevelData`.
- [ ] Multiple targets spawn under the level root and rotate per definition.
- [ ] Win/fail logic respects per-level rules without regressions to earlier phases.

---

## 7. Phase 5: Flow & Progression

### Goal
Completing level advances to next. Completing stage advances to next stage. Fail shows options.

### Why This Order
Game flow ties together all previous systems. Introduces command pattern for player actions.

---

### Deliverables by Layer

#### 7.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `ILevelCommand` | `Core/Abstractions/` | Command interface |
| `RetryLevelCommand` | `Core/Domain/` | Reload current level |
| `NextLevelCommand` | `Core/Domain/` | Advance to next level |
| `RestartStageCommand` | `Core/Domain/` | Reset to stage start |
| `LevelCommandEvent` | `Core/Events/` | ScriptableObject event for commands |

**Command Pattern:**
```
ILevelCommand
├── Execute()
└── implementations: Retry, Next, RestartStage
```

#### 7.2 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelFlowCoordinator` | `App/Services/` | Listens to events, executes flow |
| `StageProgressTracker` | `App/Services/` | Tracks current stage/level indices |

**Flow Coordinator Logic:**
```
On LevelCompleted:
├── If more levels in stage → NextLevelCommand.Execute()
├── If last level → Advance stage, load first level
└── If last stage → Show completion (placeholder)

On LevelFailed:
└── Show fail UI (let player choose command)
```

#### 7.3 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `SimpleFailPopup` | `Presentation/UI/Popups/` | Temporary fail UI |

**Fail Popup (temporary):**
- Two buttons: Retry, Restart Stage
- Each button raises appropriate command event
- Popup shown when LevelFailed event received
- Popup hidden when level loads

#### 7.4 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `LevelCommandEventChannel.asset` | `Configurations/Events/` | Command events |

---

### Patterns Applied

| Pattern | Application |
|---------|-------------|
| **Command Pattern** | LevelCommands encapsulate navigation actions |
| **Observer Pattern** | Flow coordinator listens to level events |

---

### Test Criteria

- [ ] Complete level 1 → Automatically loads level 2
- [ ] Complete all levels in stage → Loads next stage
- [ ] Fail level → Popup appears with Retry/Restart
- [ ] Click Retry → Same level reloads, popup hides
- [ ] Click Restart Stage → Stage level 1 loads
- [ ] Game unpauses after retry/restart
- [ ] StageProgressTracker correctly reports current position

---

## 8. Phase 6: Collectibles

### Goal
Tomatoes attached to targets. Hitting them awards currency.

### Why This Order
Collectibles add depth without changing core mechanics. Uses established event pattern.

---

### Deliverables by Layer

#### 8.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableDefinition` | `Core/Data/` | Configuration for hitable placement |
| `HitableObjectHitEvent` | `Core/Events/` | ScriptableObject event |
| `IFruitTracker` | `Core/Abstractions/` | Interface for fruit counting |

**Add to TargetDefinition:**
- hitableObjects: List<HitableDefinition>

#### 8.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableSpawner` | `Infrastructure/Leveling/` | Positions hitables around target |

**Spawn Positioning:**
- Calculate surface position from angle
- Parent to target transform
- Optional: orient facing outward

#### 8.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `FruitService` | `App/Services/` | Tracks fruit count, listens to events |

#### 8.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `HitableView` | `Presentation/Gameplay/Hitables/` | MonoBehaviour for tomato |

**Hit Behavior:**
- Knife collision detected
- Disable colliders (prevent double-hit)
- Raise HitableObjectHitEvent
- Destroy GameObject

#### 8.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `TomatoConfig.asset` | `Configurations/Gameplay/` | Tomato prefab, particle ref |
| `HitableHitEventChannel.asset` | `Configurations/Events/` | Event instance |

#### 8.6 Prefabs

| Prefab | Contents |
|--------|----------|
| `Tomato.prefab` | Sprite, Collider2D, HitableView |

---

### Test Criteria

- [ ] Level config includes 2 tomatoes on target
- [ ] Tomatoes appear at configured angles
- [ ] Tomatoes rotate with target
- [ ] Knife hits tomato → Tomato destroyed
- [ ] HitableObjectHitEvent raised (log)
- [ ] FruitService count increments
- [ ] Knife continues to target (doesn't stop)
- [ ] Tomato hit does NOT count toward target threshold

---

## 9. Phase 7: Feedback Systems

### Goal
Audio and visual feedback for all game actions. Game feels responsive and satisfying.

### Why This Order
Feedback transforms mechanics into experience. Uses event listeners established earlier.

---

### Deliverables by Layer

#### 9.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `IAudioPlayer` | `Core/Abstractions/` | Interface for SFX |
| `IMusicPlayer` | `Core/Abstractions/` | Interface for BGM |
| `SoundType` | `Core/Data/` | Enum of all sound types |

#### 9.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `AudioPlayer` | `Infrastructure/Audio/` | Implements IAudioPlayer |
| `MusicPlayer` | `Infrastructure/Audio/` | Implements IMusicPlayer |
| `ParticleSpawner` | `Infrastructure/Leveling/` | Spawns VFX at positions |

**AudioPlayer:**
- Uses single AudioSource
- PlayOneShot for SFX
- Reads clips from SoundSettings ScriptableObject

#### 9.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `AudioFeedbackService` | `App/Services/` | Listens to events, plays sounds |
| `VFXFeedbackService` | `App/Services/` | Listens to events, spawns particles |

**Event → Sound Mapping:**
```
KnifeStuck → Wood impact sound
KnifeBounced → Metal clang sound
HitableHit → Splat sound
LevelCompleted → Win jingle
LevelFailed → Fail sound
```

#### 9.4 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `TargetHitFeedback` | `Presentation/Gameplay/Target/` | Shake + tint on hit |
| `KnifeLoadAnimator` | `Presentation/Gameplay/Knife/` | Slide-up animation |
| `KnifeBounceAnimator` | `Presentation/Gameplay/Knife/` | Fall + rotation animation |

**DOTween Animations:**
- Target shake: DOShakePosition
- Target tint: DOColor sequence
- Knife load: DOMove + DOFade
- Knife bounce: DOMove (gravity) + DORotate

#### 9.5 Configurations

| Asset | Location | Purpose |
|-------|----------|---------|
| `SoundSettings.asset` | `Configurations/Audio/` | All audio clip references |
| `TargetHitAnimationConfig.asset` | `Configurations/Animations/` | Shake/tint parameters |
| `KnifeBounceConfig.asset` | `Configurations/Animations/` | Bounce parameters |

#### 9.6 Prefabs

| Prefab | Contents |
|--------|----------|
| `WoodSplinterParticles.prefab` | Particle system for knife stick |
| `TomatoSplatParticles.prefab` | Particle system for tomato hit |

---

### Test Criteria

- [ ] Throw knife → Hear throw whoosh
- [ ] Knife sticks → Hear wood thunk + see splinters
- [ ] Knife sticks → Target shakes and flashes
- [ ] Knife hits knife → Hear metal clang
- [ ] Hit tomato → Hear splat + see juice particles
- [ ] Background music plays and loops
- [ ] New knife loads with slide-up animation
- [ ] All parameters configurable via ScriptableObjects

---

## 10. Phase 8: User Interface

### Goal
Complete HUD showing game state. Proper fail popup with navigation.

### Why This Order
UI visualizes all systems built so far. MVVM-lite pattern for data binding.

---

### Deliverables by Layer

#### 10.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `ILevelHudData` | `Core/Abstractions/` | Interface exposing HUD data |
| `ButtonClickEvent` | `Core/Events/` | For UI sound feedback |

#### 10.2 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `LevelHudViewModel` | `App/Services/` | Provides data to HUD views |
| `SettingsService` | `App/Services/` | Manages mute state |

**ViewModel Pattern:**
```
LevelHudViewModel implements ILevelHudData
├── CurrentStage: int
├── CurrentLevel: int
├── TotalLevelsInStage: int
├── KnivesRemaining: int
├── TotalFruits: int
└── IsMuted: bool
```

#### 10.3 Presentation Layer 🟧

| Component | Location | Purpose |
|-----------|----------|---------|
| `StageProgressBarView` | `Presentation/UI/HUD/` | Level progress dots |
| `KnifeMagazineView` | `Presentation/UI/HUD/` | Remaining knife icons |
| `FruitCounterView` | `Presentation/UI/HUD/` | Total fruits display |
| `StageLabelView` | `Presentation/UI/HUD/` | "STAGE 1" text |
| `MuteButtonView` | `Presentation/UI/HUD/` | Toggle audio |
| `StageFailPopupView` | `Presentation/UI/Popups/` | Retry/Restart buttons |
| `BasePopupView` | `Presentation/UI/Common/` | Base class for popups |

**UI Raycast Blocking:**
- Before throw, check if pointer over UI
- If UI element under pointer, suppress throw
- Prevents accidental throws when clicking buttons

#### 10.4 Prefabs

| Prefab | Contents |
|--------|----------|
| `HUD.prefab` | Canvas with all HUD elements |
| `StageFailPopup.prefab` | Popup canvas overlay |
| `KnifeIcon.prefab` | Single knife for magazine |
| `LevelDot.prefab` | Progress indicator dot |

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
Persistence makes progression meaningful. Follows repository pattern.

---

### Deliverables by Layer

#### 11.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `PlayerData` | `Core/Data/` | Root data structure |
| `PlayerProgressData` | `Core/Data/` | Stage/level progress |
| `PlayerSettingsData` | `Core/Data/` | Audio, vibration settings |
| `IPlayerDataStore` | `Core/Abstractions/` | Interface for save/load |

**Data Structure:**
```
PlayerData
├── progress: PlayerProgressData
│   ├── currentStageIndex
│   ├── highestStageIndex
│   └── totalFruitsCollected
└── settings: PlayerSettingsData
    └── isMuted
```

#### 11.2 Infrastructure Layer 🟩

| Component | Location | Purpose |
|-----------|----------|---------|
| `FilePlayerDataStore` | `Infrastructure/Persistence/` | JSON file read/write |

**File Operations:**
- Path: Application.persistentDataPath + "/playerData.json"
- Atomic write: temp file, then rename
- Load with fallback to defaults
- Handle corrupted file gracefully

#### 11.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| `PlayerDataService` | `App/Services/` | Manages data lifecycle |

**Save Triggers:**
- Level completed
- Stage completed
- Settings changed
- Fruit collected (debounced)

**Load Trigger:**
- Game start (in installer)

---

### Test Criteria

- [ ] Complete a level → Progress saved
- [ ] Close and reopen game → Resumes at correct level
- [ ] Toggle mute → Setting persists after restart
- [ ] Collect fruits → Total persists after restart
- [ ] Delete save file → Game starts fresh
- [ ] Corrupt save file → Game handles gracefully, uses defaults
- [ ] Save file is readable JSON

---

## 12. Phase 10: Polish & Performance

### Goal
Object pooling, additional rotation behaviors, pre-stuck knives, multi-target levels.

### Why This Phase Last
Optimization and variety build on stable foundation. Pool pattern requires mature lifecycle.

---

### Deliverables by Layer

#### 12.1 Core Layer 🟦

| Component | Location | Purpose |
|-----------|----------|---------|
| `IPoolable` | `Core/Abstractions/` | Interface for pooled objects |
| `PulsatingRotationBehavior` | `Core/Domain/` | Speed oscillates |
| `FlippingRotationBehavior` | `Core/Domain/` | Direction reverses |
| `CurveRotationBehavior` | `Core/Domain/` | Follows AnimationCurve |

**Additional Rotation Strategies:**
```
IRotationBehavior
├── ConstantRotationBehavior (Phase 2)
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
- Warmup(count): Pre-instantiate
- Get(): Retrieve or create
- Return(obj): Deactivate, re-pool
- Reset state on return

#### 12.3 Application Layer 🟨

| Component | Location | Purpose |
|-----------|----------|---------|
| Update `ThrowService` | | Use pool instead of Instantiate |
| Update `VFXFeedbackService` | | Use particle pool |

**Pre-Stuck Knives:**
- Add to TargetDefinition: preStuckKnives config
- LevelSpawner creates knives already stuck at start
- These are obstacles, not throwable

**Multi-Target Levels:**
- TargetGroupConfig already supports list
- Update LevelSession to track multiple targets
- Win when ALL targets destroyed

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
| **PROTOTYPE** | Phase 3 | Throw, stick, win, fail — core loop works |
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
7. ✅ Code follows naming conventions from Architecture.md
8. ✅ No violations of "What to Avoid" from Architecture.md

---

### Quick Reference: Layer Placement

| If creating... | Place in... |
|----------------|-------------|
| Interface | 🟦 Core/Abstractions/ |
| Data struct/class | 🟦 Core/Data/ |
| Pure C# logic | 🟦 Core/Domain/ |
| ScriptableObject event definition | 🟦 Core/Events/ |
| Unity system wrapper | 🟩 Infrastructure/ |
| Spawner/Factory | 🟩 Infrastructure/Leveling/ |
| File I/O | 🟩 Infrastructure/Persistence/ |
| Service orchestrating features | 🟨 App/Services/ |
| VContainer installer | 🟨 App/Installers/ |
| MonoBehaviour | 🟧 Presentation/ |
| UI component | 🟧 Presentation/UI/ |

---

**END OF DOCUMENT**
