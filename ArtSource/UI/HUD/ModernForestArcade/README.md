# Modern Forest Arcade Gameplay HUD production art

Status: v1 visual family approved and integrated 2026-08-15.

## Source and modification chain

- Approved composition reference: `Assets/_Docs/UIProduction/ModernForestArcade/GameplayHUD/GameplayHUD_Reference_v3.png`.
- Approved element review sheet: built-in ImageGen output `exec-3faa0f19-c334-464f-86f7-073873a05152`.
- Production chroma outputs:
  - stage shell: `exec-b21abb03-cf2f-4bc5-a903-dddf3cd75974`
  - magazine shell: `exec-1f8da20c-87f5-442b-a130-6112061cbdbf`
  - power-up bezel: `exec-a681e48f-2272-4a7e-80f1-cdd8810ea3a0`
  - count badge: `exec-cd182957-bff5-4f1b-bf70-d3c023dda3c8`
  - progress marker: `exec-46d6a756-bccd-4997-b461-1c8ac4f893ee`
- Each source was generated on a flat magenta background, converted to straight-alpha RGBA with the installed ImageGen chroma helper using soft matte, despill, and one-pixel edge contraction, then normalized deterministically by `build_runtime_sprites.py`.
- Generated art is original project-specific output. No pixels were copied from the reference mockup or third-party UI packs.

## Prompt contract

All five built-in prompts used `stylized-concept` and requested one centered, straight-on, orthographic, blank production-source sprite matching the approved warm walnut, dark leather, ivory, gold, and restrained teal material language. Every prompt required a perfectly uniform `#FF00FF` background, at least 100 pixels of isolation, crisp alpha-friendly edges, and no text, numbers, icons, gameplay scene, foliage, captions, watermark, perspective, crop, checkerboard, or background shadow.

Element-specific requests were:

1. `HudStageProgressShell`: shallow wide 3.5–4:1 frame with quiet leather center and restrained gold corner hardware.
2. `HudKnifeMagazineShell`: tall narrow approximately 1:3.7 frame with quiet leather center and no knives.
3. `HudPowerUpBezel`: circular walnut/gold ring with four teal notches and key-colored center opening.
4. `HudCountBadge`: compact circular walnut/dark-leather/ivory badge with a blank center.
5. `HudProgressMarker`: small circular walnut/gold/dark-leather marker suitable for runtime tinting.

## Runtime contract

| Sprite | Runtime canvas | Intended reuse |
|---|---:|---|
| `HudStageProgressShell.png` | 1024×384 | One shared stage-progress plaque shell |
| `HudKnifeMagazineShell.png` | 384×1024 | One shared vertical magazine dock shell |
| `HudPowerUpBezel.png` | 512×512 | Retained as an unused source candidate; production buttons keep their existing art |
| `HudCountBadge.png` | 256×256 | Retained as an unused source candidate; production buttons keep their existing count treatment |
| `HudProgressMarker.png` | 128×128 | Shared by `ProgressBarSlot.prefab` instances |

The builder writes byte-identical derivatives to `Runtime/v1` and `Assets/Art/UI/HUD`. All runtime values, icons, and state remain separate Unity objects.
