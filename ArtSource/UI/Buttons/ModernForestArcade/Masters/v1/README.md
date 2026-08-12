# Modern Forest Arcade production masters v1

Date: 2026-08-12
Status: separate high-resolution source masters complete; normalized runtime derivatives imported into Unity

These files implement production step 2 after explicit user approval of the visual candidate.
Every element is retained both as the original keyed ImageGen output and as a locally extracted
RGBA source. Later production work derived deterministic normalized sprites without changing these
retained masters; see `../../Runtime/v1/README.md`.

## Source manifest

| Element | ImageGen output ID | Chroma dimensions | Chroma SHA-256 | RGBA SHA-256 |
|---|---|---:|---|---|
| Primary | `exec-e266449a-10f0-4dd6-b703-b8fa846c2ee9` | 2048×768 | `e9d9810caa9aec61403bd56827d1639bcedf2a25b18caee6e73511eb64cb3936` | `fcd19d044b8cfda15a79600622ca3d27b7b2eba0abfb512004ef6536d51535da` |
| Secondary | `exec-f2355ba4-c29a-4839-aeac-59809d21c105` | 2048×768 | `365eb503eb9fa138f071eea3b4b55524e8bea40cf7b6c28dac35b5bcd48c3365` | `a87f12ce4ca742f0fcd3dbe710d6f7b57f99bb3e74750794035353182b19d05a` |
| Danger | `exec-5099ffe5-0eac-4143-a6fa-de6c36e3f800` | 2048×768 | `0acbc6d8868fd6fa586ae367b86e4ec6238f23b686cea5b25185233390a29a95` | `1af8e8d941a5e05d738ab083589b3a2fb6554f5b2121345c41dda59fd8fa5e31` |
| Price | `exec-e653c40b-1df1-4d59-8fd7-4067ee61fae2` | 2048×768 | `3ce00dfc64e4fa0930c18d448e0d724a6ddea377f0bd965a37331d0ce4dd0a0e` | `d2baa98c235dde2064bb717d0eec3a79f395e020ef68d544140d6635b8316e53` |
| Status | `exec-5f67027f-91b4-4b1b-9173-a85ce39834cd` | 2048×768 | `49cb0d637c6f32de8156ad940dfac5b6bf46717e6d4437513053f3971dd72aed` | `145661ea3bc578931a952e5e59c25415f9410a4933d9dce647ab4751ac03ab99` |
| Icon Socket | `exec-683c2d00-5cc1-4198-ba75-a9c2b6b4d708` | 1254×1254 | `6eec9569a393ca6edb73613880aabc5768986070a6b28cd1c486dcab23130d47` | `540f4c2f284824a0b7e60d3ef1ad9bd2434654f9373425c818faa966d37f43da` |
| Owned mark | `exec-22c12be6-a74b-439e-bc19-95e4de48abf8` | 1254×1254 | `0f1563aed23d7254fdc08150054fdc82d72d3226bd693bcc013203cbaeae211e` | `9ff2eadc4eab9c013287c9ad9e7116dbe48881b30afed618e33c5a6214e37365` |
| Equipped mark | `exec-bcc93e15-acb1-4e93-9c04-83b67381a40f` | 1254×1254 | `32f4cbade1afe43afc02ec4fb472128074791538b84cd6cd8eff52e70e10e4f2` | `da3c7d4915d78029e6f9575b6456f332f3466bcf9a62e0dbfd8dae6c8c0833cf` |

The built-in generator returned 1254×1254 for square prompts requesting 1024×1024. Those outputs
are retained at their native returned dimensions rather than resampled.

## Alpha validation

All RGBA files have transparent corners and zero visible magenta-like pixels after extraction.
The standard extraction used automatic border-key detection, soft matte, thresholds 12/220, and
despill. The Equipped mark additionally used a one-pixel edge contraction to eliminate a thin
magenta fringe detected during inspection.

Observed alpha bounding boxes at threshold 32:

