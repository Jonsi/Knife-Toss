
---

# Phase 4 Setup Instructions

## 1. Create StagesConfig Asset
1. In Unity, right-click in `Assets/Configurations/Gameplay/`
2. Select: `Create > Knife Toss > Gameplay > Stages Config`
3. Name it `StagesConfig`
4. Configure the asset:
   - Add a Stage (click +)
   - Add a Level to the stage
   - Set `Knife Count` (e.g., 10)
   - Add a Target to the level
   - Configure Target properties (Prefab Override, Orbit Radius, etc.)

## 2. Setup Level Root
1. In the scene, create an empty GameObject named `LevelRoot`
2. Reset its Transform (Position: 0, 0, 0)
3. This object will parent all spawned targets

## 3. Update GameInstaller
1. Select the `GameplayInstaller` GameObject in the scene
2. In the Inspector, assign:
   - `StagesConfig` asset → `Stages Config` field
   - `LevelRoot` GameObject → `Level Root` field
   - `Target` prefab (from Project window) → `Default Target Prefab` field
3. **Important**: The old `Target Config`, `Target View`, and `Level Rules Config` fields have been removed. If you see "Missing Script" references or old serialized data, you can safely ignore or reset the component.

## 4. Cleanup Scene and Assets
1. Delete the old `TargetView` GameObject from the scene (targets are now spawned dynamically)
2. Delete the old config assets if they exist:
   - `Assets/Configurations/Gameplay/TargetConfig.asset`
   - `Assets/Configurations/Gameplay/LevelRulesConfig.asset`

## 5. Test Phase 4
- Enter Play Mode
- The game should load the first level of the first stage defined in `StagesConfig`
- Target(s) should spawn at configured positions
- Hitting a target tracks hits
- Destroying all targets (hits >= KnivesToBreak) should trigger Level Completion
- Running out of knives triggers Level Failed
