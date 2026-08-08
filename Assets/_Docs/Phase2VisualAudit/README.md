# Phase 2 Visual Audit Evidence

The decision record and art bible are in
[Phase 2 Visual Direction and Asset Audit](../../../PHASE_2_VISUAL_DIRECTION_AND_ASSET_AUDIT.md).

## Contents

- `ContactSheet.png`: 13 player-facing states captured at 523 x 929 and assembled into a
  labeled 1152 x 2042 review sheet.
- `Captures/`: source screenshots used by the contact sheet.
- `Data/*ReferencedAssets.csv`: live component/property bindings from the five shipping
  scenes. Inactive popup objects were included.
- `Data/DispositionByAsset.csv`: one Keep/Refine/Replace/Remove decision for each of the 75
  unique referenced asset paths.
- `Data/TextureImportAudit.csv`: Android-relevant importer settings for the 58 referenced
  sprite textures.

## Capture Coverage

Loading, Main Menu, Daily Missions, Store, Stages, Gameplay HUD, Settings, Privacy, Report
Error, Add Power-Up, Failure, Completion Reward, and Tutorial Gesture are included.

The Completion Reward, Failure, and Tutorial states were exposed in Play Mode for audit
only. No runtime test mutation was saved into a scene or player profile. The Loading state
was captured directly from its scene in Edit Mode.

`PVPPlay` and `PowerUpsMenu` are excluded because they are not in the Android build settings
and are not reachable through the audited shipping navigation. Development-only mock
provider popups are governed by the Phase 1 production-content gate.
