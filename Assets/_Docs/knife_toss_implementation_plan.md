# Knife Toss – Implementation Plan

## Phase 0 – Project Setup
1. Create new Unity project (2D or 3D with 2D camera).
2. Set up core folders:
   - Scripts/
   - Art/
   - Prefabs/
   - Scenes/
   - UI/
   - Audio/
3. Create Scene: `MainGame`.
4. Set build platform to Android and configure resolution/orientation.

## Phase 1 – Core Scene Layout
1. Add Camera (orthographic) and set background color.
2. Create `GameRoot` empty GameObject.
3. Create Target:
   - Simple circle sprite.
   - Add collider (circle).
   - Add `RotatingTarget` script.
4. Create Knife Launcher:
   - Empty GameObject at bottom center.
   - This will be spawn point for knives.

## Phase 2 – Knife Prefab
1. Create simple knife sprite (rectangle with tip) or placeholder.
2. Add components:
   - Collider (box or capsule).
   - Rigidbody2D (if using physics) or leave none if using manual movement.
   - `Knife` script (handles movement and collision callbacks).
3. Prefab the knife.

## Phase 3 – Input and Knife Throwing
1. Create `KnifeThrower` (or `GameController`) script:
   - Reference to knife prefab.
   - Reference to launcher transform.
   - Fire rate control (only one knife active at a time).
2. On screen tap:
   - If allowed, instantiate knife at launcher position.
   - Set its upward speed or apply force.
3. Lock input while knife is in flight until it hits something.

## Phase 4 – Target Rotation Logic
1. Implement `RotatingTarget`:
   - Public `float rotationSpeed`.
   - `Update()` rotates target using `Transform.Rotate`.
2. Add optional difficulty logic:
   - Randomize direction every few seconds.
   - Slight acceleration on each level.

## Phase 5 – Hit and Fail Logic
1. Decide collision rules:
   - Knife collides with target body → stick.
   - Knife collides with existing knife → fail.
2. Implement sticking:
   - On hit with target, stop movement.
   - Parent knife under target transform.
   - Adjust position slightly to embed.
3. Implement fail:
   - On hit with knife layer/tag, trigger fail event.
   - Play fail VFX/SFX.
   - Notify GameController.
4. Add layer/tag setup:
   - `Target`, `Knife`, `StuckKnife` or reuse as needed.

## Phase 6 – Level System
1. Decide what varies per level:
   - Number of knives the player must stick.
   - Rotation speed of the target.
   - Optional: number of knives already stuck at the start, and how often the target changes direction.

2. Create a simple level configuration asset:
   - A list of levels, each with the values above.
   - Editable in the Inspector so you can quickly tweak difficulty.

3. Game flow per level:
   - At the start of a level, read the relevant level data.
   - Set the target’s rotation speed and behaviour according to that data.
   - Set the total knives the player needs to stick and update the "knives left" UI.
   - Optionally spawn a few knives already attached to the target for extra challenge.

4. Tracking progress inside the level:
   - Keep counters for how many knives are successfully stuck and how many are left to throw.
   - Each time a knife sticks, update the counters and the UI.
   - When the required number of knives is reached, trigger level completion.
   - If you choose to limit the total knives, failing to reach the target count before running out triggers a fail.

5. Progression between levels:
   - Maintain a current level index that starts at 1 (or the highest unlocked level).
   - On win: move to the next level index and load its data.
   - On retry: reload the same level.
   - Optionally save the highest reached level so the player resumes from there.

6. Scaling difficulty:
   - For the first 20–30 levels, define values by hand to get a good early curve.
   - After that, gradually increase knife count and rotation speed in small steps.
   - Occasionally add more pre‑stuck knives or faster direction changes to create spikes in difficulty.

## Phase 7 – UI Implementation
1. Create Canvas with:
   - Level text ("Level X").
   - Knives remaining text.
   - Simple top-center label.
2. Create Panels:
   - `MainMenuPanel` (Play button, Sound toggle).
   - `FailPanel` (Retry, Continue button).
   - `WinPanel` (Next Level button).
3. Link buttons to `GameController` methods:
   - StartGame / RetryLevel / NextLevel / ContinueAfterAd.

## Phase 8 – Feedback & Juice
1. Particles:
   - Create `HitVFX` particle prefab for successful stick.
   - Create `FailVFX` for knife breaking.
2. Camera shake:
   - Simple script that shakes position briefly on hit or fail.
3. Time freeze:
   - Short timescale reduction (0.1s) on successful hits.
4. Audio:
   - Add AudioSource to GameRoot.
   - Hook throw SFX, hit SFX, fail SFX.

## Phase 9 – Game State Management
1. Define states in `GameController`:
   - `MainMenu`, `Playing`, `Win`, `Fail`.
2. Ensure input only works in `Playing` state.
3. Reset logic:
   - On Retry/Next Level, clear old knives.
   - Reset target rotation.
   - Reset counters and texts.

## Phase 10 – Ads Integration
1. Import chosen ad SDK (LevelPlay / AdMob / other).
2. Initialize ads in a dedicated `AdsManager`.
3. Interstitials:
   - Show on Fail.
   - Optionally every N wins.
4. Rewarded ad:
   - `Continue` button on fail triggers rewarded ad.
   - On success callback → respawn player with same level and remaining knives.

## Phase 11 – Settings & Basic Options
1. Sound toggle:
   - Mute/unmute global AudioListener or mixer.
   - Save preference with PlayerPrefs.
2. Basic pause (optional):
   - Pause button sets timescale to 0.

## Phase 12 – Device Testing & Optimization
1. Build to Android test device.
2. Check:
   - Performance (no frame drops).
   - Touch responsiveness.
   - Ad frequency feels acceptable.
3. Fix obvious bugs:
   - Double input.
   - Knives not resetting.
   - Incorrect level transitions.

## Phase 13 – Store Readiness
1. Create simple app icon.
2. Make 4–6 screenshots from device.
3. Write short store description.
4. Set category, age rating, and monetization flags.
5. Final test build and upload to Google Play.

## Phase 14 – Post-Launch Iterations (Optional)
1. Add knife skins.
2. Add different target types (multiple circles, moving targets).
3. Add simple progression (coins, unlocks).
4. Adjust ad frequency based on retention and user feedback.

