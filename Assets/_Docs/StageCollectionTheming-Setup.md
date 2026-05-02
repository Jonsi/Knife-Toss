# Stage collection theming — setup guide

This guide walks through wiring **multiple worlds** (`StagesCollection` assets) behind a **`StagesCollectionComposite`**, themed stage-select rows, and main-menu particles tied to the active world. Art you place under **`Assets/Art/Stages`** is assigned on ScriptableObjects and prefabs as described below.

---

## 1. Art in `Assets/Art/Stages`

1. Import textures (PNG, etc.) in **`Assets/Art/Stages`**.
2. Select each texture in the Inspector:
   - **Texture Type**: `Sprite (2D and UI)` (or `Default` then change when you need a different pipeline).
   - Use **Single** mode for one sprite per file, or **Multiple** + **Sprite Editor** if you sliced an atlas (e.g. several world panels on one sheet like `worlds.png`).
3. **Sprites for worlds**
   - Assign the sprite you want as each world’s **stage-select row background** to that world’s `StagesCollection` field **Stage Select Background** (see §3).
4. **Sprites for individual stages** (main menu / in-stage look) stay on **`StageConfig` → Layout Settings** (e.g. `BackgroundSprite`, `StageIcon`) as before; they do not have to live in `Art/Stages`, but you can reference assets from the same folder if you organize them there.

---

## 2. Composite and world collections

### Create or open the composite

- Path in repo: `Assets/Configurations/Gameplay/StagesCollectionComposite.asset` (or create via **Create → Knife Toss → Gameplay → Stages Collection Composite**).

### Order matters

- The **Collections** list order is **world 0, world 1, …** — progression and the stage-select screen follow this order.

### First world from existing content

- Your original flat list can stay on one **`StagesCollection`** asset (e.g. `StagesCollection.asset`).
- Add that asset as **element 0** on the composite.

### Additional worlds

1. **Create → Knife Toss → Gameplay → Stages Collection** for each new world.
2. Assign **`Theme Name`** (shown on the world row).
3. Assign **`Stage Select Background`** (sprite from `Assets/Art/Stages` or elsewhere).
4. Assign **`Menu Particle Effect Prefab`** (optional `GameObject` prefab; see §5).
5. Copy or assign **`Default Target Prefab`** and fill **`Stages`** with `StageConfig` references for that world only.
6. Append each new collection to the composite list.

---

## 3. Theme fields (each `StagesCollection`)

| Field | Purpose |
|--------|--------|
| **Theme Name** | Title on the world row (`UiWorldStageRow`). |
| **Stage Select Background** | Background `Image` sprite for that row on the stage-select screen. |
| **Menu Particle Effect Prefab** | Spawned under main menu **Background Effect Holder** when this collection is the active world (from `GameData`). Can be left empty. |
| **Default Target Prefab** / **Stages** | Same as before: gameplay defaults and ordered `StageConfig` list for this world only. |

---

## 4. Installers and prefabs

### `GameInstaller` (root / persistent)

- Assign **`Stages Collection Composite`** (replaces any old single-collection reference on this installer).
- `StagesCollectionResolver` and `BackgroundEffectService` register in code; no extra Inspector registration for those.

### `StageInstaller` (`StagePlay` scene)

- Assign the **same** **`Stages Collection Composite`** on the field used for stage flow (composite, not a lone `StagesCollection`).

### Main menu scene

1. Select **`MainMenuController`**:
   - **`Background Effect Holder`**: a `RectTransform` (or `Transform`) parent where menu particles should appear (often under the same canvas as the background).
   - Do **not** rely on a duplicate static VFX under that holder; **`BackgroundEffectService`** destroys the previous instance and spawns the prefab from the active collection.
2. Remove any **legacy** manually placed particle object under that holder (e.g. an old `Background_Leave` instance) so you do not get two effects at once.

### Stages menu / scroll prefab

1. **`StagesMenuController`** and/or **`StageListController`** (depending on your scene/prefab):
   - Assign **`Stages Collection Composite`**
   - Assign **World Row Prefab** → prefab with **`UiWorldStageRow`**
2. **`UiWorldStageRow` prefab** — wire serialized fields:

| Serialized field | Wire to |
|------------------|---------|
| **Background Image** | `UnityEngine.UI.Image` for the row’s full-width (or panel) background. |
| **World Title Text** | `TMP_Text` for the world name. |
| **Stage Item Prefab** | Your existing **`UiStageItem`** prefab (same as old horizontal list). |
| **Stages Content Root** | Empty `Transform` parent; children are instantiated per stage in that world. |

---

## 5. Menu particle prefabs

- Use a **prefab** compatible with your menu (2D/UI scale, sorting layer if needed).
- Set **`Menu Particle Effect Prefab`** on the `StagesCollection` for each world that should show an effect on the main menu.
- Leaving it **null** skips spawning for that world.

---

## 6. Save data and testing

- Progress is **`CurrentCollectionIndex`** + **local `CurrentStageIndex`** inside that collection (plus mirrored fields in player progress data).
- Old saves that only stored a single global stage index may behave incorrectly; for local testing, clear save data or use your debug/cheats reset if available.

---

## 7. Verification checklist

- **Main menu**: Background still follows the **current stage’s** layout (per `StageConfig`); **particles** match the **active world’s** `Menu Particle Effect Prefab` when set.
- **Stage select**: One **world row** per collection; each row shows **Stage Select Background** and **Theme Name**.
- **Gameplay**: Completing the **last** stage of a world advances to the **next** collection and resets local stage index as implemented in your flow.
- **No double VFX**: Only one particle instance under **Background Effect Holder** after Enter Play Mode.

---

## 8. Quick reference — asset locations

| Item | Typical path |
|------|----------------|
| Composite asset | `Assets/Configurations/Gameplay/StagesCollectionComposite.asset` |
| Per-world collections | e.g. `Assets/Configurations/Gameplay/…` or `Assets/Configurations/Gameplay/Collections/…` |
| World / stage art (your folder) | **`Assets/Art/Stages`** |
| World row prefab | e.g. `Assets/Prefabs/UI/Stage/UiWorldStageRow.prefab` |

When in doubt, search the Project window for **`t:StagesCollectionComposite`** and **`t:StagesCollection`** to see all instances.
