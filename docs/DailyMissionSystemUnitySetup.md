# Daily Mission System Unity Setup

This file tracks the Unity-side setup status for the daily mission system.

## Assets

Done:

- `MissionDefinition` assets created under `Assets/Configurations/Systems/Missions/Definitions/`
- `MissionCatalog` created at `Assets/Configurations/Systems/Missions/DailyMissionCatalog.asset`
- `MissionProgressChangedEventChannel` created at `Assets/Configurations/Events/Missions/MissionProgressChangedEventChannel.asset`
- `MissionCompletedEventChannel` created at `Assets/Configurations/Events/Missions/MissionCompletedEventChannel.asset`

Created mission definitions:

- `CompleteLevelsDaily`
- `HitCollectiblesDaily`
- `UsePowerUpsDaily`
- `WatchAdDaily`
- `CompleteStageNoFailDaily`
- `PurchaseItemDaily`

Needs verification in the editor:

- Confirm each definition has the intended `ObjectiveTypeKey`
- Confirm each definition has final `TargetCount`, `DescriptionFormat`, `Icon`, and `Rewards`

## GameInstaller

Done on `Assets/Prefabs/GameInstaller.prefab`:

- `Daily Mission Catalog`
- `Mission Progress Changed Event Channel`
- `Mission Completed Event Channel`

## StageInstaller

Already wired before this task and verified for the mission system dependency:

- `Ad Completed Event Channel`

Still editor-verification only from this doc's perspective:

- `Level Completed Event Channel`
- `Level Failed Event Channel`
- `Stage Completed Event Channel`
- `Collectible Hit Event Channel`
- `Power Up Used Event Channel`
- `Purchase Completed Event Channel`

## Main Menu UI

Done in `Assets/Scenes/MainMenu.unity`:

- `DailyMissionPopup` exists in the scene
- Popup is inactive by default
- Popup has `DailyMissionPopup` component
- Popup has a `Scroll View`
- Popup content root is wired to `_MissionItemsRoot`
- Popup close button is wired to `_CloseButton`
- `MissionItemView` prefab created at `Assets/Prefabs/UI/Missions/MissionItemView.prefab`
- Popup `_MissionItemPrefab` is wired
- Main menu missions button exists as `DailyMissionButton`
- `MainMenuController._MissionsButton` is wired
- `MainMenuInstaller._DailyMissionPopup` is wired
- `MainMenuInstaller._MissionProgressChangedEventChannel` is wired

Still not done:

- Popup title text is not assigned to `DailyMissionPopup._TitleText`
- Popup refresh timer text is not assigned to `DailyMissionPopup._RefreshTimerText`
- Final visual polish of the popup layout
- Final visual polish of the `MissionItemView` prefab

## MissionItemView Prefab

Done:

- Objective icon `Image`
- Description `TMP_Text`
- Progress `TMP_Text`
- Claim `Button`
- Claimed overlay `GameObject`
- Reward preview root `Transform`
- Existing `RewardItemView` prefab assigned as preview prefab

Not done:

- Progress `Slider` is still unassigned on the prefab

## Notes

- Daily refresh uses UTC date boundaries.
- Mission progress only routes from gameplay events while the stage scene is active.
- Reward claims from the main menu use the global reward grant service.
- The popup works without title and timer text assignments, but those fields are still optional and currently empty.

