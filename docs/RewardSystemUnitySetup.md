# Reward system — Unity setup

## 1. Event channel asset

1. In the Project window: **Create → Knife Toss → Events → Rewards → Reward Granted**.
2. Save it (e.g. `Assets/Configurations/Events/Rewards/RewardGrantedEventChannel.asset`).

## 2. Stage reward catalog

1. **Create → Knife Toss → Systems → Rewards → Stage Reward Catalog**.
2. Save it (e.g. `Assets/Configurations/Systems/Rewards/StageRewardCatalog.asset`).
3. Select the asset. In **Entries**, add one element per stage that should give rewards.
4. For each entry:
  - **Stage**: drag the same `StageConfig` asset used in your **Stages Collection** for that stage.
  - **First Clear Rewards**: add `ItemReward` types (currency / power-up / knife skin) — granted the first time the player clears each level in that stage.
  - **Stage Complete Rewards**: same — granted once when the player finishes the last level of that stage for the first time.
5. Use **Serialize Reference** dropdowns to pick concrete reward types. For **Currency Reward**, assign **Icon** if you want it in the popup.

## 3. Reward row prefab (`RewardItemView`)

1. Create an empty **UI** object under a Canvas (or in a prefab folder).
2. Add **Image** (icon) and **TextMeshPro - Text** (label). Lay them out (e.g. horizontal layout).
3. Add component **Reward Item View** (`RewardItemView`).
4. Assign **Icon** and **Label** in the inspector.
5. Save as a prefab (e.g. `RewardItemView.prefab`).

## 4. Reward popup prefab (`RewardPopup`)

1. Duplicate a working popup from the project (e.g. same structure as other popups using **Base Popup**), or create:
  - Root with **Canvas Group** (full-screen blocker).
  - Child **Content** (RectTransform) for scale/fade — referenced by **Popup Fade In Animation** if you use it.
  - **Popup Fade In Animation** on the root (same pattern as `StageFailPopup`).
2. Add **Reward Popup** on the root.
3. Assign:
  - **Title Text** — TMP for the header.
  - **Reward Items Root** — empty RectTransform with **Vertical Layout Group** (optional) for spawned rows.
  - **Reward Item Prefab** — the prefab from step 3.
  - **Collect Button** — closes the popup.
4. Start the popup **disabled** in the hierarchy (`SetActive(false)`), same as other popups.
5. Save as prefab or keep in the **Stage** scene only.

## 5. Stage scene — `StageInstaller`

1. Open the **stage play** scene that uses **Stage Installer** (`LifetimeScope`).
2. Select the GameObject with **Stage Installer**.
3. Under **Rewards**:
  - **Stage Reward Catalog** → your catalog asset (step 2).
  - **Reward Granted Event Channel** → asset from step 1.
  - **Reward Popup** → reference to the scene instance or prefab instance that has **Reward Popup** (must live in this scene so VContainer can register it).

## 6. Play mode check

- Clear or use a test profile so **first-clear** / **stage-complete** rewards can fire again if you already claimed them.
- Complete a level: you should see grants + save; if the popup is wired, it appears after **Collect**.

## Optional

- If **Reward Popup** is left empty, rewards still grant; only the popup is skipped (**Null Reward Presenter**).
- If **Reward Granted Event Channel** is empty, grants still work; nothing is raised for listeners.

