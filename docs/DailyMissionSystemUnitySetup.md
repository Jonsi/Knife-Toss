# Daily Mission System Unity Setup

## Assets

Create the shared mission assets:

- `MissionDefinition` assets under `Assets/Configurations/Systems/Missions/Definitions/`
- `MissionCatalog` asset at `Assets/Configurations/Systems/Missions/DailyMissionCatalog.asset`
- `MissionProgressChangedEventChannel` and `MissionCompletedEventChannel` assets under `Assets/Configurations/EventChannels/Missions/`

Recommended first-pass mission pool:

- `CompleteLevels_3`
- `HitCollectibles_10`
- `UsePowerUps_2`
- `WatchAd_1`
- `CompleteStageNoFail_1`
- `PurchaseItem_1`

For each definition:

- Set `ObjectiveTypeKey` to one of the constants in `Assets/Scripts/Game/Missions/Data/MissionObjectiveKeys.cs`
- Set `TargetCount`, `DescriptionFormat`, `Icon`, and `Rewards`
- Keep asset names stable because daily save data stores `MissionDefinition.name` as the definition key

## GameInstaller

Wire these fields on the persistent `GameInstaller` prefab:

- `Daily Mission Catalog`
- `Mission Progress Changed Event Channel`
- `Mission Completed Event Channel`

This enables the global tracker, the daily provider, and global reward claiming support.

## StageInstaller

Wire the ad and gameplay channels on the stage scene `StageInstaller`:

- `Level Completed Event Channel`
- `Level Failed Event Channel`
- `Stage Completed Event Channel`
- `Collectible Hit Event Channel`
- `Power Up Used Event Channel`
- `Purchase Completed Event Channel`
- `Ad Completed Event Channel`

The `MissionProgressRouter` is registered from `StageInstaller`, so it depends on these references being assigned.

## Main Menu UI

Create a `DailyMissionPopup` view in the main menu scene:

- Root panel with `DailyMissionPopup`
- Optional `PopupFadeInAnimation`
- Title `TMP_Text`
- Refresh timer `TMP_Text`
- Content root with `VerticalLayoutGroup`
- Close `Button`

Set the popup inactive by default.

Create a `MissionItemView` prefab with:

- Objective icon `Image`
- Description `TMP_Text`
- Progress `TMP_Text`
- Progress `Slider`
- Claim `Button`
- Claimed overlay `GameObject`
- Reward preview root `Transform`
- Existing `RewardItemView` prefab assigned as the preview prefab

Add a missions button to the main menu layout and wire:

- `MainMenuController._MissionsButton`
- `MainMenuInstaller._DailyMissionPopup`
- `MainMenuInstaller._MissionProgressChangedEventChannel`

## Notes

- Daily refresh uses UTC date boundaries.
- Mission progress only routes from gameplay events while the stage scene is active.
- Reward claims from the main menu use the global reward grant service. Stage rewards still use the stage-scoped reward popup presenter.