| Element | Alpha bounding box `(x,y,x2,y2)` | Canvas margins `(L,T,R,B)` |
|---|---|---|
| Primary | `162,148,1887,618` | `162,148,161,150` |
| Secondary | `162,148,1887,618` | `162,148,161,150` |
| Danger | `163,151,1885,620` | `163,151,163,148` |
| Price | `114,167,1934,601` | `114,167,114,167` |
| Status | `101,221,1946,548` | `101,221,102,220` |
| Icon Socket | `163,181,1091,1065` | `163,181,163,189` |
| Owned mark | `203,178,1050,1052` | `203,178,204,202` |
| Equipped mark | `290,199,964,1038` | `290,199,290,216` |

Secondary was edited directly from Primary and its thresholded alpha silhouette has 0.999332 IoU
with Primary. Danger was also edited from Primary but has 0.988020 IoU because ImageGen shifted
some antialiased edge pixels. Production step 3 must normalize both semantic variants to one exact
deterministic geometry before nine-slicing; this source record does not overstate pixel identity.

## Exact prompts

### Primary

```text
Use case: stylized-concept
Asset type: high-resolution production raster master for a nine-sliced Unity mobile Primary button
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its amber Primary element as the exact visual direction for silhouette, proportions, clipped corners, restrained texture, shallow depth, and palette. Recreate it cleanly as one isolated asset at higher source quality; do not copy any other sheet element.

Create exactly one blank wide Primary action-button surface, front-facing orthographic, centered, approximately 3.8:1 overall aspect ratio. Long clipped-corner rectangle with four consistent 45-degree cuts, thin Espresso dark boundary, very thin inner cream/amber top and side highlight, warm amber face (#FDA720), restrained highlight (#F6C152), shallow darker lower depth (#C35E01) occupying about 7–10% of total height. Very subtle low-contrast painted paper/woodgrain texture. Large quiet central 70% suitable for live TMP text. Symmetric endcaps and clean uninterrupted stretch center suitable for nine-slicing.

Canvas/layout: high-resolution landscape source, target 2048×768. Subject occupies about 82% canvas width and 36% canvas height with at least 96 px empty padding on every side. Exact straight-on view, no perspective, rotation, crop, overlap, or cast shadow outside the silhouette.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the subject.

Critical constraints: exactly one surface only. Absolutely no text, letters, numbers, symbols, arrows, icons, coins, knives, marks, labels, watermark, logo, border around the canvas, environment, phone, guide lines, or decorative objects. No bulky carved frame, medieval ornament, stacked bevels, black blob outline, asymmetric machinery, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```

### Secondary

```text
Use case: precise-object-edit
Asset type: high-resolution production raster master for a nine-sliced Unity mobile Secondary button
Input image: the retained approved Primary master on magenta. Preserve its exact canvas, position, silhouette, dimensions, 45-degree corner cuts, boundary thickness, inner highlight geometry, shallow lower-depth geometry, subtle texture pattern, padding, orthographic view, and uniform chroma-key background pixel-for-pixel in shape.

Change only the button color treatment from amber Primary to fresh teal Secondary: face centered around #31A78D, restrained top/side highlight around #55C9B4, dark teal lower depth, and Espresso-compatible thin boundary. Keep the same restrained luminance variation and quiet center. The output must remain one blank surface.

Critical constraints: geometry and placement must not change. No text, letters, numbers, symbols, arrows, icons, marks, labels, extra objects, new shadows, glow, watermark, logo, cropping, rotation, perspective, or background variation. Preserve the perfectly flat #FF00FF-style removable magenta background and do not place magenta inside the subject.
```

### Danger

