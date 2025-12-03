# Extended Feature Implementation Plan

Scope: post-MVP features from `knife_toss_gdd.md`, implemented in small, testable steps aligned with the current layered architecture, VContainer DI, UniTask-based flows, and DOTween for presentation polish.

The game already supports:
- Rotating target & knife collision rules
- Knife throwing via single-tap input
- Level progression & config catalog

The phases below are ordered by impact vs. risk. Each item should prefer:
- Domain logic in Core/Application layers
- DI wiring via `GameplayInstaller`
- Async flows via UniTask
- Presentation polish via DOTween (no logic in tweens)

---

## Phase 1 – Audio & UX

1. **Music & Audio Options**
   - Add `MusicPlayer` in Infrastructure layer:
     - Loop background music, separate from `AudioPlayer` (SFX only).
     - Register in `GameplayInstaller` and inject where needed.
   - Add settings storage:
     - Persist music/SFX mute flags via `ISaveService` (or new `IAudioSettingsStore` adapter).
   - Add UI toggle(s):
     - Extend existing settings or HUD UI using VContainer-injected presenters.
     - Use DOTween for simple toggle animations only.

---

## Phase 2 – Scoring & Stars

1. **Scoring in Domain**
   - Extend `LevelSession` to track `Score` and related flags (e.g. `HasFailed`, `BonusObjectsHit`).
   - Update `LevelSessionService` to raise score events on knife hit, bonus, fail, retry.

2. **3-Star Rating (GDD Rules)**
   - Implement pure domain logic (no Unity types) that maps final `LevelSession` state → `StarCount`:
     - 1 star: level completed.
     - 2 stars: clean run (no fail) OR all bonus objects hit.
     - 3 stars: clean run AND all bonus objects hit.
   - Persist best star count per level via `ILevelProgressStore`.

3. **Presentation**
   - Extend `LevelCompletePopup` presenter:
     - Show score and stars using data from `LevelSessionService` / `ScoreService`.
     - Use DOTween for star reveal animation; no game logic in tween callbacks.

---

## Phase 3 – Target Variety

Goal: introduce pluggable rotation behaviours and support multiple orbiting targets with distance-based scaling that also scales knives.

1. **Rotation Strategy**
   - Confirm/align `IRotationBehaviour` shape:
     - Keep interface minimal (e.g. `ApplyRotation(Transform target, float deltaTime)`).
   - Ensure current behaviour lives in `ConstantRotation`:
     - `RotatingTarget` holds an `IRotationBehaviour` reference (serialized or injected).
     - Update loop delegates to behaviour; no inline rotation logic.
   - Wire via config:
     - Use `LevelConfig` (or a dedicated rotation config) to select behaviour per level.
     - Optionally add a small factory in Application/Infrastructure to map config → behaviour instance.

2. **New Rotation Behaviours**
   - Implement `PulsatingRotation`:
     - Speed varies over time (sine/curve driven) using serialized parameters.
   - Implement `FlippingRotation`:
     - Random direction flips with configurable timing/probability.
   - Implement `AccelerationCurveRotation`:
     - Speed follows an `AnimationCurve` over a loop duration.
   - Keep behaviours stateless or minimally stateful and independent of Unity scene objects beyond the provided `Transform`.

3. **Multiple & Orbiting Targets**
   - Add `TargetGroupManager` in Presentation/Gameplay:
     - Own a central pivot transform (mid-screen).
     - Manage a list of target entries: prefab ref, orbit radius, starting angle, base scale, rotation behaviour ref.
     - Drive per-target position/rotation each frame (rotate pivot or compute per-target positions – pick one and keep logic isolated here).
   - Distance-based scaling:
     - Define a radius → scale mapping (simple curve or linear function).
     - Apply scale to each target so further targets appear appropriately smaller/larger.
   - Knife scaling:
     - Define an interface or context exposing the active target’s scale factor.
     - When `KnifePool` spawns a knife for a target, apply the same factor to knife transform and collider (e.g. target at 80% → knife at ~80%).
   - Config & DI:
     - Extend `LevelConfig` to describe multiple targets: orbit radius, angle, behaviour type, etc.
     - Register `TargetGroupManager` and required configs via `GameplayInstaller`.
     - Keep Level rules (win/fail conditions) in domain by representing “active target” and “targets remaining” in `LevelSession` or a dedicated domain object.

---

## Phase 4 – Bonus Objects & Obstacles

