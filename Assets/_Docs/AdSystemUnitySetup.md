# Ad System — Unity Setup Manual

> Current product rule: failure recovery never requests an ad or currency spend. The failure
> popup is `Game Over`, primary `Retry`, and quiet `Menu`. ContinueAfterFail,
> InterstitialRetry, Restart, and Continue wiring described by older revisions is deprecated
> and must not be added to StagePlay. The active setup below is limited to approved ad
> placements such as the Store rewarded CTA.

## 1. Create ScriptableObject Assets

All assets go in `Assets/Configurations/Systems/Ads/` (create the `Ads` folder first).

### 1.1 Ad Settings

- Right-click → **Create → Knife Toss → Ads → Ad Settings** → name `AdSettings`
- `Ads Enabled` = **true**
- `Use Mock Provider` = **true**
- `Global Timeout Seconds` = **30**
- `Default Cooldown Seconds` = **60**

### 1.2 Mock Ad Settings

- **Create → Knife Toss → Ads → Mock Ad Settings** → name `MockAdSettings`
- `Default Outcome > Type` = **Completed** (enum dropdown)
- `Simulated Delay Seconds` = **2**
- `Sequenced Outcomes` = leave empty

### 1.3 Legacy Continue Placement (Do Not Configure)

Do not create or wire `ContinuePlacement` / `ContinueAfterFail`. Existing assets are legacy
cleanup targets, not active product configuration.

### 1.4 Store Ad Placement (Rewarded)

- **Create → Knife Toss → Ads → Rewarded Placement** → name `StoreAdPlacement`
- `Placement Key` = **StoreRewarded** (enum dropdown)
- `Cooldown Seconds` = **120**
- `Min Attempts Between Ads` = **0**
- `Once Per Run` = **false**
- `Reward` = select **CurrencyReward** from the dropdown, then configure:
  - `Currency Type` = **Coin** (enum dropdown — this is the Tomato/primary currency)
  - `Amount` = **50**

> The `Reward` field is a polymorphic dropdown. Current options: `CurrencyReward` (grants currency via `CurrencyType` enum + amount), `KnifeSkinReward`, `PowerUpReward`. Any new `ItemReward` subclass added to the codebase appears here automatically. The CTA display text ("+50 COIN") is derived automatically from the reward — no manual text field needed.

### 1.5 Legacy Retry Interstitial (Do Not Configure)

Do not create or wire `InterstitialRetry` on Retry, Menu, or any other failure-recovery
action. Any future interstitial placement requires a separate product decision.

### 1.6 Ad Event Channels

Create folder `Assets/Configurations/Events/Ads/`, then:

1. **Create → Knife Toss → Events → Ads → Ad Requested** → name `AdRequestedEventChannel`
2. **Create → Knife Toss → Events → Ads → Ad Completed** → name `AdCompletedEventChannel`
3. **Create → Knife Toss → Events → Ads → Ad Failed** → name `AdFailedEventChannel`

### 1.7 Legacy Continue Event Channel (Do Not Configure)

Do not create or wire `ContinueRequestedChannel`. Existing channel assets are cleanup
targets only.

---

## 2. MockAdPopup Prefab

The mock popup is a **prefab** with its own Canvas so it is self-contained and works across all scenes. It is only needed during development — production removes it by stripping the `MockAdSystem` child from the `GameInstaller` prefab.

### 2.1 Create the Prefab

1. In any scene, create an empty **GameObject** → name it `MockAdPopup`
2. Add a **Canvas** component to `MockAdPopup`:
   - Render Mode = **Screen Space - Overlay**
   - Sort Order = **999** (renders above everything)
3. Add a **Canvas Scaler** component:
   - UI Scale Mode = **Scale With Screen Size**
   - Reference Resolution = match your project (e.g. 1080 x 1920)
4. Add a **Graphic Raycaster** component
5. Add the **`MockAdPopup`** script component to the root `MockAdPopup` GameObject
6. Add children:
   - **UI → Panel** `Background` — anchor stretch-stretch, all offsets 0, Image color `rgba(0, 0, 0, 0.85)`
   - **TMP_Text** `PlacementText` — centered, upper area (shows placement key name)
   - **TMP_Text** `FormatText` — below PlacementText (shows "Rewarded", "Interstitial")
   - **TMP_Text** `TimerText` — below FormatText (shows auto-dismiss countdown)
   - **Button (TMP)** `CompleteButton` — text "Complete", green
   - **Button (TMP)** `SkipButton` — text "Skip", yellow
   - **Button (TMP)** `FailButton` — text "Fail", red
7. Assign the `MockAdPopup` component fields:
   - `Placement Text` → PlacementText
   - `Format Text` → FormatText
   - `Timer Text` → TimerText
   - `Complete Button` → CompleteButton
   - `Skip Button` → SkipButton
   - `Fail Button` → FailButton
8. **Disable** the `MockAdPopup` root GameObject (uncheck the top-left checkbox) — it starts hidden
9. Drag `MockAdPopup` from the Hierarchy into `Assets/Prefabs/UI/` to save as prefab
10. Delete the temporary instance from the scene

---

## 3. GameInstaller Prefab

Open `Assets/Prefabs/GameInstaller.prefab` (double-click to enter prefab edit mode).