```text
Use case: precise-object-edit
Asset type: high-resolution production raster master for a nine-sliced Unity mobile Danger button
Input image: the retained approved Primary master on magenta. Preserve its exact canvas, position, silhouette, dimensions, 45-degree corner cuts, boundary thickness, inner highlight geometry, shallow lower-depth geometry, subtle texture pattern, padding, orthographic view, and uniform chroma-key background pixel-for-pixel in shape.

Change only the button color treatment from amber Primary to coral Danger: face centered around #F25F35, restrained top/side highlight around #F98559, shallow darker lower depth around #B52A0B, and a thin dark red-brown/Espresso-compatible boundary. Keep the same restrained luminance variation and quiet center. The output must remain one blank surface.

Critical constraints: geometry and placement must not change. No text, letters, numbers, symbols, arrows, icons, marks, labels, extra objects, new shadows, glow, watermark, logo, cropping, rotation, perspective, or background variation. Preserve the perfectly flat #FF00FF-style removable magenta background and do not place magenta inside the subject.
```

### Price

```text
Use case: stylized-concept
Asset type: high-resolution production raster master for a nine-sliced Unity mobile Price button
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its blank parchment Price surface as the exact visual direction for silhouette, slim proportions, clipped corners, restrained tactile rendering, shallow depth, and palette. Recreate it cleanly as one isolated higher-resolution asset. Do not include the sheet's other elements.

Create exactly one blank wide Price surface, front-facing orthographic, centered, approximately 3.8:1 overall aspect ratio. Long slim clipped-corner rectangle with four consistent 45-degree cuts, thin Espresso/parchment-brown boundary, very thin warm inner top and side highlight, warm parchment face (#F5DFB4), restrained highlight (#FFFBE3), shallow lower edge/depth (#C0934C) occupying about 7–9% of total height. Very subtle low-contrast paper/paint texture. Preserve a large quiet center and quiet left/right content zones for live resource icon and live TMP value. Symmetric endcaps and clean uninterrupted stretch center suitable for nine-slicing. No baked icon socket, medallion, divider, or text.

Canvas/layout: high-resolution landscape source, target 2048×768. Subject occupies about 82% canvas width and 30% canvas height with at least 96 px empty padding on every side. Exact straight-on view, no perspective, rotation, crop, overlap, or cast shadow outside the silhouette.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the subject.

Critical constraints: exactly one surface only. Absolutely no text, letters, numbers, symbols, arrows, icons, coin, price, badge, labels, extra objects, watermark, logo, canvas border, environment, phone, guide lines, or decoration. No bulky carved frame, medieval ornament, stacked bevels, black blob outline, asymmetric machinery, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```

### Status

```text
Use case: stylized-concept
Asset type: high-resolution production raster master for a nine-sliced Unity mobile Owned/Equipped status surface
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its blank deep-teal Status surface as the exact visual direction for silhouette, slim proportions, clipped corners, restrained tactile rendering, shallow depth, and palette. Recreate it cleanly as one isolated higher-resolution asset. Do not include the sheet's other elements.

Create exactly one blank wide Status surface usable for both Owned and Equipped states, front-facing orthographic, centered, approximately 4.5:1 overall aspect ratio. Long slim clipped-corner rectangle with four consistent 45-degree cuts, thin very-dark Pine/Espresso boundary, very thin teal inner top and side highlight, deep-teal face transitioning subtly between #012F2A and #014F43, and shallow darker lower depth occupying about 7–9% of total height. Very subtle low-contrast painted paper/woodgrain texture. Preserve a quiet left zone for a separate live status mark and a large quiet center for live TMP copy. Symmetric endcaps and clean uninterrupted stretch center suitable for nine-slicing. No baked status mark, divider, icon, or text.

Canvas/layout: high-resolution landscape source, target 2048×768. Subject occupies about 82% canvas width and 27% canvas height with at least 96 px empty padding on every side. Exact straight-on view, no perspective, rotation, crop, overlap, or cast shadow outside the silhouette.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the subject.

Critical constraints: exactly one surface only. Absolutely no text, letters, numbers, words, check, ring, shield, tree, knife, symbols, arrows, icons, marks, labels, extra objects, watermark, logo, canvas border, environment, phone, guide lines, or decoration. No bulky carved frame, medieval ornament, stacked bevels, black blob outline, asymmetric machinery, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```

### Icon Socket

