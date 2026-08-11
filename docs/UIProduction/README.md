# Production UI asset register

`PRODUCTION_ASSET_REGISTER.csv` is the maintainable source of truth for the shared UI
elements planned by P2. It translates the approved Hand-Carved Forest Arcade direction into
production briefs without treating any target-board pixel as a runtime asset.

## Field conventions

- Target pixels describe the editable/output canvas to author, not the rendered layout size.
- Display size is in dp. At the 1080 x 1920 reference, one dp is three Canvas reference pixels.
- Nine-slice borders are ordered left / bottom / right / top in source pixels.
- `TBD` output paths are reservations only; no final sprite is produced by P0-007/P2-001.
- `Blocked` provenance means the asset may remain as a reference but cannot be approved for
  release or used as the basis of a derivative until proof is attached.
- New ordinary UI art targets PPU 100, Bilinear filtering, sRGB on, mipmaps and Read/Write
  off, ASTC 6x6 on Android, no Crunch, straight alpha from input, and an approved SpriteAtlas
  V2 group. Critical icons/text-bearing art target ASTC 4x4.
- Atlas reservations follow `CoreUI`, `GameplayCommon`, `World01`, and `SecondaryUI`.

## Provenance boundaries

- Liberation Sans is retained from TextMesh Pro Essential Resources with its repository OFL.
- Lilita One is retained for display/action roles; the local Layer Lab guide identifies its
  Google Fonts source and license. The editable font source is not vendored.
- Layer Lab GUI art remains blocked until the project entitlement/receipt is attached.
- Legacy `Assets/Art` and world/configuration art remains blocked until an owner authorship
  and usage-right declaration is attached, or the affected output is replaced.
- The Shatter Strike and Chrono Shift production family has editable sources and a recorded
  OpenAI generation/local cleanup history under `ArtSource/PowerUps` and
  `Assets/_Docs/PowerUpButtonArt/README.md`.

## Explicit exclusion

Tutorial and onboarding UI is permanently removed. Hand pointers, gesture prompts,
spotlights, forced-focus overlays, tutorial controllers, onboarding persistence, and
tutorial-specific modal authority must not be added to this register or the production kit.
