# Phase 5 Setup Instructions (Flow & Progression)

## 1. Create Event Channel
1. In Unity, right-click in `Assets/Configurations/EventChannels/`
2. Select: `Create > Knife Toss > Events > Command Event Channel`
3. Name it `CommandEventChannel`

## 2. Setup UI
1. In your Canvas (or create one):
2. Create an empty GameObject named `UIController` and add the `UIController` script to it.
3. Create a Panel named `StageFailPopup` (or use a prefab if available)
   - Add the `StageFailPopup` component to it.
   - Add two Buttons as children (`RetryButton`, `RestartStageButton`).
   - In `StageFailPopup` Inspector, assign `Retry Button`, `Restart Stage Button`, and `Command Channel`.
4. In the `UIController` Inspector, assign:
   - `Stage Fail Popup`: Drag the `StageFailPopup` GameObject/Component here.
   - `State Changed Channel`: `LevelStateChangedEventChannel`.
5. Disable the `StageFailPopup` GameObject by default.

## 3. Update GameInstaller
1. Select the `GameplayInstaller` GameObject
2. Assign the new references:
   - `Command Event Channel`: Drag `CommandEventChannel` asset
   - `Stage Fail Popup`: Drag the `StageFailPopup` GameObject from the scene hierarchy (This is for DI injection)

## 4. Test Phase 5
- Enter Play Mode
- **Win Condition**: Destroy all targets -> Game should automatically load the next level (or loop if last level)
- **Fail Condition**: Run out of knives -> `UIController` should activate `StageFailPopup`
  - Click "Retry" -> Current level reloads
  - Click "Restart Stage" -> Stage 1 Level 1 loads (or current stage start)