```text
Use case: stylized-concept
Asset type: high-resolution production raster master for a Unity mobile icon-only button socket
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its blank square Icon Socket as the exact visual direction for silhouette, clipped corners, restrained tactile rendering, shallow depth, and palette. Recreate it cleanly as one isolated higher-resolution asset. Do not include the sheet's other elements.

Create exactly one blank square Icon Socket, front-facing orthographic and centered. Near-square clipped-corner octagonal surface using the same family construction: four consistent 45-degree corner cuts, thin very-dark Pine/Espresso boundary, very thin teal inner top and side highlight, deep-teal face between #012F2A and #014F43, and shallow darker lower depth occupying about 7–9% of total height. Very subtle low-contrast painted paper/woodgrain texture. Large quiet central 65% for a separate live icon. Symmetric geometry suitable for slicing if needed. No baked glyph.

Canvas/layout: high-resolution square source, target 1024×1024. Subject occupies about 65% of canvas width and height with generous uniform padding. Exact straight-on view, no perspective, rotation, crop, overlap, or cast shadow outside the silhouette.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the subject.

Critical constraints: exactly one blank socket only. Absolutely no text, letters, numbers, words, knife, tree, check, shield, arrows, icons, symbols, marks, labels, extra objects, watermark, logo, canvas border, environment, phone, guide lines, or decoration. No bulky carved frame, medieval ornament, stacked bevels, black blob outline, asymmetric machinery, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```

### Owned mark

```text
Use case: stylized-concept
Asset type: high-resolution production raster status mark for a Unity mobile UI
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its separate Owned ring/check mark as the exact visual direction. Recreate it cleanly as one isolated higher-resolution mark; do not include any button surface or other sheet element.

Create exactly one Owned status mark: a clear circular teal ring with one bold clean check mark crossing the center. Contemporary casual-game construction, crisp simple silhouette, two-tone fresh teal/deep teal with a restrained thin cream or mint highlight and thin dark boundary. Front-facing orthographic, centered, readable at 48–64 dp. The ring and check may be separate internal shapes but must read as one mark. Keep negative space open and avoid tiny details.

Canvas/layout: high-resolution square source, target 1024×1024. Mark occupies about 62% of canvas width and height with generous uniform padding. No crop, rotation, perspective, cast shadow, or surrounding button.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the mark.

Critical constraints: exactly one ring/check mark only. No text, letters, numbers, words, shield, tree, knife, arrows, coin, socket, panel, badge behind the ring, extra objects, watermark, logo, canvas border, environment, phone, guide lines, or decoration. No medieval ornament, stacked bevels, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```

### Equipped mark

```text
Use case: stylized-concept
Asset type: high-resolution production raster status mark for a Unity mobile UI
Input image: the approved Modern Forest Arcade candidate spritesheet. Use only its separate Equipped shield/conifer mark as the exact visual direction. Recreate it cleanly as one isolated higher-resolution mark; do not include any button surface or other sheet element.

Create exactly one Equipped status mark: a compact upright shield silhouette with a clearly distinct simple central conifer/knife-like emblem. Contemporary casual-game construction, crisp simple silhouette, deep teal shield body with a fresh-teal inner field, restrained thin cream or mint highlight, and thin dark Pine boundary. Front-facing orthographic, centered, readable at 48–64 dp. Strong non-circular silhouette so it cannot be confused with the Owned ring/check. Avoid tiny heraldic details.

Canvas/layout: high-resolution square source, target 1024×1024. Mark occupies about 58% of canvas width and 66% of canvas height with generous uniform padding. No crop, rotation, perspective, cast shadow, or surrounding button.

Background: perfectly flat, uniform #FF00FF chroma-key magenta edge to edge. No background texture, gradient, floor, reflection, lighting variation, or contact shadow. Do not use magenta in the mark.

Critical constraints: exactly one shield/emblem mark only. No text, letters, numbers, words, check, ring, arrows, coin, socket, panel, extra badge behind the shield, extra objects, watermark, logo, canvas border, environment, phone, guide lines, or decoration. No medieval ornament, ornate heraldry, stacked bevels, excessive grain, glow, or photorealism. Crisp alpha-friendly antialiased silhouette.
```
