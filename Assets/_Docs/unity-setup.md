# Unity Project Setup Guide

Follow these steps to run Knife Toss in Unity after pulling the repository.

## 1. Open the Project
1. Launch Unity Hub and click **Open**.
2. Select the project root (`Knife-Toss` folder). The project was authored with the New Input System enabled; Unity 2021.3 LTS or newer is recommended.

## 2. Configure ScriptableObject Assets
- Ensure a `LevelConfig` asset exists (default path `Assets/LevelConfig.asset`). You can regenerate it via **Knife Toss → Generate Level Config** in the Unity editor if needed.
- Create a `KnifeImpactEventChannel` asset via **Create → Knife Toss → Events → Knife Impact Channel** and place it where convenient (e.g., `Assets/Settings/KnifeImpactEventChannel.asset`).

## 3. Scene Wiring
1. Open your gameplay scene (e.g., `Assets/Scenes/Main.unity`).
2. On an empty GameObject (e.g., `Systems`), add the `GameplayInstaller` component.
   - Assign `LevelConfig`, the `KnifeImpactEventChannel` asset, and the starting level.
   - Drag references for `RotatingTarget`, `KnifeThrower`, the target `Transform`, and the knife prefab.
3. Add `LevelLifecyclePresenter` to a UI controller object and assign:
   - The same `GameplayInstaller` instance.
   - `LevelHudPresenter` (TextMeshPro labels for level/knives left).
   - `GameStatePresenter` (fail text + next-level button).
4. Add `KnifeEventRelay` anywhere convenient and reference the installer plus the event channel (leave blank to auto-fill).
5. Ensure the `KnifeThrower` GameObject also has `KnifeThrowRelay` so throw events reach the session service.
6. Each knife prefab must reference the `KnifeImpactEventChannel` asset on its `Knife` component.

## 4. UI Hookups
- Wire your retry/next buttons to call `LevelLifecyclePresenter.RequestRetryLevel()` and `LevelLifecyclePresenter.RequestNextLevel()` respectively.
- Confirm the TMP fields referenced by `LevelHudPresenter` and `GameStatePresenter` point to the correct UI elements.

## 5. Input System
- The project uses the Input System package. Ensure **Edit → Project Settings → Player → Active Input Handling** includes **Input System Package** (or **Both**).
- Assign the `InputActionAsset` (e.g., `InputSystem_Actions.inputactions`) to the `KnifeThrower` component.

## 6. Play Mode Checklist
1. Enter Play Mode.
2. Verify the installer logs any missing references; fix them if necessary.
3. Confirm knives spawn, impacts update HUD/UI, and level transitions work via the presenter buttons.

The scene is now connected through installers and presenters, so adding future systems (analytics, alternative input, custom targets) requires only subscribing to the existing event channels or extending installers rather than editing core gameplay code.
