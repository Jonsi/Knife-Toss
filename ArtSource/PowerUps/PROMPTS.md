# Power-Up Art Generation Prompts

Generated with the OpenAI built-in image generation tool on 2026-08-08.

All prompts required a perfectly uniform `#ff00ff` chroma-key background with no shadow,
gradient, texture, reflection, floor, watermark, or key color inside the subject.

## PowerUpSocket_Source.png

Production Unity mobile HUD sprite: one centered tactile circular socket for the Hand-Carved
Forest Arcade UI. Raised hand-painted wooden construction, warm brushed-brass inner rim,
strong `Ink` outline, empty central opening for a separately layered icon, exact front-facing
orthographic circle, slightly imperfect carved edges, upper-left bevel highlight, contained
lower-right depth, and the `Ink`, `WoodMid`, `WoodDark`, `Parchment`, and restrained
`SunGold` palette. Readable at 64-96 dp. No icon, text, glow, crop, or outside cast shadow.

## ShatterStrikeIcon_Source.png

Production Unity mobile HUD icon: one stout diagonal throwing knife punching through a
cracked circular wooden target with three bold angular fracture wedges. Strong grayscale
silhouette at 64-96 dp; polished hand-painted, chunky three-tone construction using `Ink`,
`Ember`, `SunGold`, and a small parchment blade face. One icon only, one knife, one target,
thick outline, no enclosing frame, text, glow, tiny debris, or photorealism.

## ChronoShiftIcon_Source.png

Production Unity mobile HUD icon: one chunky round clock with twelve readable ticks, two
bold hands, and one clockwise curved arrow integrated into the silhouette. Static silhouette
must communicate time slowing and survive grayscale at 64-96 dp. Painted-enamel and warm
metal construction using `Ink`, `Lagoon`, `SunGold`, and a small parchment face. One icon
only, no enclosing frame, text, numbers, tiny gears, glow, or photorealism.

## PowerUpBadges_Source.png

Production Unity mobile HUD 2x2 badge sheet with equally sized, isolated orthographic
badges and wide key-color gutters: empty rounded count chip; acquisition badge with one bold
`SunGold` plus; unavailable badge with a diagonal slash over an empty pouch; gameplay-lock
badge with one chunky closed padlock. Hand-painted wood/metal, chunky three-tone construction,
clear in grayscale at 32-48 dp, and no letters, numbers, extra icons, glow, or overlap.

## 2026-08-08 readability simplification pass

The socket and both primary icons were regenerated from their prior versions with the
OpenAI built-in image-generation tool, then converted from a flat `#ff00ff` chroma key to
alpha. This pass deliberately favors large shapes and instant phone-size recognition.

- `PowerUpSocket_Source.png`: one smooth brown ring and one thin gold inner line; no bolts,
  tabs, grain, grooves, seams, scratches, or ornament.
- `ShatterStrikeIcon_Source.png`: one orange target disk, one strong crack, and one pale
  knife silhouette; no growth rings, debris, tiny shards, or texture.
- `ChronoShiftIcon_Source.png`: one turquoise clock disk, two pale hands, four quarter marks,
  and one short gold arrow; no minor ticks, extra rings, gears, texture, or decoration.

All three prompts required a uniform magenta background, centered composition, generous
padding, no text, and a maximum of a few broad color/value bands.

`PowerUpBadges_Source.png` was also replaced in this pass with deterministic flat artwork:
four plain circular badges (count, add, unavailable, locked), using only `Ink`, cream,
muted gray, and gold. It contains no generated texture or ornamental surface detail.