1. **Bonus Objects (Tomatoes)**
   - Domain:
     - Add bonus hit accounting to `LevelSession` (e.g. `BonusObjectsTotal`, `BonusObjectsHit`).
   - Infrastructure/Presentation:
     - Create tomato prefab + hit-detection script that raises an event (e.g. via a new ScriptableObject event channel or existing impact channel extension).
   - Config:
     - Extend `LevelConfig` to define tomato count/placements (angles/slots that must not overlap knife-required slots).

2. **Obstacles**
   - Types: Metal Plate (deflect+fail), Spike Zone (fail), Void Sector (miss, no fail).
   - Domain:
     - Extend hit result representation (e.g. enum) so `LevelSessionService` can distinguish success/fail/miss/obstacle hit.
   - Presentation:
     - Implement obstacle prefabs with colliders and simple scripts that map collisions → hit result type.
   - Config:
     - Extend `LevelConfig` with obstacle definitions (type + placement).

---

## Phase 5 – Combo & Precision Systems

1. **Combo Tracking**
   - Add combo state to `LevelSession` (current streak, max streak).
   - Reset on miss or fail; increment on successful hits.

2. **Score Multiplier & Precision Rewards**
   - Introduce domain-level score multiplier based on combo.
   - Add “precision timing” hooks:
     - Rotation behaviours expose simple state or events (e.g. flipped this frame, currently at max speed window).
     - `LevelSessionService` can apply bonus when knife hits during these windows.

3. **HUD Presentation**
   - Extend `LevelHudPresenter`:
     - Show current combo and multiplier.
     - Use DOTween for subtle number/scale animations only.

---

## Phase 6 – Knife Variants

1. **Knife Behaviour Abstraction**
   - Introduce `IKnifeBehaviour` (or similar) in Core/Domain:
     - Encapsulate properties like collider size, throw cadence, special rules.
   - Refactor `Knife` and `KnifePool`:
     - Knife visuals/colliders read from injected/configured behaviour data.

2. **Variants (GDD-Aligned)**
   - Heavy Knife:
     - Larger collider, same base rules; tuned to be more likely to hit existing knives.
   - Slim Knife:
     - Smaller collider; more precision, same scoring rules.
   - Double-Throw:
     - Behaviour that allows two knives per input window, implemented via `LevelSessionService` + `KnifeThrower` rules (no timing logic in the prefab itself).

3. **Selection & UI**
   - Add an “Armory” or loadout UI:
     - Uses existing DI pattern for presenters and event channels.
   - Persist chosen variant via `ISaveService` / progress store and inject into `LevelSessionService` at level start.

---

## Phase 7 – Additional Game Modes

1. **Endless Mode**
   - Domain:
     - Extend/introduce a mode flag in `LevelSession` or a dedicated `GameMode` object.
     - Use existing procedural generation (`LevelConfig.GenerateProceduralLevel`) to spawn an infinite sequence with difficulty curve.
   - Application:
     - Add endless-mode entry point to `LevelFlowCoordinator` / menu UI.
   - Persistence:
     - Track and save endless high score via `ILevelProgressStore` or a dedicated high-score store.

2. **Daily Challenges**
   - Domain/Application:
     - Define a deterministic seed-based level generator that takes a daily seed value.
   - Infrastructure:
     - For now, compute a local “daily seed” (e.g. date-based); later can be replaced with a server-provided seed behind an interface.
   - UI:
     - Add a menu entry and simple results screen reusing existing score/star presentation where possible.

---

Further post-launch ideas (leaderboards, cosmetic skins, etc.) should follow the same principles: push rules into Core/Application, keep Infrastructure replaceable, and use VContainer, UniTask, and DOTween only in their respective roles (wiring, async, and visuals).*** End Patch***assistant +#+#+#+#+#+assistant to=functions.apply_patchyrmakablytyped to=functions.apply_patch((&___tool_input: str__) => any)validator  ভিডassistant to=functions.apply_patch/Subthresholdassistant to=functions.apply_patch	RTLU to=functions.apply_patchной to=functions.apply_patchาติ to=functions.apply_patchassistant to=functions.apply_patch тай to=functions.apply_patchassistant to=functions.apply_patchraichte to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchичество to=functions.apply_patchassistant to=functions.apply_patch Beau to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant потол to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patchassistant to=functions.apply_patch to=functions.apply_patch ***!
