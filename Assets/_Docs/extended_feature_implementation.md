# Extended Feature Implementation Plan

## Purpose
This document outlines the recommended order of implementation for the post-MVP features described in `knife_toss_gdd.md`, along with any required architectural refactors to keep the project maintainable and test-friendly.

The game already supports:
* Rotating target & knife collision rules
* Knife throwing via single-tap input
* Level progression & config catalog

The plan below therefore focuses on additive features and technical debt reduction.

---

## Existing Implementation Survey (Dec 2025)

| Area | Status | Key Classes / Assets |
|------|--------|----------------------|
| **Rotating Behaviour** | Implemented (constant speed + random flip) | `RotatingTarget.cs` (Update loop, direction flip) |
| **Knife Collision / Tracking** | Implemented | `KnifeTracker.cs`, `KnifeImpactEventChannel.cs` |
| **Particles on Knife Hit** | Implemented | `KnifeImpactParticlesSpawner.cs` |
| **Fail Screen & Retry Animation** | Implemented | `FailFeedbackPresenter.cs`, `LevelHudPresenter.cs` |
| **Audio SFX plumbing** | Core `AudioPlayer.cs`; Button click & knife impact channel wired, but **no music or mute toggle** |
| **Procedural Levels** | Implemented | `LevelConfig.cs` – `GenerateProceduralLevel()` |
| **HUD Knife Counter Animation** | Implemented | `LevelHudPresenter.cs` + TMP counter |

> We will treat the rows above as DONE unless future polish is required.

---

## Phase 1 – Quality-of-Life & UX Polishing

This phase focuses on foundational features that significantly improve the player experience.

1. **Music & Audio Options:**
   - **Task:** Implement a dedicated `MusicPlayer` service that handles looping background music. This keeps music management separate from the existing `AudioPlayer`, which is designed for one-shot sound effects.
   - **Task:** Add a global mute/unmute toggle in the UI. The state of this toggle will be persisted using the existing `ISaveService` so that the player's preference is saved across sessions.
   - **Rationale:** A good audio experience is crucial for player engagement. Separating music and SFX players follows the Single Responsibility Principle, making the audio system more robust and easier to manage.

---

## Phase 2 – Scoring & Stars

This phase introduces a scoring system to provide players with performance feedback and replayability, directly based on the GDD's specifications.

1. **Track Score:**
   - **Task:** Extend the `LevelSession` domain object to include a `score` property. This will be the central point for tracking all score-related events.
   - **Rationale:** Centralizing session-specific data within `LevelSession` keeps our domain logic clean and decoupled from Unity's presentation layer.
2. **Implement 3-Star Rating (GDD-Aligned):**
   - **Task:** Implement a system to award stars based on the following GDD criteria:
     - **1 Star:** Awarded for basic level completion.
     - **2 Stars:** Awarded for a "Clean Run" (completing the level without failing and retrying) OR hitting all bonus objects in the level.
     - **3 Stars:** Awarded for a "Perfect Run" (completing the level without failing AND hitting all bonus objects).
   - **Task:** The star rating for each level should be saved using the `ILevelProgressStore`.
   - **Rationale:** A star system with clear, defined goals provides players with concrete challenges and encourages them to replay levels to achieve mastery.
3. **Display Stars:**
   - **Task:** Update the `LevelCompletePopup` to display the final score and the stars awarded.
   - **Rationale:** This provides immediate visual feedback to the player on their performance.

---

## Phase 3 – Target Variety

To keep gameplay engaging, this phase focuses on introducing more varied and challenging target behaviors as outlined in the GDD.

1. **Refactor Rotation Logic (SOLID):**
   - **Task:** Create an `IRotationBehaviour` interface and move the existing rotation logic into a `ConstantRotation` class that implements this interface (Strategy Pattern).
   - **Task:** Modify `RotatingTarget.cs` to hold a reference to an `IRotationBehaviour`, delegating the rotation logic to it.
   - **Rationale:** This refactoring adheres to the **Open/Closed Principle**, allowing us to add new rotation behaviors without modifying the `RotatingTarget` class.
2. **Implement GDD Target Behaviors:**
   - **Task:** Create new classes that implement `IRotationBehaviour` for:
     - `PulsatingRotation` (speed varies over time).
     - `FlippingRotation` (randomly changes direction mid-rotation).
     - `JitterWobbleRotation` (introduces a slight, unpredictable wobble).
     - `AccelerationCurveRotation` (rotation speed follows a predefined curve).
   - **Task:** Extend `LevelConfig` to allow designers to select and configure a rotation behavior for each level.
   - **Rationale:** These new behaviors, taken directly from the GDD, will increase the variety and challenge of the gameplay.
