# Level Difficulty Profiles

This document defines the difficulty profiles for "Knife Toss" levels. These profiles act as guidelines for creating `LevelConfig` and `TargetConfig` assets to ensure a consistent progression curve.

## Key Configuration Parameters

### Level Parameters (`LevelConfig`)
*   **Knives To Throw**: Total knives required to complete the level (derived from Target HP).
*   **Extra Knives**: Allowed misses (usually 0 for standard play, maybe 1-2 for tutorial).

### Target Parameters (`TargetConfig`)
*   **HP (KnivesToBreak)**: How many hits the target takes.
*   **Scale (`BaseScale`)**: Size of the target.
    *   `1.0`: Standard
    *   `< 1.0`: Harder (smaller hit area)
    *   `> 1.0`: Easier (usually) or Boss (large but crowded)
*   **Obstacles (`PreStuckKnives`)**: Number and arrangement of pre-existing knives.
    *   *Values*: List of angles (e.g., `[0, 90, 180, 270]`).

### Rotation Behaviors (`ILocalBehaviorConfig`)

#### 1. Constant Spin (`ConstantSpinBehaviorConfig`)
*   **Speed**: Rotation speed in degrees per second.
    *   Positive = Clockwise
    *   Negative = Counter-Clockwise

#### 2. Flipping Spin (`FlippingSpinBehaviorConfig`)
*   **Base Speed**: Speed during rotation.
*   **Interval (`Min`/`Max`)**: Random time between direction flips.

#### 3. Curve Spin (`CurveSpinBehaviorConfig`)
*   **Curve**: AnimationCurve defining speed over time (allows for Stop & Go, Acceleration, etc.).

---

## Difficulty Profiles

### 1. Tutorial / Very Easy (Stage 1: Levels 1-3)
*   **Goal**: Introduce mechanics.
*   **Behavior**: `Constant Spin`
*   **Speed**: `30 - 50` (Very predictable)
*   **HP**: `3 - 5`
*   **Obstacles**: `0`
*   **Scale**: `1.0`
*   **Notes**: No surprises. Pure aim practice.

### 2. Easy (Stage 1: Levels 4-10, Stage 2: Early)
*   **Goal**: Basic challenge.
*   **Behavior**: `Constant Spin` (Faster) or `Flipping Spin` (Lazy)
*   **Speed**: `60 - 90`
*   **HP**: `6 - 8`
*   **Obstacles**: `0 - 2` (Widely spaced)
*   **Scale**: `1.0`
*   **Notes**: Introduction of obstacles requiring timing.

### 3. Normal (Mid-Game)
*   **Goal**: Rhythm disruption.
*   **Behavior**: `Flipping Spin`
*   **Speed**: `90 - 120`
*   **Flip Interval**: `2.0s - 4.0s` (Predictable flips)
*   **HP**: `8 - 12`
*   **Obstacles**: `2 - 3` (Asymmetrical placement)
*   **Scale**: `0.9 - 1.0`
*   **Notes**: Player must anticipate direction changes or wait for a safe opening.

### 4. Hard (Late Game / Challenge Levels)
*   **Goal**: Precision and Reflexes.
*   **Behavior**: `Flipping Spin` (Aggressive) or `Curve Spin` (Stop & Go)
*   **Speed**: `120 - 180`
*   **Flip Interval**: `0.5s - 1.5s` (Frequent changes)
*   **HP**: `10 - 15`
*   **Obstacles**: `3 - 5` (Tight clusters)
*   **Scale**: `0.8 - 0.9` (Small targets)
*   **Notes**: "Stop & Go" patterns where the target pauses briefly, baiting a throw, then spins rapidly.

### 5. Insane / Boss
*   **Goal**: Pattern memorization and perfect execution.
*   **Behavior**: `Curve Spin` (Complex)
*   **Speed**: Variable (0 to 300+ bursts)
*   **HP**: `15 - 25`
*   **Obstacles**: `5 - 8`
*   **Scale**: Variable (`0.7` tiny or `1.2` massive wall)
*   **Notes**:
    *   **The "Wall"**: Large target, high HP, many obstacles.
    *   **The "Gnat"**: Tiny target, super fast spin.
    *   **The "Trickster"**: Slow spin that suddenly accelerates 3x when a knife is thrown (requires anticipation).

---

## Configuration Constraints & Safety Limits

To ensure playable and valid levels, adhere to the following constraints when configuring `LevelConfig` and `TargetConfig`.

### 1. Target Health & Capacity (Physical Limits)
The target has a limited circumference. Attempting to stick too many knives will result in unavoidable collisions (impossible levels).

*   **Max Capacity Rule**: `(KnivesToBreak + PreStuckKnives.Count) <= MaxCapacity`
*   **Capacity by Scale**:
    *   Scale `1.0`: ~25 Knives Max
    *   Scale `0.8`: ~18 Knives Max
    *   Scale `0.6`: ~12 Knives Max
    *   Scale `1.2`: ~30 Knives Max
*   **Recommendation**: Keep total knife count at least **20% below** max capacity for "Fair" difficulty.

### 2. Spatial Boundaries (Screen Space)
Targets must remain visible and reachable. Assumes standard Portrait aspect ratio (9:16).

*   **Position Offset (`InitialOffset`)**:
    *   **X Range**: `-2.2` to `2.2` (Horizontal bounds)
    *   **Y Range**: `-1.0` to `3.5` (Vertical bounds; keep above thrower, below UI)
*   **Scale Limits**:
    *   **Min Scale**: `0.5` (Hitbox becomes pixel-perfect difficult)
    *   **Max Scale**: `1.5` (Begins to obscure UI or clip screen edges)

### 3. Obstacle Placement (Pre-Stuck Knives)
*   **Angle Separation**: Maintain at least **15 degrees** between pre-stuck knives.
*   **Safe Zones**: Ensure there are gaps large enough for the player to land `KnivesToBreak` (e.g., if player needs to throw 5 knives, don't block 90% of the circle).

### 4. Rotation Speed
*   **Playable Cap**: `±360` degrees/second (1 full rotation per second) is extremely fast.
*   **Standard Range**: `45` to `180` degrees/second.
*   **Human Reaction Limit**: Speeds > `200` require anticipation rather than reaction.

---

## Progression Logic (Draft)

1.  **Stage 1 (Forest)**: Profiles 1 & 2. mostly Constant Speed.
2.  **Stage 2 (Desert)**: Introduce Flipping.
3.  **Stage 3 (Ice)**: Introduce Variable Speed (Curves/Slippery feel).
4.  **Stage 4 (Space)**: Small targets, erratic movement.

