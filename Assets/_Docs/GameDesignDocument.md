# KNIFE TOSS — Game Design Document (GDD)
**Version 1.0**  
**Document Last Updated: December 2024**  
**Platform: Mobile (iOS/Android)**  
**Engine: Unity 2D**

---

# Table of Contents

1. [Game Overview](#1-game-overview)
2. [Core Gameplay](#2-core-gameplay)
3. [Mechanics](#3-mechanics)
4. [Levels & Stages](#4-levels--stages)
5. [Game Systems](#5-game-systems)
6. [Meta & Progression](#6-meta--progression)
7. [UI/UX](#7-uiux)
8. [Audio & VFX](#8-audio--vfx)
9. [Monetization](#9-monetization)
10. [Technical Requirements](#10-technical-requirements)
11. [Expansion & Scalability](#11-expansion--scalability)
12. [Refactor Plan](#12-refactor-plan)

---

# 1. Game Overview

## 1.1 Vision Statement

**Knife Toss** is a casual arcade-style mobile game where players throw knives at rotating wooden targets. The game delivers satisfying, tactile gameplay through simple one-touch controls, creating a "just one more level" addiction loop. The core fantasy is becoming a master knife thrower with precision timing and reflexes.

## 1.2 Elevator Pitch

"Tap to throw knives into spinning wooden targets. Don't hit another knife! Collect tomatoes, progress through beautiful forest-themed stages, and challenge yourself with increasingly difficult rotation patterns."

## 1.3 Genre

- **Primary:** Casual Arcade / Skill-based Action
- **Secondary:** Hyper-casual / Puzzle-timing hybrid
- **Comparison Titles:** Knife Hit, Knife Throw, Flippy Knife

## 1.4 Target Platform

- **Primary:** Mobile (iOS and Android)
- **Supported Input:** Touch screen (single-tap), Mouse click (for testing/PC builds)
- **Screen Orientation:** Portrait mode (assumed based on vertical gameplay layout)

## 1.5 Target Audience

- **Primary Demographics:** 
  - Age: 13-45
  - Gender: All
  - Casual mobile gamers who enjoy quick, satisfying gameplay sessions
  
- **Player Psychographics:**
  - Players who enjoy timing-based challenges
  - Achievement seekers who like progression systems
  - Players with 1-5 minute session availability
  - Users who play during commutes, breaks, or downtime

## 1.6 Core Fantasy

The player embodies a skilled knife thrower in a mystical forest setting. Each successful throw embeds satisfyingly into wooden targets with tactile feedback. The fantasy is one of precision, skill mastery, and progression through increasingly challenging environments.

## 1.7 Win Condition

A **level is won** when the player successfully sticks the required number of knives into the target(s), causing the target to "break." This is tracked via the `knivesToBreak` threshold per target. When all targets in a level reach their knife threshold, the level completes.

## 1.8 Fail States

The player **fails** when any of the following occurs:

1. **Knife-to-Knife Collision:** A thrown knife hits another knife already stuck in the target. This is an instant fail condition.
   
2. **Exhausted Knives:** The player runs out of knives before destroying all targets. If `KnivesThrown >= Rules.KnifeCount` and `TargetsRemaining > 0`, the level fails.

3. **Out of Bounds:** A knife leaves the gameplay camera viewport without hitting a target (currently triggers failure, though this appears to be edge-case handling).

Upon failure:
- `Time.timeScale` is set to 0 (game pauses)
- Fail popup is displayed
- Player input for throwing is disabled
- Player may retry current level or restart the entire stage

---

# 2. Core Gameplay

## 2.1 Main Game Loop

```
┌──────────────────────────────────────────────────────────────┐
│                    SESSION START                              │
├──────────────────────────────────────────────────────────────┤
│  1. Load Stage → Load Level 1 of current stage               │
│  2. Configure Target(s) → Apply rotation, spawn hitables     │
│  3. Setup Magazine → Load knife count for level              │
│  4. Display HUD → Show progress bar, knife magazine, fruits  │
├──────────────────────────────────────────────────────────────┤
│                    GAMEPLAY LOOP                              │
├──────────────────────────────────────────────────────────────┤
│  REPEAT:                                                      │
│    • Player taps screen → Knife launches upward              │
│    • Knife flies toward rotating target                      │
│    • COLLISION RESOLUTION:                                    │
│      - Hit Target → Stick, play VFX/SFX, award score         │
│      - Hit Knife → FAIL → Show fail popup                    │
│      - Hit Tomato → Destroy tomato, award fruit, continue    │
│    • Check completion:                                        │
│      - All targets broken → Level Complete → Next Level      │
│      - Out of knives → FAIL                                  │
│  UNTIL: Level Complete OR Fail                               │
├──────────────────────────────────────────────────────────────┤
│                    LEVEL TRANSITION                           │
├──────────────────────────────────────────────────────────────┤
│  • On Complete: Auto-advance to next level                   │
│  • On Last Level of Stage: Advance to next stage             │
│  • On Fail: Show popup → Retry or Restart Stage              │
└──────────────────────────────────────────────────────────────┘
```

## 2.2 Moment-to-Moment Flow

### Pre-Throw State
- Player sees rotating target with any pre-stuck knives and hitable objects
- Magazine shows remaining knives (visual queue at bottom)
- Next knife is loaded at launcher position with fade-in animation

### Throw Action
1. Player releases touch/click
2. Knife launches instantly upward (`Vector2.up`)
3. Throw sound plays
4. Next knife begins loading animation from below

### Resolution States

**Successful Stick:**
- Knife embeds in target surface
- Parent knife to target transform (rotates with target)
- Play wood hit sound + splinter particles
- Target receives shake + tint feedback
- Score awarded (+100 per hit)
- Check if target threshold reached

**Target Destruction:**
- When `HitCount >= knivesToBreak`
- Target is destroyed (removed from scene)
- Session marks target completed
- If all targets destroyed → Level Complete

**Knife-to-Knife Collision:**
- Detected via tag ("Knife") or component check
- Knife plays bounce-off animation (falls with gravity + rotation)
- Knife-hit-knife sound plays
- Session records failure
- Input disabled, fail popup shows

## 2.3 Controls

### Touch Input
| Action | Input | Response |
|--------|-------|----------|
| Throw Knife | Tap release anywhere (not on UI) | Launch knife upward |

### Input Handling Details
- Input processed via Unity's new Input System (`InputActionReference`)
- UI raycast detection prevents throws when tapping buttons
- Supports both touch and mouse input
- Input is disabled during:
  - Fail state
  - Level transition
  - When no knives remain in magazine

## 2.4 Player Feedback Systems

### Visual Feedback
- **Knife Load Animation:** Knife slides up from below with fade-in (0.3s default, configurable)
- **Target Hit Shake:** DOTween shake effect on target sprites
- **Target Hit Tint:** Brief color flash (warm tint, fades out)
- **Bounce Animation:** Falling knife on collision with another knife
- **Particle Effects:** Wood splinters on successful stick, tomato juice particles

### Audio Feedback
- Knife throw whoosh sound
- Wood impact sound (target hit)
- Metal clang sound (knife-to-knife)
- Splat sound (tomato hit)
- UI button click sound
- Background music (looping)

### Haptic Feedback
- Vibration system exists in `PlayerSettingsData` (vibrationEnabled flag)
- Implementation appears to be planned but not fully connected

---

# 3. Mechanics

## 3.1 Knife Mechanics

### 3.1.1 Knife Properties

| Property | Default Value | Description |
|----------|---------------|-------------|
| Throw Speed | 10 units/sec | Constant velocity, no deceleration |
| Launch Direction | Vector2.up | Always straight up (no aiming) |
| Embed Offset | 0.02 units | Penetration depth into target surface |
| Snap Position | Child transform | Tip position for accurate embedding |

### 3.1.2 Knife States

```
┌─────────────┐    Throw    ┌────────────┐
│   LOADED    │ ──────────► │ IN FLIGHT  │
│ (Magazine)  │             │            │
└─────────────┘             └─────┬──────┘
                                  │
              ┌───────────────────┼───────────────────┐
              │                   │                   │
              ▼                   ▼                   ▼
       ┌────────────┐      ┌────────────┐      ┌────────────┐
       │   STUCK    │      │  BOUNCED   │      │OUT OF BOUNDS│
       │ (on target)│      │ (collision)│      │            │
       └────────────┘      └─────┬──────┘      └─────┬──────┘
                                  │                   │
                                  ▼                   ▼
                           ┌────────────┐      ┌────────────┐
                           │  RETURNED  │      │  RETURNED  │
                           │ (to pool)  │      │ (to pool)  │
                           └────────────┘      └────────────┘
```

### 3.1.3 Knife Flight Behavior
- Movement via `Rigidbody2D.linearVelocity` when available
- Fallback to `transform.position` translation
- No gravity during flight (`gravityScale = 0`)
- Maintains constant speed until collision or boundary exit

### 3.1.4 Collision Detection
- Uses both `OnTriggerEnter2D` and `OnCollisionEnter2D`
- Resolution priority:
  1. Check for knife collision first (instant fail)
  2. Check for hitable objects (tomatoes)
  3. Check for target ("Target" tag)

### 3.1.5 Knife Sticking Algorithm
1. Resolve circle collider center and radius
2. Calculate surface point on circle perimeter
3. Calculate direction from center to impact point
4. Rotate knife to face inward (toward center)
5. Position knife tip at surface point
6. Parent knife to target transform
7. Set Rigidbody2D to Static

### 3.1.6 Pre-Stuck Knives
Levels can define knives already stuck in targets at start:
- `preStuckKnives`: Number of knives to spawn
- `preStuckKnifeSpacing`: Angular degrees between knives
- `preStuckKnifeSpacingOffset`: Starting angle offset
- These knives count toward total stuck count but not player's "thrown" count
- They create obstacles the player must avoid

### 3.1.7 Bounce-Off Animation
When hitting another knife:
- Knife detaches and plays falling animation
- Configurable via `KnifeBounceOffAnimationConfig`:
  - Gravity: 9.81 (fall acceleration)
  - Impact Force: 0.2 (horizontal knockback)
  - Rotation Degrees: 540° (tumble amount)
  - Rotation Speed: 720°/sec
  - Movement Ease: InQuad
  - Rotation Ease: OutQuad

## 3.2 Target Mechanics

### 3.2.1 Target Properties

| Property | Type | Description |
|----------|------|-------------|
| prefab | GameObject | Custom target prefab (optional) |
| orbitRadius | float | Distance from central pivot |
| startingAngle | float | Initial angular position |
| orbitDegreesPerSecond | float | Orbital movement speed |
| baseScale | float | Size multiplier |
| knivesToBreak | int | Knife hits required to destroy |
| rotation | TargetRotationConfig | Rotation behavior settings |

### 3.2.2 Rotation Behaviors

The game supports four distinct rotation behaviors:

#### Constant Rotation
- Simplest behavior
- Continuous rotation at fixed speed
- Configuration: `speed` (degrees/second)

#### Pulsating Rotation
- Speed oscillates sinusoidally
- Creates tension through variable timing
- Configuration:
  - `speed`: Base rotation speed
  - `amplitude`: Speed variation range
  - `frequency`: Oscillation rate (Hz)
- Formula: `actualSpeed = baseSpeed + sin(time * frequency * 2π) * amplitude`

#### Flipping Rotation
- Randomly reverses direction
- Unpredictable and challenging
- Configuration:
  - `speed`: Rotation speed
  - `flipInterval`: Time between flip checks
  - `flipProbability`: Chance to flip (0-1)

#### Acceleration Curve Rotation
- Speed follows animation curve
- Allows complex patterns (slow-fast-slow, etc.)
- Configuration:
  - `speed`: Base speed multiplier
  - `accelerationCurve`: AnimationCurve
  - `curveLoopDuration`: Cycle length in seconds

### 3.2.3 Multi-Target System

Levels can contain multiple targets:
- Each target tracked independently in `TargetGroupManager`
- Targets can orbit a central pivot point
- Each has independent:
  - Position (radius + angle)
  - Scale
  - Rotation behavior
  - Knife threshold
  - Pre-stuck knives
  - Hitable objects

### 3.2.4 Target Destruction

When a target reaches its `knivesToBreak` threshold:
1. `TargetHitTracker.RecordHit()` increments hit count
2. If `HitCount >= knivesToBreak`:
   - Target removed from tracking
   - `TargetGroupManager.DestroyTarget()` called
   - All tweens on target killed
   - GameObject destroyed
   - `LevelSession.MarkTargetCompleted()` called
   - Check if level is complete

## 3.3 Hitable Objects (Tomatoes)

### 3.3.1 Purpose
- Bonus collectible objects attached to targets
- Reward players with "fruits" currency
- Add visual interest and secondary objectives
- Create timing challenges (hit tomato while avoiding knives)

### 3.3.2 Properties

| Property | Type | Description |
|----------|------|-------------|
| config | HitableObjectConfig | Defines prefab and particles |
| count | int | Number to spawn around target |
| angularSpacing | float | Degrees between objects |
| orientOutwards | bool | Face away from target center |

### 3.3.3 Spawn Placement
- Positioned on target surface perimeter
- Uses `TargetGeometry.GetCenterAndRadius()` for positioning
- Respects `hitableObjectSpacingOffset` for angle adjustment
- Parents to target (rotates with target)

### 3.3.4 Hit Behavior
1. Knife collision detected (tag check for "Knife")
2. Mark as hit (prevent double-processing)
3. Disable colliders
4. Spawn particle effect at impact point
5. Raise `HitableObjectHitEventChannel` event
6. Destroy game object (if `destroyOnHit = true`)
7. Session records hitable hit
8. Player data adds fruit (+1)

### 3.3.5 Feedback
- Particle system plays (tomato juice splatter)
- Hit sound effect
- Fruit counter in HUD updates

## 3.4 Scoring System

### 3.4.1 Score Awards

| Action | Points | Notes |
|--------|--------|-------|
| Knife Stuck in Target | +100 | Per successful stick |
| Hit Tomato | 0 (score) | Awards fruit currency instead |

### 3.4.2 Score Service
- `ScoreService` listens to `KnifeImpactEventChannel`
- Adds score to `LevelSession` on each knife stuck event
- Score persists only within session (not saved)

### 3.4.3 Missing/Planned Features
- Combo system (consecutive hits without miss)
- Time bonus
- Perfect stage bonus
- High score tracking

## 3.5 Difficulty Mechanics

### 3.5.1 Difficulty Levers

| Parameter | Effect | Range |
|-----------|--------|-------|
| Rotation Speed | Faster = harder timing | 30-180+ deg/sec |
| Rotation Type | Pulsating/Flipping harder than Constant | 4 types |
| Pre-Stuck Knives | More = less safe angles | 0-8+ |
| Knife Count | Fewer = less margin for error | Minimum to break targets |
| Target Count | More = longer levels | 1-5+ |
| Knives to Break | More = longer target engagement | 3-15+ |
| Hitable Objects | Distraction/bonus challenge | 0-6+ |
| Orbit Movement | Moving targets harder | 0-60+ deg/sec |
| Target Scale | Smaller = harder | 0.5-1.5 |

### 3.5.2 Difficulty Progression Pattern
Typical progression within a stage:
1. Level 1: Slow constant rotation, few pre-stuck knives
2. Level 2: Slightly faster rotation
3. Level 3: Add tomatoes
4. Level 4: Increase pre-stuck knives
5. Level 5: Introduce pulsating/flipping rotation
6. Boss Level: Multiple targets, complex patterns

## 3.6 Special Mechanics (Identified Icons, Potentially Planned)

Based on art assets, these mechanics appear planned but may not be implemented:

### Shield (shield_icon.png)
- Potential: Protect from one knife collision
- Not found in codebase

### Star System (star_icon.png)
- `LevelProgressData` has star tracking
- `GetStars()` and `SetStars()` methods exist
- Not connected to gameplay evaluation
- Potential: 1-3 stars based on performance

### Freeze Target
- `RotatingTarget.Freeze()` exists
- `RotatingTarget.Resume()` exists
- Could be activated by special items

---

# 4. Levels & Stages

## 4.1 Stage/Level Hierarchy

```
Game
└── Stages (1, 2, 3, ...)
    ├── Stage 1: "Ancient Woods"
    │   ├── Level 1
    │   ├── Level 2
    │   ├── Level 3
    │   └── Level 4
    ├── Stage 2: "Mossfall Depths"
    │   ├── Level 1
    │   ├── Level 2
    │   └── ...
    └── ...
```

## 4.2 Stage Configuration

Each stage (`StageData`) contains:

| Field | Type | Description |
|-------|------|-------------|
| stageName | string | Display name for stage |
| layoutSettings | StageLayoutSettings | Visual theme |
| levels | List<LevelData> | Ordered level definitions |

### Stage Layout Settings
Each stage has distinct visual theming:
- `backgroundTexture`: Stage-specific background image
- `targetSprite`: Stage-specific wooden target appearance

### Identified Stages (from art folders)
1. Ancient Woods
2. Mossfall Depths
3. (Placeholder names in meta files:) Duskmoss Forest, Faded Wilds, Forgotten Grove, Lost Evergreen Realm, Mosswood Hollow, Old Timberlands, Shadowed Grove, Verdant Echoes

## 4.3 Level Configuration

Each level (`LevelData`) contains:

| Field | Type | Description |
|-------|------|-------------|
| knives | int | Total knives for level (0 = calculate from targets) |
| targetGroup | TargetGroupConfig | Target definitions |
| totalFruitsHit | int | UI display (deprecated field) |

### Knife Count Calculation
If `knives = 0`, automatically calculated:
```csharp
int total = 0;
foreach(target in targetGroup.targets)
    total += target.knivesToBreak;
return total;
```

This ensures exactly enough knives to complete the level with no margin for error (hard mode). Designers can add buffer by setting explicit knife count.

## 4.4 Target Group Configuration

Defines all targets in a level:

```csharp
TargetGroupConfig {
    List<TargetDefinition> targets;
}
```

Each `TargetDefinition` specifies:
- Custom prefab (or use default)
- Orbit position and movement
- Scale
- Rotation behavior
- Pre-stuck knife configuration
- Hitable object placements
- Destruction threshold

## 4.5 Level Flow

### Level Start Sequence
1. `LevelSessionService.StartLevelAsync()` called
2. Kill any existing knife tweens
3. Rebuild knife hierarchy (destroy old knives)
4. Apply rules (parse level data)
5. Apply stage layout (background)
6. Configure target(s) via `TargetGroupManager`
7. Configure thrower (set knife count)
8. Wait one frame
9. Spawn hitable objects
10. Spawn pre-stuck knives
11. Initialize target hit tracking
12. Apply target textures

### Level Complete Sequence
1. `TargetHitTracker` detects threshold reached
2. Target destroyed
3. `LevelSession.MarkTargetCompleted()` decrements remaining
4. `MarkCompleteIfNeeded()` checks if `TargetsRemaining <= 0`
5. If complete: `Completed` event fires
6. `LevelFlowCoordinator` catches event
7. Calls `NextLevelAsync()`
8. If last level in stage: advance to next stage

### Level Fail Sequence
1. Failure detected (knife collision or exhausted)
2. `LevelSession.RecordFailure()` called
3. `Failed` event fires
4. `LevelFlowCoordinator` catches event
5. Pauses game (`Time.timeScale = 0`)
6. Shows `StageFailPopup`
7. Player chooses: Retry or Restart Stage

## 4.6 Stage Progression

### Save Triggers
Progress saved when:
- Completing a level (current stage index persisted)
- Advancing to new stage (highest stage updated)

### Stage Unlocking
- Players must complete all levels in a stage to unlock next
- `highestStageIndex` tracks maximum unlocked stage
- `currentStageIndex` tracks where player left off
- On game start: loads `currentStageIndex` from save

### Replay Rules
- Players can restart current level (retry)
- Players can restart current stage (level 1)
- Currently no level select UI to replay arbitrary levels

## 4.7 Difficulty Scaling Guidelines

### Recommended Progression Curves

**Rotation Speed (degrees/second):**
- Stage 1: 60-90
- Stage 2: 75-110
- Stage 3: 90-130
- Stage 4+: 100-150+

**Pre-Stuck Knives:**
- Level 1 of any stage: 0-2
- Mid-stage levels: 2-4
- Final levels: 4-6

**Target Count:**
- Early stages: 1 target per level
- Mid stages: 1-2 targets
- Late stages: 2-3+ targets

**Rotation Type Distribution:**
- 60% Constant
- 20% Pulsating
- 15% Flipping
- 5% Acceleration Curve

---

# 5. Game Systems

## 5.1 Input System

### Architecture
- Uses Unity's new Input System package
- `InputActionThrowInput` component bridges actions to game
- `IThrowInput` interface for abstraction

### Input Events
| Event | Trigger | Game Response |
|-------|---------|---------------|
| Pressed | Touch/click down | (Currently unused) |
| Released | Touch/click up | Throw knife if ready |

### UI Blocking
- Before processing throw, raycasts UI elements
- If touch hits UI element (button), throw is suppressed
- Uses Unity's `EventSystem.RaycastAll()`

### Input Gating
Throw suppressed when:
- `canThrow = false`
- `isKnifeInFlight = true`
- No knives remaining in magazine
- Prefab/launcher not configured

## 5.2 Physics System

### Collider Setup
- Knives: 2D colliders (triggers and/or colliders)
- Targets: CircleCollider2D
- Hitable Objects: 2D colliders

### Layer/Tag Configuration
- "Knife" tag for knife detection
- "Target" tag for target detection
- Physics2D queries hit triggers enabled

### Collision Detection Approach
- `OnTriggerEnter2D` and `OnCollisionEnter2D` both used
- Searches parent hierarchy for components
- `HitableSearchRadius` (0.12 units) for nearby object detection

## 5.3 Object Pooling

### Knife Pool
- Pre-warms configurable number of knives (default: 10)
- Returns knives to pool after resolution
- Handles visual state reset (scale, color, colliders)
- VContainer injection for DI on pooled objects

### Pool Operations
- `Warmup(count)`: Pre-instantiate objects
- `GetOrCreate()`: Retrieve from pool or create
- `Return(knife)`: Deactivate and re-pool

## 5.4 Knife Magazine System

### Concept
Magazine manages the "next knife to throw" queue:
- Holds total knife count for level
- Animates knives loading into position
- Tracks remaining knives

### Load Animation
1. Knife positioned below launcher (offset configurable)
2. Color fades from transparent to original
3. Position tweens to launcher position
4. Duration and easing configurable

### Magazine State
```csharp
KnivesRemaining: int  // Decrements on throw
_nextKnife: GameObject  // Currently loaded knife
```

## 5.5 Session Management

### LevelSession
Central state container for current attempt:

| Property | Type | Description |
|----------|------|-------------|
| StageIndex | int | Current stage (1-based) |
| LevelIndex | int | Current level (1-based) |
| Rules | LevelRules | Parsed level parameters |
| KnivesStuck | int | Total knives in targets |
| KnivesThrown | int | Player throws made |
| IsComplete | bool | Level finished successfully |
| IsFailed | bool | Level failed |
| Score | int | Points accumulated |
| TargetCount | int | Total targets in level |
| TargetsRemaining | int | Targets not yet destroyed |
| HitableObjectsHit | int | Tomatoes collected |

### Session Events
- `SessionChanged`: Any state mutation
- `Completed`: Level successfully finished
- `Failed`: Level failed

## 5.6 Event System

### ScriptableObject Event Channels
Decoupled communication via SO-based events:

#### KnifeImpactEventChannel
- `KnifeStuck`: Knife successfully embedded
- `KnifeStuckAt(Vector3)`: With position for VFX

#### HitableObjectHitEventChannel
- `HitableHitAt(Vector3)`: Tomato destroyed

#### LevelCommandEventChannel
- `RetryRequested`: Replay current level
- `NextRequested`: Advance to next level
- `LevelRequested(int)`: Jump to specific level
- `ResetToStageStartRequested`: Restart stage

#### ButtonClickEventChannel
- Generic button click for UI sounds

## 5.7 Dependency Injection

### VContainer Integration
- `GameplayInstaller` extends `LifetimeScope`
- Registers all services, configs, and components
- Supports constructor injection and method injection
- `[Inject]` attribute for component injection

### Registered Services (Selection)
- `IAudioPlayer → AudioPlayer`
- `IMusicPlayer → MusicPlayer`
- `IPlayerDataStore → FilePlayerDataStore`
- `IPlayerDataService → PlayerDataService`
- `ILevelCatalog → StagesConfigCatalog`
- `IKnifeTracker → KnifeTracker`
- `ISettingsService → SettingsService`

## 5.8 Tween Management

### DOTween Usage
- Knife load animations
- Bounce-off animations
- Target hit shake/tint feedback
- UI animations

### Tween Cleanup
Critical for level transitions:
- `TweenKiller` utility class
- Kills tweens by target GameObject
- Recursive child cleanup
- Called before level reload

---

# 6. Meta & Progression

## 6.1 Persistence Architecture

### Save Data Structure
```csharp
PlayerData
├── PlayerSettingsData
│   ├── AudioSettingsData
│   │   └── isMuted: bool
│   ├── language: string
│   └── vibrationEnabled: bool
└── PlayerProgressData
    ├── currentStageIndex: int
    ├── highestStageIndex: int
    ├── totalFruitsHit: int
    └── LevelProgressData
        └── starsPerLevel: Dictionary<int, int>
```

### Storage Implementation
- JSON serialization via Unity's `JsonUtility`
- File: `playerData.json` in `Application.persistentDataPath`
- Atomic writes via temp file + replace

### Save Triggers
- Stage completion/advancement
- Settings changes (mute toggle)
- Fruit collection

## 6.2 Stage Unlocking

### Current Implementation
- Linear progression required
- Must complete all levels in stage N to unlock stage N+1
- `highestStageIndex` gates accessible content

### Potential Expansion
- Stage keys/currency to unlock stages
- Skip mechanics via rewarded ads
- Premium stage packs

## 6.3 Currency: Fruits

### Earning Fruits
- Hit hitable objects (tomatoes): +1 per hit
- Persistent across sessions
- Displayed in HUD

### Spending Fruits (Not Implemented)
Potential uses:
- Cosmetic knife skins
- Continue/retry without watching ad
- Unlock new stages early
- Power-up purchases

## 6.4 Star System (Partially Implemented)

### Data Structure
- `LevelProgressData.starsPerLevel` dictionary
- `GetStars(levelIndex)` / `SetStars(levelIndex, count)`

### Planned Functionality (Not Connected)
Star criteria could include:
- 1 Star: Complete level
- 2 Stars: Complete with X knives remaining
- 3 Stars: Complete + hit all tomatoes

## 6.5 Player Motivation Loop

```
┌─────────────────────────────────────────────────────────┐
│                 CORE LOOP (30 seconds)                  │
│  Tap → Throw → Satisfy (stick) OR Frustrate (fail)     │
│  Repeat until level complete/fail                       │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│              SESSION LOOP (5-15 minutes)                │
│  Complete level → See progress bar fill → Next level    │
│  Fail → Retry → Master pattern → Succeed                │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                META LOOP (days/weeks)                   │
│  Complete stage → Unlock new theme/backgrounds          │
│  Collect fruits → (Future) Unlock cosmetics             │
│  Beat high scores → Achievement dopamine                │
└─────────────────────────────────────────────────────────┘
```

---

# 7. UI/UX

## 7.1 Screen Inventory

### Current Screens
1. **Gameplay HUD** (main screen)
2. **Stage Fail Popup** (overlay)

### Potential/Needed Screens
- Main Menu
- Stage/Level Select
- Settings Menu
- Shop/Store
- Cosmetics Gallery
- Achievement/Stats Screen
- Tutorial/How to Play

## 7.2 Gameplay HUD

### Layout Elements

```
┌─────────────────────────────────────────┐
│  [Mute]          STAGE 1          [???] │  ← Top bar
│                                         │
│     [1] ─ ○ ─ ○ ─ ○ ─ ○ ─ [2]          │  ← Progress bar
│                                         │
│              🍅 125                      │  ← Fruit counter
│                                         │
│                                         │
│                                         │
│              ╭──────╮                   │
│             │ TARGET │ ← with knives    │  ← Gameplay area
│              ╰──────╯                   │
│                                         │
│                                         │
│                                         │
│              🔪 🔪 🔪 🔪 🔪              │  ← Knife magazine
│                                         │
│               [THROW]                   │  ← Launcher area
└─────────────────────────────────────────┘
```

### HUD Components

#### Stage Progress Bar (`StageProgressBarView`)
- Shows current stage number (left)
- Shows next stage number (right, if exists)
- Shows level progress slots between
- Completed levels: green/filled
- Incomplete levels: gray/empty

#### Knife Magazine Display (`KnifeMagazineDisplay`)
- Horizontal row of knife icons
- Available knives: full opacity
- Used knives: faded (25% opacity)
- Pooled icon system for performance
- Configurable colors and order

#### Fruit Counter (`FruitCountDisplay`)
- TextMeshPro display
- Shows `totalFruitsHit` from player data
- Updates when tomatoes hit

#### Mute Button
- Toggles all audio (music + SFX)
- Icon switches between muted/unmuted states
- Persists setting to save file

## 7.3 Stage Fail Popup

### Layout
```
┌─────────────────────────────────────────┐
│                                         │
│              STAGE FAILED               │
│                                         │
│              Stage 1                    │
│              Level 3                    │
│                                         │
│     ┌──────────────────────────┐       │
│     │      CONTINUE (AD)       │       │  ← Watch ad to retry
│     └──────────────────────────┘       │
│                                         │
│     ┌──────────────────────────┐       │
│     │    RESTART STAGE         │       │  ← Back to level 1
│     └──────────────────────────┘       │
│                                         │
└─────────────────────────────────────────┘
```

### Button Functions
- **Continue:** Calls `RaiseRetry()` → replays current level
- **Restart Stage:** Calls `RaiseResetToStageStart()` → level 1 of current stage

### Popup Behavior
- Inherits from `BasePopup`
- `Show()`: activates game object
- `Hide()`: deactivates game object
- Displays current stage/level numbers

## 7.4 Navigation Flow

```
Game Launch
    │
    ▼
Gameplay HUD (Level 1, Stage 1 or saved position)
    │
    ├── [Complete Level] ─────► Next Level (auto)
    │                               │
    │                               ├── [Complete Stage] ─► Next Stage Level 1
    │                               │
    │                               └── [Last Stage] ─► Replay last level?
    │
    └── [Fail] ─────────────────► Stage Fail Popup
                                      │
                                      ├── [Continue] ─► Retry Current Level
                                      │
                                      └── [Restart] ─► Stage Level 1
```

## 7.5 UI Technical Details

### Canvas Setup
- Screen Space - Overlay (assumed)
- Canvas Scaler for resolution independence
- TextMesh Pro for text rendering

### UI Blocking
Input system checks for UI hits before processing throws:
```csharp
EventSystem.current.RaycastAll(pointerData, results)
// If results.Count > 0, input is on UI, don't throw
```

### Animation
- DOTween for popup transitions (potential)
- Built-in Unity UI transitions for buttons

---

# 8. Audio & VFX

## 8.1 Sound Design

### Sound Assets

| Sound | File | Trigger | Notes |
|-------|------|---------|-------|
| Knife Throw | knife_throw.wav | On throw release | Whoosh/swish |
| Hit Target | hit_target_sound.wav | Knife sticks in wood | Thunk/wood impact |
| Hit Knife | knife_hit_knife_ogg.ogg | Knife-to-knife collision | Metal clang |
| Hit Tomato | hitable_fruit_sound.wav | Destroy tomato | Splat |
| Button Click | click_sound.wav | Any UI button | Click/tap |
| Knife Load | load_sound.wav | Knife enters launcher | Slide/click |
| Win | win_sound.wav | Level complete | (may not be implemented) |
| Lose | lose_sound.wav | Level fail | (may not be implemented) |

### Audio Implementation

#### AudioPlayer (SFX)
- Singleton `AudioSource` on persistent GameObject
- `PlayOneShot` for fire-and-forget sounds
- Global mute control
- Low-latency configuration (256 buffer size)

#### MusicPlayer (BGM)
- Separate persistent GameObject
- Looping background music
- Independent mute control
- Starts automatically on game launch

### Sound Configuration
`SoundSettings` ScriptableObject consolidates all audio clips:
- Easy designer adjustment without code changes
- Central reference point for all sounds

## 8.2 Visual Effects

### Particle Systems

#### Knife Hit Splinters (`KnifeHitParticles.prefab`)
- Triggered on successful target stick
- Wood debris/splinter particles
- Spawned at impact position
- Auto-destroy after particle lifetime

#### Tomato Hit Particles (`TomatoParticles.prefab`)
- Triggered on tomato destruction
- Juice/pulp splatter effect
- Spawned at impact position
- Parents to target (moves with rotation)

### Spawning System
`KnifeImpactParticlesSpawner`:
- Subscribes to `KnifeStuckAt` event
- Instantiates prefab at position
- Auto-destroys based on particle duration

### Materials
- `CloudsMaterial.mat`: Background clouds effect
- `SplinterMaterial.mat`: Wood particle material

## 8.3 Animation & Feedback

### Target Hit Animation (`TargetHitFeedbackPlayer`)

#### Shake Effect
| Parameter | Default | Description |
|-----------|---------|-------------|
| Duration | 0.25s | Length of shake |
| Strength | 15 units | Shake magnitude |
| Vibrato | 20 | Shake frequency |
| Randomness | 45° | Direction randomness |
| Fade Out | true | Diminish over time |

#### Tint Effect
| Parameter | Default | Description |
|-----------|---------|-------------|
| Color | Warm yellow | Flash color |
| Fade In | 0.05s | Time to reach tint |
| Hold | 0.05s | Time at full tint |
| Fade Out | 0.2s | Time to restore |
| Ease | OutQuad | Transition curve |

### Knife Animations

#### Load Animation
- Slides up from below launcher
- Fades in from transparent
- Duration: 0.3s (configurable)
- Ease: OutQuad

#### Bounce-Off Animation
- Activates on knife-to-knife collision
- Physics-like fall with rotation
- Configurable gravity, rotation speed
- Returns to pool after animation

## 8.4 Polish Guidelines

### Juice Principles Applied
1. **Immediate Feedback:** Every throw has instant audio/visual response
2. **Satisfying Success:** Wood thunk + shake + particles = visceral hit
3. **Clear Failure:** Metal clang + bouncing knife = unmistakable miss
4. **Continuous Motion:** Target always rotating, background scrolling
5. **Secondary Motion:** Attached knives/tomatoes rotate with target

### Areas for Enhancement
- Screen shake on fail
- Slow-motion on near-miss
- Celebration particles on level complete
- Streak/combo visual indicators
- Trail effect on flying knife
- Impact ripple on target

---

# 9. Monetization

## 9.1 Current State

The monetization system is **not implemented** in the codebase. However, UI elements suggest planned ad integration:
- `ad_icon(1).png` exists in assets
- "Continue" button in fail popup implies rewarded ad

## 9.2 Recommended Ad Strategy

### Rewarded Video Ads

#### Continue After Fail
- **Trigger:** Player fails level
- **Reward:** Retry current level (essentially a "second chance")
- **Placement:** Fail popup "Continue" button
- **Cap:** 1-2 per level attempt

#### Bonus Fruits
- **Trigger:** Level complete screen (future)
- **Reward:** 2x fruits collected in level
- **Placement:** Optional button on win screen
- **Cap:** Unlimited (player choice)

### Interstitial Ads

#### Between Levels
- **Trigger:** After completing a level
- **Frequency:** Every 3-5 levels
- **Skip:** During stage transitions (don't interrupt flow)
- **Cooldown:** Minimum 60 seconds between interstitials

#### After Retry
- **Trigger:** After retrying from fail popup
- **Frequency:** Every 2nd retry
- **Skip:** If rewarded ad was just watched

### Banner Ads

#### Persistent Banner
- **Position:** Bottom of screen (below knife magazine)
- **Type:** Adaptive banner
- **Disable:** During gameplay? (consider covering only)

## 9.3 Soft Currency Economy

### Fruit Economy Design

#### Earning Rates
| Source | Amount | Frequency |
|--------|--------|-----------|
| Tomato Hit | +1 | Per tomato |
| Daily Bonus | +10-50 | Once per day |
| Rewarded Ad | +20 | On demand |
| Stage Complete | +25-100 | Per stage |

#### Spending Sinks
| Item | Cost | Notes |
|------|------|-------|
| Continue | 50 | Retry without ad |
| Knife Skin | 200-1000 | Cosmetic |
| Target Skin | 500-2000 | Cosmetic |
| Skip Level | 100 | Unlock next level |
| Unlock Stage | 500 | Early access |

### IAP Opportunities

#### Fruit Bundles
- Small: $0.99 for 500 fruits
- Medium: $2.99 for 2000 fruits
- Large: $4.99 for 5000 fruits

#### Premium Features
- Remove Ads: $2.99 (removes interstitials/banners)
- VIP Pass: $4.99 (double fruits, exclusive skins)

#### Cosmetic Packs
- Knife Set: $1.99 (5 knife skins)
- Target Set: $1.99 (5 target skins)
- Theme Pack: $2.99 (background + target + knife)

## 9.4 Analytics Events to Track

| Event | Parameters | Purpose |
|-------|------------|---------|
| level_start | stage, level | Funnel analysis |
| level_complete | stage, level, knives_used, time | Difficulty tuning |
| level_fail | stage, level, fail_reason | Pain point identification |
| ad_shown | ad_type, placement | Revenue tracking |
| ad_completed | ad_type, placement | Engagement |
| ad_skipped | ad_type, placement | UX friction |
| fruit_earned | amount, source | Economy balance |
| fruit_spent | amount, item | Economy balance |
| iap_purchase | item, price | Revenue |

---

# 10. Technical Requirements

## 10.1 Platform Requirements

### Unity Version
- Current: Unity 2022+ (based on Input System usage)
- Target: Latest LTS recommended

### Target Devices
- iOS: iPhone 8+ (iOS 13+)
- Android: API level 24+ (Android 7.0+)
- RAM: 2GB minimum
- Storage: ~100MB

### Performance Targets
- Frame Rate: 60 FPS target
- Load Time: <3 seconds cold start
- Battery: <5% per 30-minute session

## 10.2 Architecture Overview

### Layer Structure
```
┌─────────────────────────────────────────────────────┐
│                  PRESENTATION                        │
│  UI Views, Gameplay Components, VFX, Audio Players  │
├─────────────────────────────────────────────────────┤
│                  APPLICATION                         │
│  Services: Knife, Level, Score, Settings            │
├─────────────────────────────────────────────────────┤
│                     CORE                             │
│  Domain: Session, Rules | Data: Configs | Events    │
├─────────────────────────────────────────────────────┤
│                 INFRASTRUCTURE                       │
│  Persistence, Audio Players, Level Spawners         │
└─────────────────────────────────────────────────────┘
```

### Namespace Organization
- `App.Services`: Application-level services
- `Core.Abstractions`: Interfaces/contracts
- `Core.Data`: ScriptableObject configs, data structures
- `Core.Domain`: Business logic (Session, Rules)
- `Core.Events`: Event channels
- `Infrastructure.Audio`: Audio implementation
- `Infrastructure.Leveling`: Level setup systems
- `Infrastructure.Persistence`: Save/load
- `Presentation.Gameplay`: Gameplay MonoBehaviours
- `Presentation.UI`: UI components

## 10.3 Data-Driven Configuration

### ScriptableObject Assets

| Asset Type | Purpose | Location |
|------------|---------|----------|
| StagesConfig | Level definitions | Configurations/Gameplay/ |
| StageLayoutSettings | Visual themes | Configurations/StagesLayout/ |
| HitableObjectConfig | Tomato definitions | Configurations/Gameplay/ |
| SoundSettings | Audio clips | Configurations/Audio/ |
| TargetHitAnimationConfig | Feedback params | Configurations/Animations/ |
| KnifeBounceOffAnimationConfig | Bounce params | Configurations/Animations/ |
| Event Channels | Decoupled events | Configurations/Events/ |
| ScoreTextConfiguration | Score UI settings | Configurations/UI/ |

### Config Best Practices
- All tuning values in ScriptableObjects
- No magic numbers in code
- Designers can adjust without code changes
- Version control friendly (separate assets)

## 10.4 Save System

### File Format
- JSON via `JsonUtility`
- Human-readable for debugging
- Located in platform-specific persistent path

### Save Strategy
- Atomic writes (temp file + replace)
- Load with fallback to defaults
- Data migration for version updates

### Data Integrity
- Null checks on all data sections
- Default value fallbacks
- Validation on load

## 10.5 Editor Tools

### Existing Tools

#### CheatsWindow (Editor/CheatsWindow.cs)
- Requires `TEST_ENABLED` define
- Restart level button
- Expandable for more debug options

#### StagesConfigEditor
- Custom inspector for stage configuration
- (Details in Editor scripts)

#### PlayerDataEditorWindow
- View/edit player save data
- Reset progress functionality

### Needed Tools
- Level preview in editor
- Rotation pattern visualizer
- Difficulty calculator
- Economy balance simulator

## 10.6 Build Configuration

### Defines
- `TEST_ENABLED`: Enables cheat window

### Build Profiles
- Located in Settings/Build Profiles/
- Platform-specific settings

## 10.7 Dependencies

### Packages
- Unity Input System
- TextMesh Pro
- DOTween
- VContainer (DI framework)
- UniTask (async/await for Unity)

### Third-Party Assets
- Bow and Hammer Sound Effects
- Casual Game Sounds U6
- Leohpaz (audio)
- Free Pack (audio)

---

# 11. Expansion & Scalability

## 11.1 Content Roadmap

### Phase 1: Core Polish (Current → +1 month)
- [ ] Complete all planned stages (8-10 stages)
- [ ] Implement star rating system
- [ ] Add level complete celebration
- [ ] Implement main menu
- [ ] Add settings screen

### Phase 2: Monetization (+1-2 months)
- [ ] Integrate rewarded video ads
- [ ] Integrate interstitial ads
- [ ] Implement fruit economy
- [ ] Add IAP for ad removal
- [ ] Add fruit bundle IAPs

### Phase 3: Engagement (+2-3 months)
- [ ] Daily challenges
- [ ] Achievement system
- [ ] Knife cosmetics shop
- [ ] Target cosmetics
- [ ] Daily login rewards

### Phase 4: Social (+3-4 months)
- [ ] Global leaderboards
- [ ] Friend challenges
- [ ] Share functionality
- [ ] Weekly tournaments

## 11.2 Feature Backlog

### Gameplay Features

#### Boss Levels
- Multi-phase targets
- Special mechanics per boss
- Unique visual themes

#### Special Knives
- Fire knife: burns through one extra knife
- Ice knife: freezes target briefly
- Split knife: splits into two on impact

#### Power-Ups
- Shield: survive one collision
- Slow-Mo: slow target rotation
- Magnet: pulls knife to safe spot
- Multi-Throw: throw 3 knives at once

#### Challenge Modes
- Time Attack: complete levels under time limit
- Endless: survive as long as possible
- No-Miss: fail on any collision
- Speed Run: complete stage fastest

### Content Features

#### New Themes (Stages)
- Desert/Canyon
- Ice/Snow
- Volcano/Lava
- Space/Sci-Fi
- Underwater
- Halloween/Seasonal
- Chinese New Year/Cultural

#### Cosmetics
- Knife skins (20+ variations)
- Target skins (per theme)
- Background variations
- Particle effect variations
- Sound packs

#### Events
- Limited-time stages
- Seasonal content
- Collaboration events
- Holiday themes

## 11.3 Technical Scalability

### Level System Scaling
Current architecture supports:
- Unlimited stages
- Unlimited levels per stage
- Easy content addition via ScriptableObjects
- No code changes for new content

### Performance Considerations
- Object pooling already implemented
- Tween cleanup on transitions
- Event-based decoupling
- Lazy initialization patterns

### Expansion Points
- `IRotationBehaviour`: Add new rotation types
- `HitableObjectConfig`: Add new object types
- Event channels: Add new events without touching existing code
- Services: Inject new services via DI

## 11.4 Live Operations Considerations

### Remote Config Needs
- Difficulty tuning without update
- Event scheduling
- A/B testing parameters
- Economy balancing

### Analytics Priorities
1. Level completion rates (identify hard levels)
2. Session length/frequency
3. Ad engagement/revenue
4. Retry patterns (failure points)
5. Fruit economy flow

### Update Cadence
- Weekly: Bug fixes
- Bi-weekly: Balance adjustments
- Monthly: New stages/content
- Quarterly: Major features

---

# 12. Refactor Plan

## 12.1 Architecture Improvements

### Separate Presentation from Logic
**Current State:** Some gameplay logic embedded in MonoBehaviours (e.g., Knife.cs has both movement and collision logic).

**Ideal State:**
- Pure C# classes for game logic
- MonoBehaviours only for Unity integration
- Testable business logic without Play mode

**Proposed Structure:**
```
KnifeController (pure C#)
├── Movement logic
├── Collision resolution logic
└── State machine

KnifeView (MonoBehaviour)
├── Visual updates
├── Physics bridge
└── Animation triggers
```

### State Machine for Level Flow
**Current State:** Flow controlled by events and flags scattered across services.

**Ideal State:**
- Explicit state machine
- Clear state transitions
- Easier debugging/logging

**Proposed States:**
```
LevelStateMachine
├── Loading
├── Playing
├── Paused
├── Completed
├── Failed
└── Transitioning
```

### Command Pattern for Actions
**Current State:** Direct method calls for level commands.

**Ideal State:**
- Command objects for all actions
- Undo capability (future)
- Replay/demo system support

## 12.2 Feature Module Organization

### Proposed Module Structure
```
Features/
├── Knife/
│   ├── KnifeController.cs
│   ├── KnifeView.cs
│   ├── KnifePool.cs
│   ├── KnifeMagazine.cs
│   └── KnifeConfig.cs
├── Target/
│   ├── TargetController.cs
│   ├── TargetView.cs
│   ├── RotationBehaviours/
│   └── TargetConfig.cs
├── HitableObjects/
│   ├── HitableController.cs
│   ├── HitableView.cs
│   └── HitableConfig.cs
├── Scoring/
│   ├── ScoreManager.cs
│   └── ComboTracker.cs
├── Progression/
│   ├── StageManager.cs
│   ├── LevelManager.cs
│   └── UnlockManager.cs
└── UI/
    ├── Screens/
    ├── Popups/
    ├── HUD/
    └── Common/
```

## 12.3 Data Architecture Improvements

### Separate Runtime from Authoring Data
**Current:** Same classes for config and runtime state.

**Proposed:**
```
LevelDefinition (ScriptableObject) - Designer authored
↓
LevelRuntimeData (runtime struct) - Game state
```

### Validation Layer
- Add validation on config load
- Editor-time validation tools
- Runtime fallbacks with logging

## 12.4 UI Architecture

### MVP/MVVM Pattern
**Current:** Direct binding in presenters.

**Proposed:**
```
Model: LevelSession, PlayerData
  ↓
ViewModel: LevelHudViewModel (already exists!)
  ↓
View: HUD components (binding only)
```

### Screen Management System
- Screen stack for navigation
- Transition animations
- History/back support

## 12.5 Event System Enhancements

### Typed Event Bus
- Generic event handling
- Event history for debugging
- Event filtering/routing

### Reduce ScriptableObject Events
- Keep for cross-scene communication
- Use C# events for same-scene
- Clearer lifetime management

## 12.6 Testing Infrastructure

### Unit Test Setup
- Separate assemblies for testability
- Mock interfaces for dependencies
- Test coverage for business logic

### Integration Tests
- Scene-based tests
- Automated level completion
- Regression detection

## 12.7 Performance Optimization Targets

### Memory
- Profile and reduce GC allocations
- Pool particle systems
- Optimize string operations (use StringBuilder)

### CPU
- Cache component references
- Batch UI updates
- Optimize rotation calculations

### Loading
- Addressables for asset loading
- Async scene loading
- Progressive content loading

---

# Appendix A: Glossary

| Term | Definition |
|------|------------|
| Stage | Collection of levels with shared visual theme |
| Level | Single gameplay challenge within a stage |
| Target | Rotating wooden disc that knives stick into |
| Knife | Player's projectile, thrown upward |
| Magazine | Visual queue showing remaining knives |
| Hitable Object | Bonus collectible attached to target (e.g., tomato) |
| Pre-Stuck Knife | Knife already in target at level start |
| Session | Single attempt at a level |
| Fruit | Soft currency earned by hitting hitables |
| Threshold | Number of knives required to destroy target |

# Appendix B: File Reference

### Key Scripts
| File | Purpose |
|------|---------|
| Knife.cs | Knife movement, collision, sticking logic |
| KnifeThrower.cs | Player input, throwing, magazine management |
| RotatingTarget.cs | Target rotation behavior |
| TargetGroupManager.cs | Multi-target management |
| TargetHitTracker.cs | Track hits per target, destruction |
| LevelSession.cs | Current attempt state |
| LevelSessionService.cs | Level lifecycle management |
| LevelFlowCoordinator.cs | Game flow orchestration |
| PlayerDataService.cs | Save/load player data |
| StagesConfig.cs | Level definitions data structure |

### Key Configurations
| File | Purpose |
|------|---------|
| StagesConfig1.asset | Main level definitions |
| SoundSettings.asset | Audio clip references |
| TargetHitAnimationConfig.asset | Hit feedback parameters |
| StageLayoutSettings *.asset | Per-stage visuals |

---

**END OF DOCUMENT**

*This GDD represents a complete analysis of the Knife Toss project codebase and serves as a foundation for future development, onboarding, and design decisions.*

