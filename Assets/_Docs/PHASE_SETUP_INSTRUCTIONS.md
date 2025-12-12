## Phase 8 Setup Steps (UI)

1) Scene prerequisites
- Ensure a single EventSystem exists in the scene for UI raycast checks.
- Confirm GameInstaller is in the scene and assigned as the active LifetimeScope.

2) Create/assign event channels (assets)
- LevelStartedEventChannel: create under Configurations/EventChannels/Level/ and assign to GameInstaller field.
- KnifeThrownEventChannel, CollectibleHitEventChannel, RetryLevelClicked, RestartStageClicked: ensure existing assets are assigned in GameInstaller.

3) Assign GameInstaller scene references
- KnifeThrowerView → existing scene object.
- LevelRoot → transform that holds spawned level content.
- StageFailPopup → fail popup GameObject.
- UIController → UI controller GameObject.

4) Configure HUD objects (event-driven, no DI)
- StageProgressBarView: set Container; SlotPrefab (ProgressBarSlot); Completed/Incomplete colors; assign LevelStartedEventChannel.
- KnifeMagazineDisplay: set Container; IconPrefab (MagazineKnifeIcon); Active/Inactive colors; assign LevelStartedEventChannel, KnifeThrownEventChannel, RetryLevelClicked, RestartStageClicked.
- CollectibleCountDisplay: set CountText (TMP); assign LevelStartedEventChannel, CollectibleHitEventChannel, RetryLevelClicked, RestartStageClicked.
- MuteButtonView: set Button (UI Button); Label (TMP); assign SettingsService instance reference.

5) Verify TextMeshPro assembly reference
- Game.asmdef must include Unity.TextMeshPro (already added). If missing, add it and reimport.

6) Playmode validation
- Launch Play mode and throw knives; HUD should reflect stage/level, progress dots, knife magazine, and collectible count.
- Click mute button to toggle audio; label should swap between MUTE/UNMUTE and audio should pause/resume.
- Attempt to throw while pointer is over UI; throw should be blocked.

## Phase 9 Setup Steps (Persistence)

1) Scene prerequisites
- GameInstaller present and active LifetimeScope (already required).

2) Event channels (assets)
- LevelCompletedEventChannel, StageCompletedEventChannel, CollectibleHitEventChannel: ensure assets exist under Configurations/EventChannels/ and are assigned to GameInstaller.

3) GameInstaller wiring
- PersistenceInstaller is invoked in GameInstaller.Configure; verify the three channels above are assigned in the inspector.

4) Save location
- playerData.json is stored at Application.persistentDataPath; first save creates it automatically.

5) Playmode validation
- Enter Play mode, complete a level and a stage, collect at least one collectible, exit Play mode, re-enter, and confirm stage index and total collectibles persist.
