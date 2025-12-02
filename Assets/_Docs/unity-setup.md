# Unity Setup (Step-by-Step)

1. **Open Project**
   - Unity Hub → **Open** → select repo root (`Knife-Toss`). Use Unity 2021.3+ with the Input System enabled.

2. **Create Required Assets**
   - Ensure `Assets/LevelConfig.asset` exists (regenerate via **Knife Toss → Generate Level Config** if missing).
   - Create two ScriptableObject channels: `KnifeImpactEventChannel` and `LevelCommandEventChannel` (*Create → Knife Toss → Events → ...*).

3. **Prepare Prefabs**
   - Provide a rotating target prefab (`RotatingTarget` + collider) and a knife thrower prefab (`KnifeThrower` with your `InputActionAsset`). The knife projectile prefab already ships with the repo.

4. **Configure `GameplayInstaller` (DI container)**
   - Add an empty GameObject (e.g., `GameplayInstaller`) to your gameplay scene and attach the component.
    - Assign:
      - `LevelConfig` asset.
      - `KnifeImpactEventChannel` asset.
      - `LevelCommandEventChannel` asset.
      - Starting level (e.g., 1).
      - Target/wheel prefab.
      - KnifeThrower prefab.
      - Knife prefab (used for pre-stuck knives).
      - Scene reference for your HUD (`LevelHudPresenter`) so the coordinator can update UI state.
   - The installer wires everything through VContainer. `GameplayObjectProvider` instantiates the gameplay prefabs at runtime, so you don’t place them manually.

5. **Hook Up UI Commands**
   - Keep your HUD canvas in the scene with `LevelHudPresenter` (it now also owns the fail/complete UI elements).
   - Assign the same `LevelCommandEventChannel` plus references to the Retry/Next buttons and fail text on the presenter. It automatically registers button listeners that raise the channel events—no UnityEvent wiring needed.

6. **Input / Knife Prefab**
   - Ensure the knife thrower prefab references your `InputActionAsset` and the shared knife projectile prefab is set on the installer.

7. **FMOD Audio Setup**
   - **RuntimeManager**: Automatically created when FMOD is first accessed. No manual setup required.
   - **StudioListener**: Already added to Main Camera for 3D audio positioning. This enables spatial audio for sound effects.
   - **AudioListener**: Disabled on Main Camera (FMOD replaces Unity's audio system).
   - **Banks**: If using FMOD Studio, import banks via **FMOD → Settings** and configure bank loading. For simple .wav files, you can use EventReferences directly.
   - **Event References**: Assign FMOD Event References to `SoundEffect` ScriptableObjects in your prefabs/components.

8. **Playtest**
   - Enter Play Mode. `GameplayInstaller` + `LevelFlowCoordinator` handle instantiation, event wiring, and UI updates. Buttons raise channel events, the coordinator listens, and the HUD reflects the current level state.
   - Audio will play automatically when sound effects are triggered. Check the Console for any FMOD initialization warnings.