3. **Support for Multiple & Orbiting Targets:**
   - **Task:** This is a significant architectural change. A new manager class, perhaps a `TargetGroupManager`, will be needed to coordinate multiple targets.
   - **Task:** Levels with multiple targets will need to define which target is currently "active" for knife throws.
   - **Task:** `LevelConfig` will need to be extended to support defining multiple target prefabs, their positions, and their rotation behaviors.
   - **Rationale:** This feature dramatically expands design possibilities, allowing for much more complex and interesting level layouts.

---

## Phase 4 – Bonus Objects & Obstacles

This phase adds new interactive elements to the targets to create more dynamic gameplay scenarios, as specified in the GDD.

1. **Bonus Objects (Tomatoes):**
   - **Task:** Create a "Tomato" prefab with a script that detects hits. When hit, it should grant bonus points and trigger a visual/audio effect.
   - **Task:** **Crucially**, the system for placing tomatoes must ensure they **do not block required knife placement slots**, as per the GDD. They should be placed in the empty spaces on the target.
   - **Task:** Extend `LevelConfig` to allow designers to specify the number and placement of bonus objects.
   - **Rationale:** Bonus objects add an extra layer of skill-based scoring, rewarding precision.
2. **Obstacles (GDD-Inspired):**
   - **Task:** Implement the following obstacle types from the GDD:
     - **Metal Plates:** Deflect knives and cause a fail state.
     - **Spike Zones:** Small areas on the target that cause a fail state if hit.
     - **Void/No-Hit Sectors:** Sections of the target that are "intangible," causing the knife to pass through and resulting in a miss (but not a fail).
   - **Task:** The `Knife` projectile's collision logic will need to be updated to handle these new object types and their unique behaviors.
   - **Task:** Extend `LevelConfig` to define obstacle placements on the target.
   - **Rationale:** These obstacles introduce new challenges and require players to be more strategic and precise with their throws.

---

## Phase 5 – Advanced Gameplay Mechanics

This phase introduces a combo system to reward skilled play and add more depth to the scoring mechanics.

1. **Combo System:**
   - **Task:** Implement a combo tracker in `LevelSession` to count consecutive hits. The combo resets on a miss or fail.
   - **Task:** Implement a score multiplier that increases with the combo streak.
   - **Task (GDD-Inspired):** Add "Precision-Timing Rewards". For example, grant a larger score bonus if a knife is thrown immediately after a target flips direction or during its fastest rotation phase.
   - **Task:** The `LevelHudPresenter` will be updated to display the current combo count and multiplier.
   - **Rationale:** A combo system with precision rewards adds an exciting "high-risk, high-reward" element, encouraging skillful and daring play.

---

## Phase 6 – Gameplay-Modifying Knife Variants

This phase introduces different types of knives that alter gameplay, as suggested in the GDD.

1. **Knife Variant System:**
   - **Task:** Refactor the `Knife` prefab and scripts to support different knife properties. An `IKnifeBehaviour` or similar strategy pattern could be used.
   - **Task:** Implement the following GDD knife variants:
     - **Heavy Knife:** Has a larger collider, making it easier to hit the target but also easier to hit other knives.
     - **Slim Knife:** Has a smaller collider, requiring more precision.
     - **Double-Throw:** A power-up or knife type that allows two knives to be thrown in quick succession.
   - **Task:** A system for selecting and equipping knives will be needed, likely in a new "Armory" UI screen.
   - **Rationale:** Knife variants add a layer of strategy and player choice, increasing long-term engagement.

---

## Phase 7 – Additional Game Modes

This phase focuses on adding new game modes to extend the life of the game, as outlined in the GDD's "Post-Launch Enhancements".

1. **Endless Mode:**
   - **Task:** Create a new game mode where the player faces a never-ending sequence of procedurally generated targets. The difficulty would increase over time.
   - **Task:** This mode will require a separate high-score tracking system.
   - **Rationale:** Endless modes are excellent for retention, providing a long-term challenge for dedicated players.
2. **Daily Challenges:**
   - **Task:** Implement a system that generates a unique, pre-seeded challenge level each day.
   - **Task:** This will require a server-side component to ensure all players get the same challenge.
   - **Rationale:** Daily challenges encourage players to return to the game regularly.

---

*Further phases, including leaderboards and knife skins, to be planned after the above are implemented.*
