# Stage collection theming — Unity setup

## Assets

1. **Composite** — `StagesCollectionComposite` (Create → Knife-Toss / Stages Collection Composite, or use existing under `Assets/Configurations/Gameplay/`).
2. **Per-world collections** — Each `StagesCollection` asset lists that world’s `StageConfig` entries.
3. **Composite list** — Assign all world `StagesCollection` assets to the composite’s ordered list (progression order = list order).

## Theme fields (each `StagesCollection`)

- **ThemeName** — Shown as the world title where the UI reads it.
- **StageSelectBackground** — Sprite for that world’s row on the stage-select screen.
- **MenuParticleEffectPrefab** — Optional `GameObject` prefab spawned under the main menu background-effect holder when this collection is active. Leave empty to show no particles for that world.

## Installers and scenes

1. **GameInstaller** (persistent prefab) — Assign **Stages Collection Composite**. Resolver and background effect service register from code.
2. **StageInstaller** / **StagePlay** — Assign the same composite on the installer used in the play scene (field name references the composite).
3. **MainMenu** — On **MainMenuController**, assign **Background Effect Holder** `Transform` (empty parent under the menu camera/UI where particles should appear). Remove any legacy particle instance that duplicated the themed prefab (e.g. old static `Background_Leave` under that holder) so only the service-spawned effect runs.
4. **Stages menu** — On **StagesMenuController** or **StageListController** (whichever owns the list in your prefab), assign the **Stages Collection Composite** and the **World Row Prefab** (`UiWorldStageRow`).

## World row prefab (`UiWorldStageRow`)

Wire serialized references in the prefab instance:

- Background image for **StageSelectBackground**.
- World title text for **ThemeName**.
- Stage slot prefab / list container as required by your row layout.

## Save data

Progress uses **current/highest collection index** and **local stage index** within that collection. Older saves may not migrate; clear or reset player data when testing.

## Verification checklist

- Main menu: background art follows current stage in the active collection; particles match **MenuParticleEffectPrefab** when set.
- Stage select: one row per collection; each row uses its **StageSelectBackground** and title.
- Completing the last stage of a world advances to the next collection (stage index resets locally).
