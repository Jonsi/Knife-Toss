# FAIL-502 Knife Sprite Animation Production Record

## Authoritative source

- Approved target: `Captures/FAIL502_GameOverTarget.png`
- Direct extraction atlas: `Assets/Art/UI/Failure/FailurePopupAtlas.png`
- Source sprite: `CrossedKnives`
- Unity source rect: `x=10, y=449, width=385, height=430` (bottom-left origin)
- Raster extraction rect: `left=10, top=145, right=395, bottom=575`
- Separated motion source: `Assets/Art/UI/Failure/FailureKnifeSeparatedSource.png`
- Output: `Assets/Art/UI/Failure/FailureKnivesAnimation.png`
- Output layout: 2048×2048 RGBA, 4 columns × 4 rows, sixteen 512×512 cells
- Playback order: left-to-right, top-to-bottom

The approved crossed-knives art remains the settled source of truth. Frames 1–7 use two
separated red- and blue-handled knife layers reconstructed from that reference so the knives can
actually travel and rotate independently. Frames 8–16 return to the direct approved extraction,
and frames 14–16 are unmodified exact holds. No generated typography, popup frame, button art,
or unrelated UI is used.

## Frame recipe

All frames are centered within their 512×512 cell. The complete beat is intentionally short:
the collision occurs on frame 8 and the remaining frames provide a fast damped recoil.

| Frames | Purpose |
| --- | --- |
| `FailureKnives_00`–`01` | separated anticipation |
| `FailureKnives_02`–`05` | independent inward travel and rotation with restrained motion echoes |
| `FailureKnives_06` | blades meet immediately before the burst |
| `FailureKnives_07` | approved collision art at 112% impact overshoot |
| `FailureKnives_08`–`12` | alternating short recoil at +3°, -2.5°, +1.7°, -1°, and +0.5° |
| `FailureKnives_13`–`15` | unmodified approved final hold |

Lanczos resizing and bicubic rotation are used for the moving layers. Transparent padding
prevents clipping and keeps one consistent center pivot. Motion echoes appear only during the
fast approach; there is no slow whole-illustration zoom or persistent glow.

## Image-generation attempt and rejection

The built-in image-generation tool was asked to create an eight-frame transparent spritesheet
using the existing atlas as its sole reference. The prompt requested exact art preservation,
4×2 equal cells, transparent backgrounds, consistent pivots, a contact/recoil/settle sequence,
and no text or surrounding UI.

The exact generation prompt was:

> Create a production-ready transparent PNG spritesheet for the crossed-knives game-over
> animation, using the supplied image as the sole authoritative visual reference. Isolate and
> preserve the exact crossed knives and orange impact-burst artwork: same blade shapes, bevels,
> highlights, black outlines, red left handle, blue right handle, gold guards and pommels,
> proportions, colors, and polished cartoon game-art rendering. Do not include the title,
> buttons, popup frame, text, or any other UI. Layout: exactly 8 equal square cells arranged 4
> columns by 2 rows, read left-to-right then top-to-bottom. Each cell must have a fully
> transparent background, consistent centered pivot, generous padding, no cropping, and no
> artwork crossing cell boundaries. Animate anticipation, inward snap, contact, a large orange
> impact, recoil, secondary shake, settle, and the exact final reference pose. Keep the knives
> visually identical in every frame; only pose, scale, and impact sparks change. No added
> objects, shadows outside the artwork, checkerboard, labels, grid lines, or text.

That full-sheet result was rejected because it malformed the blade geometry and returned an
opaque checkerboard. A second, tightly scoped extraction generated only one complete red-handled
knife and one complete blue-handled knife. Its checkerboard was removed by connected-background
extraction, the result was converted to real alpha, and the cleaned source was saved as
`FailureKnifeSeparatedSource.png`. Those two layers are used only for the approach frames; the
collision and final hold use the direct approved pixels.

The accepted separated-source prompt was:

> Extract the two individual swords from the supplied crossed-swords illustration and present
> them uncrossed as two separate complete sprites. Use the supplied image as the sole
> authoritative reference. Place the complete red-handled sword and complete blue-handled sword
> vertically in two equal cells with transparent padding. Preserve the blade rendering, gold
> guards and pommels, handle colors, proportions, and cartoon game-art materials. Reconstruct
> only regions hidden by the crossing and burst. Include no burst, sparks, fragments, shadows,
> text, dividers, checkerboard, or surrounding UI.

## Unity integration contract

- Import mode: multiple sprites, sixteen 512×512 rects, centered pivots
- Texture: true alpha, bilinear filtering, no mipmaps, uncompressed, max size 2048
- Runtime owner: `FailureKnifeSpriteAnimator`
- Start delay: 0.08 seconds
- Frame duration: 0.028 seconds
- Clock: unscaled time
- Completion: stop advancing and hold `FailureKnives_15`
- Composition: the sprite animator owns only frame selection; the existing
  `UiElementAnimator` continues to own the illustration's overall popup entrance transform

The collection is fixed at sixteen frames. It does not grow at runtime and requires neither
pooling nor virtualization.

## Validation contract

- The popup contains one `FailureKnifeSpriteAnimator` with exactly sixteen ordered frame refs.
- Every frame ref resolves to `FailureKnives_00` through `FailureKnives_15` in the production
  atlas.
- The animator advances in order and stops on frame 16.
- The popup retains exactly two linked shared action-button prefab instances.
- Time-based acceptance requires a real Play Mode recording and visual inspection, not only a
  still screenshot or serialized-state check.

Final validation on 2026-08-16 passed the scene validator and all three focused Edit Mode tests.
The Play Mode recording contains 180 real Game View frames at 60 fps (1080×1920, three seconds),
and frame-by-frame inspection confirmed independent approach, contact, impact, damped recoil, and the
settled hold. A direct pixel comparison also confirmed that the final frame's source-art region
is identical to `CrossedKnives`.
