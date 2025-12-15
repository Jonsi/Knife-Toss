## Target Behavior Configuration Setup

Follow these steps to configure the new designer-friendly target behavior system. All assets live directly in level config data—no ScriptableObjects required.

### 1. Understanding the Structure
- `TargetConfig` (per level target) now exposes a `TargetBehaviorSet` with two lists: **World Behaviors** and **Local Behaviors**.
- Each behavior entry derives from `TargetBehaviorConfig` and implements either `ILocalBehaviorConfig` (local rotation/scale) or `IWorldBehaviorConfig` (world motion).
- Behaviors are serialized via `[SerializeReference]`, so you can mix different behavior types and toggle them individually.

### 2. Adding Behaviors to a Target
1. Open the level asset (e.g., `StagesConfig.asset`) and expand a target entry.
2. Expand either **World Behaviors** (for orbit/bobbing/etc.) or **Local Behaviors** (for spins/scales) and use the `+` icon to add a new entry.
3. Use the **Behavior Kind** dropdown on each entry to pick the desired type (world list currently offers `Orbit`; local list offers `ConstantSpin`, `PulsatingSpin`, `FlippingSpin`, `CurveSpin`).
4. The inspector automatically swaps in the corresponding configuration fields for that kind. Adjust the parameters (speed, amplitudes, angles, etc.) as needed. For `Orbit`, set **Distance From Center** to define how far the target orbits from the center point.
5. Use the checkbox on each entry to enable/disable it without removing the data.

### 3. Stacking Local and World Behaviors
- Add as many entries as you need in each list. Example: one `Orbit` behavior in the world list + two different spin behaviors in the local list for layered motion.
- World behaviors evaluate first, updating the target position; local behaviors then adjust rotation/scale around that position.

### 4. Initial Rotation and Scale
- `InitialRotation` still controls the starting local angle before behaviors tick.
- `BaseScale` and `KnivesToBreak` remain on `TargetConfig` and behave as before.

### 5. Best Practices
- Keep related behaviors grouped (world first, then local) for readability.
- Avoid redundant behaviors (e.g., two constant spins) unless you intentionally offset them.
- When cloning targets, verify each behavior entry still references the desired config type; Unity sometimes defaults to the last used type for new list items.

### 6. Troubleshooting
- If a target appears static, confirm at least one behavior entry is enabled.
- If orbit paths look wrong, double-check `Radius`, `StartingAngle`, and `DegreesPerSecond` on the `OrbitBehaviorConfig`.
- For issues during serialization, re-expand the behavior list and ensure no entry displays `Missing Type`. Replace it with a valid config if needed.

With this workflow designers can toggle, reorder, and combine behaviors quickly without touching code. Feel free to add new behavior config classes under `Assets/Scripts/Game/Entities/Target/Data/Behaviors` following the same pattern if new motion types are needed.
