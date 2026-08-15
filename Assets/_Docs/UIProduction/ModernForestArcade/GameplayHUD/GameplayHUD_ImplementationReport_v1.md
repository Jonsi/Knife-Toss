# Gameplay HUD implementation report

Status: integrated and validated on 2026-08-15  
Approved visual target: `GameplayHUD_Reference_v3.png`  
Scene: `Assets/Scenes/StagePlay.unity`

## Reused production sources

- `Assets/Prefabs/UI/Buttons/MenuButton.prefab`: settings action and hit target, now using the approved shared Icon Socket and `IconSettings` artwork.
- `Assets/Prefabs/UI/Coin/CoinUiDisaply.prefab`: Main Menu and StagePlay use this exact shared prefab source. StagePlay overrides only root placement and scale; its surface, icon, text, hierarchy, and live currency feedback remain authored by the shared prefab.
- `Assets/Prefabs/UI/PowerUp/PowerUpButton.prefab`: restored byte-for-byte to its pre-HUD source. Shatter Strike and Chrono Shift retain their original button art, counts, selection, acquisition, and cooldown behavior with no added rings.
- `Assets/Art/PowerUps/ShatterStrikeIcon.png` and `ChronoShiftIcon.png`: existing power-up identities.
- `Assets/Prefabs/MagazineKnifeIcon.prefab`: one shared knife source, retaining the existing 27.79-degree lean.
- `Assets/Prefabs/UI/ProgressBarSlot.prefab`: one shared progress-marker source.
- Existing event channels, catalog data, stage progression, knife count, inventory count, modal layering, and safe-area ownership.

## New production sources

- `Assets/Prefabs/UI/HUD/StageProgressPlaque.prefab`
- `Assets/Prefabs/UI/HUD/KnifeMagazineDock.prefab`
- Five neutral HUD sprite candidates under `Assets/Art/UI/HUD`:
  - `HudStageProgressShell.png`
  - `HudKnifeMagazineShell.png`
  - `HudPowerUpBezel.png`
  - `HudCountBadge.png`
  - `HudProgressMarker.png`
- Reproducible RGBA masters and normalization tooling under `ArtSource/UI/HUD/ModernForestArcade`.
- `Assets/Editor/GameplayHudProductionBuilder.cs` for deterministic importer, prefab, layout, and linkage updates.

No environment art, target art, tomatoes, thrown/stuck knives, gameplay rules, economy values, or inventory values were generated or baked into HUD sprites. The stage plaque intentionally omits fraction text; its progress markers remain runtime-driven.

Following runtime review, `HudPowerUpBezel` and `HudCountBadge` are retained only as unused source candidates. Neither is referenced by the production power-up prefab or StagePlay.

## Runtime scaling behavior

- Stage progress markers use a high-watermark pool. The view instantiates only when a later stage requires more markers, then reuses and hides excess markers for smaller stages.
- Magazine knives use the existing high-watermark pool. The dock and knives are enlarged, and spacing is calculated from the authoritative count. Five knives remain individually separated; the configured maximum of eight contracts to fit without clipping.
- Power-up buttons are bounded by the catalog. The current catalog produces two runtime instances from the same shared `PowerUpButton.prefab`.
- Stage and magazine scene roots are connected prefab instances. Their repeated children reference `ProgressBarSlot.prefab` and `MagazineKnifeIcon.prefab` respectively.
- Main Menu and StagePlay currency instances both retain direct prefab links to `CoinUiDisaply.prefab`.

## Validation evidence

- Unity structural validation: PASS.
- Missing scene references/scripts: 0.
- Decorative raycast targets in stage plaque and magazine: 0.
- Runtime Console errors after final pass: 0.
- EditMode tests: 182 passed, 0 failed, 0 skipped.
- PlayMode tests: 1 passed, 0 failed, 0 skipped.
- Inspected at 360x640, 1080x1920, 1080x2160, and 1440x2560.
- A focused prefab-source test confirms that StagePlay and Main Menu use the same currency prefab.
