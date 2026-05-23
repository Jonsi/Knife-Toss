# Phase 1 Execution - Opening Flow Audit

## Approved Decisions

- First-session style: **Focused arcade**.
- Soft gating is allowed.
- Phase 1 includes implementation, not just audit.
- Tutorial should not hide existing menu features.
- First-time popups are not globally banned, but the first Play path should remain focused and uninterrupted unless a popup is explicitly required.

## Current Flow Audit

### What Already Worked

- The main menu has a direct Play button wired to the current stage.
- Play clamps the selected stage to the unlocked range before loading gameplay.
- StagePlay is preloaded from the main menu, which supports a fast Play path.
- The current stage label is displayed as `Stage X`.
- The main menu updates its background from the selected stage/theme.
- Missions open only when the Missions button is clicked.
- Store opens only when the Shop button is clicked.
- Remove Ads purchase starts only when the Remove Ads button is clicked.
- PVP and Power-ups were not wired in the current MainMenu scene references, so they were not active first-session distractions in this scene.
- The coin display was present immediately.
- The current scene did not expose a main-menu `SettingsButtonView`.

### First-Session Risks Found

- Shop, Stages, Daily Missions, Remove Ads, and the coin display were visible immediately.
- Remove Ads was a monetization CTA before the player had necessarily played.
- Main menu animation references include many menu elements, so tutorial overlays should avoid changing their normal visibility.

## Implemented Changes

Updated `Assets/Scripts/Game/UI/MainMenu/MainMenuController.cs`.

### Tutorial Visibility

The first-session tutorial no longer hides menu features. Existing buttons, coins, settings, and other menu surfaces remain visible while the gesture tutorial is active.

Play remains the primary route because the opening gesture points at Play and starts the Play flow on tap.

### Remove Ads Safety

Remove Ads uses its normal availability logic and also checks that the Remove Ads flow exists before reading purchase availability.

### Animation Safety

Menu Play-In and Play-Out animations run normally during the tutorial because no menu elements are hidden for the tutorial.

### Tap Gesture Overlay

Added a reusable `GesturePopupView` component and wired `Assets/Prefabs/UI/GesturePopup.prefab` into the MainMenu scene.

Added `GestureHintConfiguration` at `Assets/Configurations/UI/GestureHintConfiguration.asset` to hold tuned hand positions. Current configured positions:

- `TapOnPlayPosition`
- `TapToThrowPosition`

Fresh/no-progress players now see a full-screen tap indication overlay with the hand positioned over the Play button. Tapping anywhere on the overlay:

- hides the gesture overlay,
- triggers the Play flow,
- saves a one-time onboarding flag so the Play gesture does not appear again,
- keeps the opening focused on immediate gameplay.

The gesture view uses DOTween for fade and pulse animation. It can be positioned from configuration data or pointed at a target `RectTransform`, so later onboarding beats can place the hand over other UI elements.

### Tap-To-Throw Stage Gesture

Added a one-time StagePlay gesture hint that appears from the same gesture prefab and shared configuration.

The StagePlay version is non-blocking: the player taps normally, the knife throw still happens, and the hint dismisses only after the real knife-thrown event fires. This prevents the tutorial overlay from stealing the player's first gameplay input.

After the first real throw:

- the stage gesture hides,
- `HasTappedThrowGesture` is saved,
- the gesture does not appear again on later StagePlay visits.

## Current Behavior After Change

For a fresh player with no progression:

- Play is still active.
- Existing main menu buttons remain visible.
- Coin display remains visible if present.
- Settings/mute buttons remain visible if present.
- Remove Ads follows its normal availability rules.
- A tap gesture overlay is shown over the Play button and starts Play on tap.
- After the player taps the gesture once, it stays dismissed on later MainMenu visits.
- On first StagePlay entry, a tap-to-throw gesture is shown at the configured `TapToThrowPosition`.
- The first normal stage tap throws the knife and dismisses the stage gesture permanently.

For a player with progression beyond the initial frontier, the Play gesture is skipped and the menu behaves normally.

## Manual QA Checklist

Test with fresh player data:

- [ ] Launch to MainMenu.
- [ ] Confirm Play is visible and prominent.
- [ ] Confirm existing menu buttons remain visible during the tutorial.
- [ ] Confirm coin display remains visible if present.
- [ ] Confirm settings/mute icon remains visible if present.
- [ ] Confirm Remove Ads follows its normal availability rules.
- [ ] Confirm the tap gesture overlay appears over the Play button.
- [ ] Tap the overlay and confirm StagePlay loads.
- [ ] Confirm the tap-to-throw gesture appears in StagePlay.
- [ ] Tap the gameplay area and confirm the knife throws immediately.
- [ ] Confirm the tap-to-throw gesture hides after that throw.
- [ ] Reload StagePlay and confirm the tap-to-throw gesture does not appear again.
- [ ] Return to MainMenu before progression and confirm the gesture does not appear again.
- [ ] Confirm no popup appears automatically unless intentionally configured elsewhere.
- [ ] Press Play.
- [ ] Confirm StagePlay loads quickly.
- [ ] Return to MainMenu before progression and confirm the menu still shows its normal visible elements.

Test with progressed player data:

- [ ] Complete or unlock beyond the initial stage frontier.
- [ ] Return to MainMenu.
- [ ] Confirm the Play gesture is skipped.
- [ ] Confirm normal menu visibility is unchanged.
- [ ] Confirm Remove Ads appears only if available and not owned.
- [ ] Confirm Play still works.

## Remaining Risks

- This phase uses the existing progression frontier (`HighestCollectionIndex` and `HighestStageIndex`) to decide whether the Play gesture should appear.
- If stage completion does not advance or save that frontier correctly, the Play gesture may remain eligible until its own tap flag is saved.
- This should be verified during the Phase 1 manual playtest and revisited in Phase 4/5 if the post-Stage-1 reveal timing needs a more explicit first-session flag.

## Phase 1 Status

Implementation is complete for the first tutorial gesture pass.

Phase 1 should be closed only after a fresh-data Unity playtest confirms that the first player journey reaches gameplay quickly while normal menu visibility remains intact.
