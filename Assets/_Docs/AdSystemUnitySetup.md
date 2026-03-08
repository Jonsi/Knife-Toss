# Ad System — Unity Setup Manual

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

### 1.3 Continue Placement (Rewarded)

- **Create → Knife Toss → Ads → Rewarded Placement** → name `ContinuePlacement`
- `Placement Key` = **ContinueAfterFail** (enum dropdown)
- `Cooldown Seconds` = **0** (falls back to global default)
- `Min Attempts Between Ads` = **0**
- `Once Per Run` = **true**
- `Reward` = **(None)** — the reward is the level continuation itself, not an item

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

### 1.5 Interstitial Placement

- **Create → Knife Toss → Ads → Interstitial Placement** → name `InterstitialPlacement`
- `Placement Key` = **InterstitialRetry** (enum dropdown)
- `Cooldown Seconds` = **90**
- `Min Attempts Between Ads` = **3**
- `Once Per Run` = **false**

### 1.6 Ad Event Channels

Create folder `Assets/Configurations/Events/Ads/`, then:

1. **Create → Knife Toss → Events → Ads → Ad Requested** → name `AdRequestedEventChannel`
2. **Create → Knife Toss → Events → Ads → Ad Completed** → name `AdCompletedEventChannel`
3. **Create → Knife Toss → Events → Ads → Ad Failed** → name `AdFailedEventChannel`

### 1.7 Continue Requested Event Channel

In `Assets/Configurations/Events/UI/LevelFailedPopup/`:

- **Create → Knife Toss → Events → Common → Void** → name `ContinueRequestedChannel`

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

### 4.1 StageFailPopup — Add Continue Button

1. Find the `StageFailPopup` GameObject
2. Add a **Button (TMP)** child → name `ContinueButton`, text "Continue (Ad)"
3. Style to match the existing Retry / Restart buttons
4. On the `StageFailPopup` component, assign:
   - `Continue Button` → `ContinueButton`
   - `Continue Requested Channel` → `ContinueRequestedChannel`

### 4.2 StageInstaller — Assign Ad Fields

1. Select the `StageInstaller` GameObject
2. Under **Ads (optional)**, assign:
   - `Continue Requested Channel` → `ContinueRequestedChannel`
   - `Continue Placement` → `ContinuePlacement`
   - `Interstitial Placement` → `InterstitialPlacement`

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

### Continue After Failure (StagePlay)

1. Enter a stage → fail a level
2. `StageFailPopup` appears with Retry, Restart, and **Continue (Ad)** buttons
3. Tap Continue (Ad):
   - MockAdPopup appears full-screen:
     - Placement: **ContinueAfterFail**
     - Format: **Rewarded**
     - Timer counting down from 2s
     - Buttons: Complete, Skip, Fail
   - **Complete** → popup closes → level retries from the same point (no reward granted — continuation is the reward)
   - **Skip / Fail** → popup closes → Continue button re-enables if policy allows
4. After one successful continue the Continue button disappears for the rest of the run (`Once Per Run = true`)
5. Starting a new run resets the flag

### Interstitials (StagePlay)

1. Play through 3+ failures or stage completions to satisfy `Min Attempts Between Ads = 3`
2. On the next Retry or Restart tap → MockAdPopup appears:
   - Placement: **InterstitialRetry**
   - Format: **Interstitial**
3. After Complete, the attempt counter resets — need 3 more events before the next interstitial
4. The 90s cooldown additionally prevents back-to-back interstitials

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
