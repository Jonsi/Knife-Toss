# Modern Forest Arcade button spritesheet candidate

Date: 2026-08-12
Status: approved direction; production masters, deterministic runtime sprites, and representative Unity integration complete

The user approved this visual direction on 2026-08-12. Separate retained production masters are
available under [`Masters/v1/`](Masters/v1/README.md), and deterministic normalized outputs are
recorded under [`Runtime/v1/`](Runtime/v1/README.md). The original candidate sheet remains the
visual source of truth and is preserved unchanged.

## Files

| File | Purpose | Dimensions | SHA-256 |
|---|---|---:|---|
| `ModernForestArcade_ButtonSpritesheet_Chroma_v1.png` | Original built-in ImageGen output on removable magenta | 1254×1254 RGB | `cc7610c3de7bd2f623cbb25da883f7450c644e40953d65049e2b2c627b50f7c4` |
| `ModernForestArcade_ButtonSpritesheet_RGBA_v1.png` | Locally keyed visual-review candidate | 1254×1254 RGBA | `b4fee50efdffdd72b1742c5b3ff75405d7f161b3ab31cef346e6fc4da80dc344` |

ImageGen output ID: `exec-90e3c693-0661-4069-a07e-b75c9df0e1de`.

The built-in generator returned 1254×1254 despite the 2048×2048 prompt. This remains a
visual-approval source sheet, not a runtime atlas. Runtime sprites were derived from the separate
masters by the checked-in deterministic builder and imported into the V2 `CoreUI.spriteatlasv2`.

## Contents

The sheet contains eight requested elements:

1. blank Primary surface;
2. blank Secondary surface;
3. blank Danger surface;
4. blank Price surface;
5. blank Status surface;
6. blank square Icon Socket;
7. separate Owned ring/check mark;
8. separate Equipped shield/conifer mark.

No text, numbers, arrows, currency, knife glyph, environment, phone, title, or guide decoration is
baked into the candidate.

## Alpha conversion

The RGBA candidate was produced with the installed ImageGen helper:

```text
python3 /Users/yoni/.codex/skills/.system/imagegen/scripts/remove_chroma_key.py \
  --input ModernForestArcade_ButtonSpritesheet_Chroma_v1.png \
  --out ModernForestArcade_ButtonSpritesheet_RGBA_v1.png \
  --auto-key border \
  --soft-matte \
  --transparent-threshold 12 \
  --opaque-threshold 220 \
  --despill
```

Detected key: `#fb03fa`.

Validation of the RGBA file:

- 1,060,565 fully transparent pixels;
- 12,327 partially transparent antialiasing pixels;
- 499,624 fully opaque pixels;
- all four corner alpha values are zero;
- zero visible magenta-like pixels remain;
- every requested element is spatially isolated.

## Exact generation prompt

```text
Use case: stylized-concept
Asset type: clean production-source sprite sheet for a portrait casual mobile game UI
Input image: use the supplied approved Modern Forest Arcade reference only for its button silhouette, restrained tactile rendering, clipped corners, shallow depth, and exact semantic color direction. Do not reproduce the reference-board layout, phone, environment, labels, arrows, or sample icons.

Create a single orthographic 2048×2048 asset sheet containing exactly eight separate, isolated UI elements arranged in a clean grid with generous empty spacing:
1. one blank wide PRIMARY button surface in warm amber (#FDA720 face, #F6C152 restrained highlight, #C35E01 shallow lower depth);
2. one blank wide SECONDARY surface in fresh teal (#31A78D face, #55C9B4 restrained highlight);
3. one blank wide DANGER surface in coral (#F25F35 face, #F98559 restrained highlight, #B52A0B shallow lower depth);
4. one blank wide PRICE surface in warm parchment (#F5DFB4 face, #FFFBE3 highlight, #C0934C restrained edge/depth);
5. one blank wide STATUS surface in deep teal (#012F2A to #014F43), suitable for both Owned and Equipped;
6. one blank square ICON SOCKET in deep teal, using the identical clipped-corner construction;
7. one separate OWNED MARK: a simple teal ring with a clear check, no enclosing button surface;
8. one separate EQUIPPED MARK: a distinct small shield with a simple conifer/knife-like central silhouette, no enclosing button surface.

Construction: premium contemporary casual mobile game UI, crisp raster illustration, long clipped-corner rectangles, consistent 45-degree corner cuts, thin dark Espresso boundary (#321F0E), a very thin inner top/side highlight, subtle low-contrast painted paper/woodgrain texture, and a shallow bottom depth of roughly 7–10% of total height. Large quiet flat centers intended for live TMP text and live icons. Primary may be slightly taller; Secondary and Danger must share exactly the same geometry; Price and Status should be slimmer. Square socket must clearly belong to the same family. Marks must remain separate from surfaces.

Layout: orthographic asset sheet, no perspective, no rotation, no overlap, no cropping. Each element fully contained with at least 80 pixels of perfectly uniform background separating it from every other element and the canvas boundary. Horizontal surfaces centered in their cells, all at consistent pixel scale.

Background: perfectly flat solid #FF00FF chroma-key magenta across the entire canvas, uniform to the edges, for local background removal. No magenta anywhere in the elements. No floor, no contact shadow cast onto the background, no ambient scene lighting, no reflections, no gradient or texture in the background.

Critical constraints: absolutely no text, letters, numbers, words, labels, arrows, coins, knives inside sockets, decorative plants, phones, forest, target, UI guide lines, captions, title, watermark, logo, border around the whole sheet, or mockup presentation. No bulky carved frames, no medieval ornament, no stacked bevels, no black blob outlines, no asymmetric machinery, no excessive grain. Preserve clean alpha-friendly crisp silhouettes and generous spacing.
```

The supplied image was the approved repository reference:
`docs/UIProduction/References/ModernForestArcade_ButtonSystem_Reference.png`.
