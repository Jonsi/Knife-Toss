# KNIFE TOSS — Development Plan

**Version 1.0**  
**Last Updated: December 2024**  
**Based on:** Architecture.md, GameDesignDocument.md

---

## Table of Contents

1. [Plan Overview](#1-plan-overview)
2. [Phase 0: Project Setup](#2-phase-0-project-setup)
3. [Phase 1: Core Throw Loop](#3-phase-1-core-throw-loop)
4. [Phase 2: Target System](#4-phase-2-target-system)
5. [Phase 3: Collision & Win/Fail States](#5-phase-3-collision--winfail-states)
6. [Phase 4: Level System](#6-phase-4-level-system)
7. [Phase 5: Stage Progression](#7-phase-5-stage-progression)
8. [Phase 6: Hitable Objects (Tomatoes)](#8-phase-6-hitable-objects-tomatoes)
9. [Phase 7: Audio System](#9-phase-7-audio-system)
10. [Phase 8: Visual Feedback & Polish](#10-phase-8-visual-feedback--polish)
11. [Phase 9: UI System](#11-phase-9-ui-system)
12. [Phase 10: Persistence & Save System](#12-phase-10-persistence--save-system)
13. [Phase 11: Final Polish & Optimization](#13-phase-11-final-polish--optimization)
14. [Phase Summary Table](#14-phase-summary-table)

---

## 1. Plan Overview

### 1.1 Philosophy

Each phase follows the principle of **Vertical Slices** — delivering a playable, testable feature before moving to the next. This allows:

- Early detection of design issues
- Continuous validation in Unity Play Mode
- Incremental complexity building
- Clear milestone tracking

### 1.2 Phase Structure

Each phase includes:

| Section | Description |
|---------|-------------|
| **Goal** | What this phase achieves |
| **Prerequisites** | What must be completed before starting |
| **Deliverables** | Concrete outputs (scripts, prefabs, configs) |
| **Architecture Components** | Which layers/patterns are introduced |
| **Test Criteria** | How to verify the phase is complete |
| **Estimated Effort** | Relative complexity (S/M/L) |

### 1.3 Layer Legend

Throughout this document:
- 🟦 **Core** — Pure C#, no Unity dependencies
- 🟩 **Infrastructure** — Implementations, external systems
- 🟨 **App** — Services, coordinators
- 🟧 **Presentation** — MonoBehaviours, Unity components

---

## 2. Phase 0: Project Setup

### Goal
Establish the foundational project structure, assembly definitions, and dependency injection framework.

### Prerequisites
- Unity 2022+ LTS installed
- Required packages imported (VContainer, UniTask, DOTween, Input System, TextMesh Pro)

### Deliverables

#### Folder Structure
```
Assets/Scripts/
├── Core/
│   ├── Abstractions/
│   ├── Data/
│   ├── Domain/
│   └── Events/
├── Infrastructure/
├── App/
│   ├── Installers/
│   └── Services/
├── Presentation/
│   ├── Gameplay/
│   ├── UI/
│   └── VFX/
└── Editor/
```

#### Assembly Definitions
| Assembly | File |
|----------|------|
| KnifeToss.Core | `Scripts/Core/KnifeToss.Core.asmdef` |
| KnifeToss.Infrastructure | `Scripts/Infrastructure/KnifeToss.Infrastructure.asmdef` |
| KnifeToss.App | `Scripts/App/KnifeToss.App.asmdef` |
| KnifeToss.Presentation | `Scripts/Presentation/KnifeToss.Presentation.asmdef` |
| KnifeToss.Editor | `Scripts/Editor/KnifeToss.Editor.asmdef` |

#### Core Files
| File | Layer | Purpose |
|------|-------|---------|
| `GameplayInstaller` | 🟨 App | VContainer LifetimeScope, DI registration |
| `GameBootstrapper` | 🟧 Presentation | Entry point, ensures installer runs |

#### Scene Setup
- Main scene with GameplayInstaller attached to root GameObject
- Camera configured for 2D portrait mode
- Basic background placeholder

### Architecture Components
- VContainer LifetimeScope setup
- Assembly definition boundaries

### Test Criteria
- [ ] Project compiles without errors
- [ ] Play Mode enters without exceptions
- [ ] VContainer installer initializes (log message)
- [ ] Assembly boundaries prevent invalid references

### Estimated Effort: **S (Small)**

---

## 3. Phase 1: Core Throw Loop

### Goal
Player can tap to throw a knife upward. The knife flies straight up and exits the screen.

### Prerequisites
- Phase 0 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `IThrowInput` | 🟦 Core | Interface for throw input |
| `ThrowInputHandler` | 🟧 Presentation | Unity Input System wrapper |
| `KnifeView` | 🟧 Presentation | MonoBehaviour for knife visuals/physics |
| `KnifeController` | 🟦 Core | Pure C# knife state and logic |
| `KnifeConfig` | 🟦 Core | Data structure for knife settings |
| `KnifeThrower` | 🟧 Presentation | Handles spawning and launching knives |

#### Prefabs
| Prefab | Contents |
|--------|----------|
| `Knife.prefab` | Sprite, Rigidbody2D, Collider2D, KnifeView |

#### Configurations
| Asset | Purpose |
|-------|---------|
| `KnifeConfig.asset` | Throw speed, spawn position |

#### Input Actions
- Configure Input System action for "Throw" (touch/click release)

### Architecture Components
- 🟦 **Core:** `IThrowInput` interface
- 🟧 **Presentation:** Input handling, knife MonoBehaviour
- **Pattern:** MonoBehaviour Wrapper (KnifeView wraps KnifeController)

### Test Criteria
- [ ] Tap/click anywhere on screen
- [ ] Knife spawns at bottom of screen
- [ ] Knife flies straight upward at constant speed
- [ ] Knife exits top of screen
- [ ] Multiple taps spawn multiple knives
- [ ] No exceptions in console

### Estimated Effort: **M (Medium)**

---

## 4. Phase 2: Target System

### Goal
A rotating target exists in the scene. Knives can stick into the target and rotate with it.

### Prerequisites
- Phase 1 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `IRotationBehavior` | 🟦 Core | Interface for rotation strategies |
| `ConstantRotation` | 🟦 Core | Simple constant speed rotation |
| `TargetView` | 🟧 Presentation | MonoBehaviour for target |
| `TargetController` | 🟦 Core | Rotation logic, knife tracking |
| `TargetConfig` | 🟦 Core | Target settings data |
| `TargetGeometry` | 🟦 Core | Utility for surface calculations |

#### Prefabs
| Prefab | Contents |
|--------|----------|
| `Target.prefab` | Sprite, CircleCollider2D, TargetView |

#### Configurations
| Asset | Purpose |
|-------|---------|
| `TargetConfig.asset` | Rotation speed, size, position |

### Architecture Components
- 🟦 **Core:** Rotation behavior interface + implementation
- **Pattern:** Strategy Pattern for rotation behaviors

### Knife Sticking Logic
1. Detect collision with target
2. Calculate surface contact point
3. Rotate knife to face target center
4. Parent knife to target transform
5. Disable knife physics

### Test Criteria
- [ ] Target rotates continuously in scene
- [ ] Throw knife at target
- [ ] Knife embeds at contact point
- [ ] Knife rotates with target after sticking
- [ ] Multiple knives can stick to target
- [ ] Knives maintain their relative positions

### Estimated Effort: **M (Medium)**

---

## 5. Phase 3: Collision & Win/Fail States

### Goal
Knife-to-knife collision causes failure. Sticking enough knives destroys the target (win).

### Prerequisites
- Phase 2 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `LevelSession` | 🟦 Core | Current attempt state (knives thrown, stuck, failed) |
| `LevelRules` | 🟦 Core | Level parameters (knife count, threshold) |
| `KnifeCollisionHandler` | 🟧 Presentation | Detects knife-to-knife collision |
| `TargetHitTracker` | 🟧 Presentation | Tracks hits, triggers destruction |
| `ILevelSession` | 🟦 Core | Interface for session access |

#### Events (ScriptableObject Channels)

| Event Channel | Raised When |
|---------------|-------------|
| `KnifeImpactEventChannel` | Knife sticks or bounces |
| `LevelStateEventChannel` | Win or fail triggered |

### Architecture Components
- 🟦 **Core:** Session state, rules, events
- **Pattern:** Observer Pattern via event channels
- **Pattern:** State tracking in LevelSession

### Collision Detection Flow
```
Knife Collision
    │
    ├── Hit Target? → Stick → Increment hit count → Check threshold
    │                                                    │
    │                                         Threshold met? → WIN
    │
    └── Hit Knife? → FAIL → Raise failure event
```

### Test Criteria
- [ ] Throw knife, hits target → sticks (success)
- [ ] Throw knife, hits stuck knife → bounces off (fail state)
- [ ] Console/debug shows "FAILED" on knife-to-knife hit
- [ ] Stick required number of knives → target destroyed
- [ ] Console/debug shows "WIN" when target destroyed
- [ ] Session tracks: knives thrown, knives stuck, status

### Estimated Effort: **M (Medium)**

---

## 6. Phase 4: Level System

### Goal
Levels are defined in ScriptableObjects. Game loads level configuration and sets up the scene accordingly.

### Prerequisites
- Phase 3 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `LevelData` | 🟦 Core | Data structure for level definition |
| `StageData` | 🟦 Core | Data structure for stage (collection of levels) |
| `StagesConfig` | 🟦 Core | ScriptableObject holding all stages |
| `ILevelCatalog` | 🟦 Core | Interface to access level definitions |
| `StagesConfigCatalog` | 🟩 Infrastructure | Implementation of ILevelCatalog |
| `LevelSessionService` | 🟨 App | Manages level lifecycle |
| `LevelSpawner` | 🟩 Infrastructure | Instantiates level elements |

#### Configurations
| Asset | Purpose |
|-------|---------|
| `StagesConfig.asset` | Contains Stage 1 with 3-4 test levels |

### Architecture Components
- 🟦 **Core:** Level/Stage data structures
- 🟩 **Infrastructure:** Level spawning
- 🟨 **App:** Level lifecycle service
- **Pattern:** Data-Driven Design via ScriptableObjects

### Level Load Flow
```
LevelSessionService.StartLevel(stageIndex, levelIndex)
    │
    ├── Get LevelData from ILevelCatalog
    ├── Parse into LevelRules
    ├── Clear previous level objects
    ├── Spawn target(s) via LevelSpawner
    ├── Configure KnifeThrower with knife count
    └── Initialize LevelSession
```

### Test Criteria
- [ ] Create 3 different levels in StagesConfig
- [ ] Level 1: Slow rotation, 5 knives, threshold 5
- [ ] Level 2: Faster rotation, 6 knives, threshold 6
- [ ] Level 3: Different rotation speed, different count
- [ ] Call `StartLevel(0, 0)` → Level 1 loads correctly
- [ ] Call `StartLevel(0, 1)` → Level 2 loads with different settings
- [ ] Target parameters match configuration

### Estimated Effort: **M (Medium)**

---

## 7. Phase 5: Stage Progression

### Goal
Completing a level advances to the next. Completing all levels in a stage advances to next stage. Failing shows retry option.

### Prerequisites
- Phase 4 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `LevelFlowCoordinator` | 🟨 App | Orchestrates level transitions |
| `LevelCommandEventChannel` | 🟦 Core | Event channel for retry/next/reset commands |
| `SimpleFailUI` | 🟧 Presentation | Temporary UI for retry button |
| `SimpleWinUI` | 🟧 Presentation | Temporary UI showing "Level Complete" |

### Architecture Components
- 🟨 **App:** Flow coordination
- **Pattern:** Command Pattern for level commands
- **Pattern:** Observer Pattern for state changes

### Flow Logic
```
Level Complete
    │
    ├── More levels in stage? → Load next level
    │
    └── Last level in stage?
            │
            ├── More stages? → Load Stage N+1, Level 1
            │
            └── Last stage? → Show "Game Complete" (placeholder)

Level Failed
    │
    └── Show Fail UI
            │
            ├── [Retry] → Reload current level
            │
            └── [Restart Stage] → Load Stage N, Level 1
```

### Test Criteria
- [ ] Complete Level 1 → Automatically loads Level 2
- [ ] Complete Level 2 → Automatically loads Level 3
- [ ] Complete all levels in Stage 1 → Loads Stage 2 Level 1 (or shows complete)
- [ ] Fail a level → Simple UI appears with Retry button
- [ ] Click Retry → Same level reloads
- [ ] Click Restart Stage → Stage resets to Level 1
- [ ] Transitions happen without errors

### Estimated Effort: **M (Medium)**

---

## 8. Phase 6: Hitable Objects (Tomatoes)

### Goal
Tomatoes can be attached to targets. Hitting them awards fruit currency and destroys them.

### Prerequisites
- Phase 5 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `HitableObjectConfig` | 🟦 Core | Configuration for hitable types |
| `HitableObjectView` | 🟧 Presentation | MonoBehaviour for tomato |
| `HitableObjectSpawner` | 🟩 Infrastructure | Spawns hitables around target |
| `HitableObjectHitEventChannel` | 🟦 Core | Event when hitable is hit |
| `FruitTracker` | 🟦 Core | Tracks fruit count in session |

#### Prefabs
| Prefab | Contents |
|--------|----------|
| `Tomato.prefab` | Sprite, Collider2D, HitableObjectView |

#### Level Data Extension
- Add hitable object configuration to LevelData
- Support: count, angular spacing, type reference

### Architecture Components
- 🟦 **Core:** Hitable config, events
- 🟩 **Infrastructure:** Spawning logic
- **Pattern:** Observer Pattern for hit events

### Test Criteria
- [ ] Configure level with 2 tomatoes on target
- [ ] Tomatoes appear at configured positions
- [ ] Tomatoes rotate with target
- [ ] Knife hits tomato → Tomato destroyed
- [ ] Fruit counter increments (debug log)
- [ ] Knife continues to target (doesn't stop at tomato)
- [ ] Hitting tomato does NOT count as knife stuck

### Estimated Effort: **S (Small)**

---

## 9. Phase 7: Audio System

### Goal
Sound effects play for all game actions. Background music loops.

### Prerequisites
- Phase 6 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `IAudioPlayer` | 🟦 Core | Interface for SFX playback |
| `IMusicPlayer` | 🟦 Core | Interface for music playback |
| `AudioPlayer` | 🟩 Infrastructure | SFX implementation |
| `MusicPlayer` | 🟩 Infrastructure | BGM implementation |
| `SoundSettings` | 🟦 Core | ScriptableObject with audio clips |
| `AudioService` | 🟨 App | Coordinates audio, listens to events |

#### Configurations
| Asset | Purpose |
|-------|---------|
| `SoundSettings.asset` | All audio clip references |

### Sound Triggers

| Event | Sound |
|-------|-------|
| Knife thrown | Whoosh/throw sound |
| Knife sticks in target | Wood impact sound |
| Knife hits knife | Metal clang sound |
| Tomato hit | Splat sound |
| Level complete | Win jingle |
| Level fail | Fail sound |
| Button click | Click sound |

### Architecture Components
- 🟦 **Core:** Audio interfaces
- 🟩 **Infrastructure:** Audio implementations
- 🟨 **App:** Audio service listening to events
- **Pattern:** Observer Pattern (events trigger sounds)

### Test Criteria
- [ ] Throw knife → hear throw sound
- [ ] Knife sticks → hear wood impact
- [ ] Knife-to-knife → hear metal clang
- [ ] Hit tomato → hear splat
- [ ] Background music plays and loops
- [ ] All sounds play without delay or stutter

### Estimated Effort: **S (Small)**

---

## 10. Phase 8: Visual Feedback & Polish

### Goal
Add particle effects, animations, and screen feedback for satisfying gameplay feel.

### Prerequisites
- Phase 7 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `TargetHitFeedback` | 🟧 Presentation | Shake and tint on hit |
| `KnifeLoadAnimation` | 🟧 Presentation | Knife slide-up animation |
| `KnifeBounceAnimation` | 🟧 Presentation | Falling knife on fail |
| `ParticleSpawner` | 🟩 Infrastructure | Spawns VFX at positions |
| `TargetHitAnimationConfig` | 🟦 Core | Shake/tint parameters |
| `KnifeBounceOffAnimationConfig` | 🟦 Core | Bounce animation parameters |

#### Prefabs
| Prefab | Contents |
|--------|----------|
| `KnifeHitParticles.prefab` | Wood splinter particle system |
| `TomatoParticles.prefab` | Juice splatter particle system |

#### Configurations
| Asset | Purpose |
|-------|---------|
| `TargetHitAnimationConfig.asset` | Shake duration, strength, tint color |
| `KnifeBounceOffAnimationConfig.asset` | Gravity, rotation, timing |

### Visual Effects List

| Effect | Trigger | Description |
|--------|---------|-------------|
| Wood splinters | Knife sticks | Particles at impact point |
| Target shake | Knife sticks | Brief shake animation |
| Target tint | Knife sticks | Flash warm color |
| Tomato splatter | Tomato hit | Juice particles |
| Knife load | New knife ready | Slide up with fade-in |
| Knife bounce | Hit another knife | Fall with tumble rotation |

### Architecture Components
- 🟧 **Presentation:** Animation controllers
- 🟩 **Infrastructure:** Particle spawning
- **Pattern:** ScriptableObject Configuration for all parameters

### Test Criteria
- [ ] Knife sticks → Wood particles spawn at contact
- [ ] Knife sticks → Target shakes briefly
- [ ] Knife sticks → Target flashes color, fades back
- [ ] Tomato hit → Splatter particles spawn
- [ ] New knife → Slides up from below, fades in
- [ ] Knife collision → Knife falls with rotation
- [ ] All animations configurable without code changes

### Estimated Effort: **M (Medium)**

---

## 11. Phase 9: UI System

### Goal
Complete HUD and popup UI for gameplay feedback and navigation.

### Prerequisites
- Phase 8 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `StageProgressBarView` | 🟧 Presentation | Shows level progress in stage |
| `KnifeMagazineDisplay` | 🟧 Presentation | Shows remaining knives |
| `FruitCountDisplay` | 🟧 Presentation | Shows total fruits |
| `MuteButton` | 🟧 Presentation | Toggles audio mute |
| `StageFailPopup` | 🟧 Presentation | Fail screen with retry/restart |
| `BasePopup` | 🟧 Presentation | Base class for popups |
| `LevelHudViewModel` | 🟨 App | Provides data to HUD views |
| `ButtonClickEventChannel` | 🟦 Core | Event for button click sounds |

#### Prefabs
| Prefab | Contents |
|--------|----------|
| `HUD.prefab` | Canvas with all HUD elements |
| `StageFailPopup.prefab` | Fail popup UI |
| `MagazineKnifeIcon.prefab` | Single knife icon for magazine |
| `LevelIcon.prefab` | Progress bar level indicator |

### UI Elements

| Element | Location | Function |
|---------|----------|----------|
| Stage label | Top center | "STAGE 1" |
| Mute button | Top left | Toggle audio |
| Progress bar | Below top | Level dots with current highlighted |
| Fruit counter | Below progress | "🍅 125" |
| Knife magazine | Bottom | Horizontal knife icons |
| Fail popup | Overlay | Continue / Restart buttons |

### Architecture Components
- 🟧 **Presentation:** UI views
- 🟨 **App:** ViewModel for data binding
- **Pattern:** MVVM-lite (ViewModel provides data, View displays)

### Test Criteria
- [ ] HUD shows current stage number
- [ ] Progress bar shows correct level count
- [ ] Current level highlighted in progress bar
- [ ] Knife magazine shows correct count
- [ ] Throwing knife reduces magazine display
- [ ] Fruit counter updates when tomato hit
- [ ] Mute button toggles audio on/off
- [ ] Fail popup appears on failure
- [ ] Continue button retries level
- [ ] Restart button resets to stage start
- [ ] UI doesn't block gameplay input (proper raycast handling)

### Estimated Effort: **L (Large)**

---

## 12. Phase 10: Persistence & Save System

### Goal
Player progress persists between sessions. Settings (mute state) are saved.

### Prerequisites
- Phase 9 complete

### Deliverables

#### Scripts

| File | Layer | Purpose |
|------|-------|---------|
| `PlayerData` | 🟦 Core | Root save data structure |
| `PlayerProgressData` | 🟦 Core | Stage/level progress |
| `PlayerSettingsData` | 🟦 Core | Audio, vibration settings |
| `IPlayerDataStore` | 🟦 Core | Interface for save/load |
| `FilePlayerDataStore` | 🟩 Infrastructure | JSON file implementation |
| `PlayerDataService` | 🟨 App | Manages player data lifecycle |
| `ISettingsService` | 🟦 Core | Interface for settings |
| `SettingsService` | 🟨 App | Settings management |

### Data Structure
```
PlayerData
├── PlayerProgressData
│   ├── currentStageIndex
│   ├── highestStageIndex
│   └── totalFruitsHit
└── PlayerSettingsData
    ├── isMuted
    └── vibrationEnabled
```

### Save/Load Flow
```
Game Start
    │
    └── PlayerDataService.Load()
            │
            ├── File exists? → Deserialize JSON
            │
            └── No file? → Create default data

Save Triggers
    │
    ├── Level complete → Save progress
    ├── Stage complete → Save progress
    ├── Mute toggled → Save settings
    └── Fruit collected → Save periodically
```

### Architecture Components
- 🟦 **Core:** Data structures, interfaces
- 🟩 **Infrastructure:** File system operations
- 🟨 **App:** Data service coordination
- **Pattern:** Repository Pattern for data access

### Test Criteria
- [ ] Complete a level → Progress saved to file
- [ ] Close and reopen game → Resumes at saved level
- [ ] Toggle mute → Setting persists after restart
- [ ] Collect fruits → Total persists after restart
- [ ] Delete save file → Game starts fresh with defaults
- [ ] Save file is valid JSON (human-readable)
- [ ] Corrupt save file → Game handles gracefully with defaults

### Estimated Effort: **M (Medium)**

---

## 13. Phase 11: Final Polish & Optimization

### Goal
Object pooling, performance optimization, additional rotation behaviors, pre-stuck knives, and final quality pass.

### Prerequisites
- Phase 10 complete

### Deliverables

#### Object Pooling

| File | Layer | Purpose |
|------|-------|---------|
| `IObjectPool<T>` | 🟦 Core | Generic pool interface |
| `KnifePool` | 🟩 Infrastructure | Knife-specific pooling |
| `ParticlePool` | 🟩 Infrastructure | Particle effect pooling |

#### Additional Rotation Behaviors

| Behavior | Description |
|----------|-------------|
| `PulsatingRotation` | Speed oscillates sinusoidally |
| `FlippingRotation` | Randomly reverses direction |
| `AccelerationCurveRotation` | Speed follows AnimationCurve |

#### Pre-Stuck Knives

- Add pre-stuck knife configuration to LevelData
- Spawn knives already embedded at level start
- These create obstacles player must avoid

#### Multi-Target Support

- Support multiple targets per level
- Each target tracked independently
- Level completes when ALL targets destroyed

#### Performance Optimization

| Area | Optimization |
|------|--------------|
| Knives | Object pooling instead of Instantiate/Destroy |
| Particles | Pool particle systems |
| Components | Cache GetComponent calls |
| Tweens | Kill tweens on level transition |
| UI | Pool magazine icons |

### Architecture Components
- **Pattern:** Object Pool Pattern
- **Pattern:** Strategy Pattern (additional rotation behaviors)

### Test Criteria

#### Pooling
- [ ] Throw many knives rapidly → No frame drops
- [ ] Particle effects spawn without GC spikes
- [ ] Profiler shows minimal allocations during gameplay

#### Rotation Behaviors
- [ ] Level with PulsatingRotation → Speed visibly oscillates
- [ ] Level with FlippingRotation → Direction randomly reverses
- [ ] Level with AccelerationCurve → Speed follows curve pattern

#### Pre-Stuck Knives
- [ ] Level with 3 pre-stuck knives → They appear at start
- [ ] Pre-stuck knives rotate with target
- [ ] Throwing into pre-stuck knife → Fail

#### Multi-Target
- [ ] Level with 2 targets → Both appear
- [ ] Destroy first target → Second remains
- [ ] Destroy both → Level complete

#### Performance
- [ ] 60 FPS maintained on target devices
- [ ] No memory leaks over extended play
- [ ] Clean level transitions (no orphaned objects)

### Estimated Effort: **L (Large)**

---

## 14. Phase Summary Table

| Phase | Name | Deliverable | Effort | Cumulative |
|-------|------|-------------|--------|------------|
| 0 | Project Setup | Folder structure, DI framework | S | S |
| 1 | Core Throw Loop | Tap → Knife flies up | M | M |
| 2 | Target System | Rotating target, knife sticking | M | M+ |
| 3 | Collision & Win/Fail | Fail on knife hit, win on threshold | M | L |
| 4 | Level System | ScriptableObject levels, spawning | M | L |
| 5 | Stage Progression | Auto-advance, retry, stage flow | M | L+ |
| 6 | Hitable Objects | Tomatoes, fruit currency | S | L+ |
| 7 | Audio System | SFX and music | S | XL |
| 8 | Visual Feedback | Particles, animations, juice | M | XL |
| 9 | UI System | HUD, popups, magazine display | L | XL+ |
| 10 | Persistence | Save/load progress and settings | M | XL+ |
| 11 | Final Polish | Pooling, optimization, extra features | L | XXL |

### Milestone Summary

| Milestone | After Phase | State |
|-----------|-------------|-------|
| **Prototype** | Phase 3 | Core loop playable, win/fail works |
| **Alpha** | Phase 6 | Full gameplay with levels and collectibles |
| **Beta** | Phase 9 | Complete game with UI |
| **Release Candidate** | Phase 11 | Polished, optimized, save system |

---

## Appendix A: Testing Checklist Template

Use this template for each phase's QA pass:

```markdown
## Phase [N] Testing Checklist

### Functional Tests
- [ ] Test case 1
- [ ] Test case 2
- [ ] ...

### Edge Cases
- [ ] Rapid input spam
- [ ] Pause/resume during action
- [ ] Background/foreground app switch

### Performance
- [ ] No frame drops
- [ ] No GC spikes in Profiler
- [ ] Memory stable over time

### Error Handling
- [ ] No exceptions in console
- [ ] Graceful handling of invalid states

### Regression
- [ ] Previous phase features still work
- [ ] No new bugs introduced
```

---

## Appendix B: Definition of Done

A phase is considered **DONE** when:

1. ✅ All deliverables implemented
2. ✅ All test criteria pass
3. ✅ No compiler errors or warnings
4. ✅ No exceptions in Play Mode
5. ✅ Code follows Architecture.md guidelines
6. ✅ Previous phases still function correctly
7. ✅ Playable in Unity Editor

---

**END OF DOCUMENT**

*This Development Plan provides a structured roadmap for building Knife Toss incrementally. Each phase results in a playable, testable state, allowing continuous validation and iteration.*