### 3.1 Add AdInstaller

1. Select the root `GameInstaller` GameObject
2. **Add Component → `AdInstaller`**
3. Assign:
   - `Ad Settings` → `AdSettings`
   - `Mock Ad Settings` → `MockAdSettings`
   - `Ad Requested Channel` → `AdRequestedEventChannel`
   - `Ad Completed Channel` → `AdCompletedEventChannel`
   - `Ad Failed Channel` → `AdFailedEventChannel`
   - `Mock Ad Popup Bridge` → assigned in step 3.3

### 3.2 Wire AdInstaller into GameInstaller

1. On the same root GameObject, find the **`GameInstaller`** component
2. Under the **Ads** header, drag the `AdInstaller` component into the `Ad Installer` field

### 3.3 Add MockAdPopupBridge (Development only)

**Option A — Automated (recommended):** Use **Knife Toss → Ads → Fix MockAd Setup in GameInstaller** from the Unity menu. This creates `MockAdSystem` with Canvas, adds the MockAd prefab as a nested instance, wires `MockAdPopupBridge`, and updates `AdInstaller` references.

**Option B — Manual:**
1. Create a **child** GameObject under the root → name it `MockAdSystem`
2. Add **Canvas** (Screen Space Overlay, Sort Order 999), **Canvas Scaler**, **Graphic Raycaster** to `MockAdSystem`
3. **Add Component → `MockAdPopupBridge`** to `MockAdSystem`
4. Drag the `MockAdPopup` **prefab** from `Assets/Prefabs/UI/` into the Hierarchy as a child of `MockAdSystem` — this creates a nested prefab instance
5. Drag the nested `MockAdPopup` **instance from the Hierarchy** into the `Popup` field on `MockAdPopupBridge`
6. Go back to the `AdInstaller` component and assign `Mock Ad Popup Bridge` → the `MockAdPopupBridge` on `MockAdSystem`

> For production: delete the `MockAdSystem` child from the prefab and clear the `Mock Ad Popup Bridge` field on `AdInstaller`. No scene changes required.

### 3.4 Save the Prefab

Apply all changes and exit prefab edit mode.

---

## 4. StagePlay Scene

Open `Assets/Scenes/StagePlay.unity`.

### 4.1 StageFailPopup — No Ad Wiring

Do not add a Continue, Restart, price, currency, rewarded-ad, or interstitial control. The
approved popup contract is documented in
`Assets/_Docs/Phase5VisualAudit/README.md`.

### 4.2 StageInstaller — Leave Failure Ad Fields Unassigned

Legacy failure Continue and retry-interstitial fields must remain unused and should be
removed during the FAIL-503/FAIL-506 cleanup.

---

## 5. StoreScene

Open `Assets/Scenes/StoreScene.unity`.

### 5.1 StoreAdCtaView — Watch-Ad Button

1. In the store UI, add a section for the ad CTA (e.g. at the bottom)
2. Create a GameObject → name `StoreAdCta`
3. Add children:
   - **Button (TMP)** `ActionButton` — text "Watch Ad"
   - **TMP_Text** `RewardText` — populated at runtime by calling `GetDisplayText()` on the placement's reward (e.g. "+50 COIN" for `CurrencyReward` with Amount 50)
4. Add the **`StoreAdCtaView`** component to `StoreAdCta`
5. Assign:
   - `Reward Text` → `RewardText`
   - `Action Button` → `ActionButton`

### 5.2 StoreSceneInstaller — Assign Ad Fields

1. Select the `StoreSceneInstaller` GameObject
2. Under **Ads**, assign:
   - `Store Ad Placement` → `StoreAdPlacement`
   - `Store Ad Cta View` → `StoreAdCta`

---

## 6. Expected Test Results

### Failure Recovery (StagePlay)

1. Enter a stage and fail a level.
2. `StageFailPopup` shows only `Game Over`, primary `Retry`, and quiet `Menu`.
3. Retry reloads the current level exactly once and makes no ad or spend request.
4. Menu exits cleanly and makes no ad or spend request.
5. No Continue, Restart/Start Over, price, rewarded-ad, or interstitial recovery path is
   visible or reachable.

### Store Rewarded Ad (StoreScene)

1. Open the Store scene — the "Watch Ad" button is **enabled** and shows **"+50 COIN"** (derived automatically from the `CurrencyReward` configuration)
2. Tap it → MockAdPopup appears:
   - Placement: **StoreRewarded**
   - Format: **Rewarded**
3. **Complete** → popup closes → the `CurrencyReward` grants 50 Coin (Tomato) to the player's balance
4. The button disables for 120s (cooldown), then re-enables
5. To reward a different type: change the `Reward` dropdown to `CurrencyReward` with `Currency Type = Orange`, or swap to `PowerUpReward` / `KnifeSkinReward` — the button label updates automatically

### Debug Cheats (TEST_ENABLED builds)

1. SRDebugger → **5. Ads** tab
2. `Ads Enabled` — shows the master kill switch state
3. **Reset Cooldowns** — clears all cooldown timers and attempt counters
4. **Reset Once-Per-Run Flags** — re-enables all once-per-run placements for this run
5. **Force: Completed / Skipped / Unavailable / Timeout / Failed** — overrides the outcome of the next mock ad show, regardless of configured outcomes
