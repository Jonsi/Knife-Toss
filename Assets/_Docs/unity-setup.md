# Unity Setup Guide

This guide covers the essential steps to get the project running in the Unity Editor.

## 1. Open Project
- Open the project root folder with **Unity Hub** (Unity 2021.3+ recommended).
- Ensure the **Input System** package is enabled.

## 2. Create Core Assets
Create the following ScriptableObject assets in your `Assets` folder if they don't exist. You can right-click in the Project window to find them under the `Create` menu.
- `LevelConfig` (*Create → KnifeToss → Level Config*)
- `SoundSettings` (*Create → KnifeToss → Sound Settings*)
- **Event Channels** (*Create → KnifeToss → Events → ...*):
  - `KnifeImpactEventChannel`
  - `LevelCommandEventChannel`
  - `ButtonClickEventChannel`

## 3. Prepare Prefabs
Ensure you have prefabs for:
- The rotating **Target**.
- The **Knife Thrower**.
- The **Knife** projectile.

## 4. Configure the Scene
1.  Open the `MainGame` scene.
2.  Create an empty GameObject named `[Installer]`.
3.  Add the `GameplayInstaller` component to it.
4.  Assign the following in the `GameplayInstaller` component's inspector:
    - **Configurations:**
        - `LevelConfig` asset
        - `KnifeImpactEventChannel` asset
        - `LevelCommandEventChannel` asset
        - `SoundSettings` asset
        - `ButtonClickEventChannel` asset
        - `Start Level` (e.g., 1)
    - **Prefabs For Gameplay:**
        - `Target Prefab`
        - `Knife Thrower Prefab`
        - `Knife Prefab`
    - **UI References:**
        - `Level Hud Presenter` (from your scene's Canvas)

## 5. Configure the UI
1.  **Level HUD Presenter:** Select the `LevelHudPresenter` object in your scene. Assign all of its UI references in the Inspector:
    - `Level Text`
    - `Level Complete Popup`
    - `Retry Button`
    - `Mute Button`
    - `Mute Icon` (the object to show when muted)
    - `Unmute Icon` (the object to show when not muted)

2.  **Level Complete Popup:** Select the `LevelCompletePopup` object (the one assigned to the presenter). Assign its UI references:
    - `Level Complete Text`
    - `Next Button`
    - `Retry Button`
    - `Stars` (an array of 3 GameObjects for the star display)

## 6. Configure Audio
1.  Select your `SoundSettings.asset`.
2.  Assign `AudioClip` files to the fields:
    - `Hit Target Sound`
    - `Hit Knife Sound`
    - `Music` (background music track)
    - `ButtonClickSound`

## 7. Play
Enter Play Mode. The `GameplayInstaller` will set up the scene and the game should be playable.
