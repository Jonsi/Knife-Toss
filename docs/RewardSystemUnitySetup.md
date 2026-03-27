# Reward system — Unity setup

## 1. Event channel asset

1. In the Project window: **Create → Knife Toss → Events → Rewards → Reward Granted**.
2. Save it (e.g. `Assets/Configurations/Events/Rewards/RewardGrantedEventChannel.asset`).

## 2. Reward grant assets (one asset per reward)

Create the **right asset type** for what you want to grant (each type only shows its own fields in the Inspector):

| Menu | Asset | Inspector fields |
|------|--------|------------------|
| **Knife Toss → Rewards → Currency Reward Grant** | `CurrencyRewardGrantEntry` | Currency type, amount |
| **Knife Toss → Rewards → Power-Up Reward Grant** | `PowerUpRewardGrantEntry` | Power-up config, count |
| **Knife Toss → Rewards → Knife Skin Reward Grant** | `KnifeSkinRewardGrantEntry` | Knife skin config |

Save under e.g. `Assets/Configurations/Rewards/`. Reuse the same asset wherever you need the same grant (multiple stages, store, etc.).

## 3. Wire rewards on each `StageConfig`

1. Open each **Stage** asset used in your **Stages Collection**.
2. **Rewards** is a list of references to **any** of the grant assets above (all inherit `RewardGrantEntryBase`). Drag currency / power-up / skin grant assets into the list (or leave empty for no stage-completion rewards).

## 4. Reward row prefab (`RewardItemView`)

1. Create an empty **UI** object under a Canvas (or in a prefab folder).
2. Add **Image** (icon) and **TextMeshPro - Text** (label). Lay them out (e.g. horizontal layout).
3. Add component **Reward Item View** (`RewardItemView`).
4. Assign **Icon** and **Label** in the inspector.
5. Save as a prefab (e.g. `RewardItemView.prefab`).

## 5. Reward popup prefab (`RewardPopup`)

1. Duplicate a working popup from the project (e.g. same structure as other popups using **Base Popup**), or create:
   - Root with **Canvas Group** (full-screen blocker).
   - Child **Content** (RectTransform) for scale/fade — referenced by **Popup Fade In Animation** if you use it.
   - **Popup Fade In Animation** on the root (same pattern as `StageFailPopup`).
2. Add **Reward Popup** on the root.
3. Assign:
   - **Title Text** — TMP for the header.
   - **Reward Items Root** — empty RectTransform with **Vertical Layout Group** (optional) for spawned rows.
   - **Reward Item Prefab** — the prefab from step 4.
   - **Collect Button** — closes the popup.
4. Start the popup **disabled** in the hierarchy (`SetActive(false)`), same as other popups.
5. Save as prefab or keep in the **Stage** scene only.

## 6. Stage scene — `StageInstaller`

1. Open the **stage play** scene that uses **Stage Installer** (`LifetimeScope`).
2. Select the GameObject with **Stage Installer**.
3. Under **Rewards**:
   - **Reward Granted Event Channel** → asset from step 1.
   - **Reward Popup** → scene instance with **Reward Popup** (must be in this scene for DI).

## 7. Play mode check

- Clear or use a test profile so **stage-completion** rewards can fire again if you already claimed them for that stage index.
- Finish a stage: grants + save; with popup wired, it appears after **Collect**.

## Optional

- If **Reward Popup** is empty, rewards still grant; UI is skipped (**Null Reward Presenter**).
- If **Reward Granted Event Channel** is empty, grants still work; listeners get no event.
