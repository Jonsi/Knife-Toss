# Phase 2 Visual Direction and Asset Audit

Date: 2026-08-08

Roadmap phase: [Phase 2 - Visual Direction Lock and Asset Audit](CORE_GAMEPLAY_REFINEMENT_PHASES.md#phase-2---visual-direction-lock-and-asset-audit)

## Outcome

The core visual target is approved as **Hand-Carved Forest Arcade**: a colorful,
high-contrast forest-fantasy toy world built from painted wood, matte stone, warm metal,
bold warm outlines, and compact arcade feedback. The existing forest gameplay and strongest
wooden UI establish the target. Generic flat UI, default-looking form controls, unrelated
purple panels, placeholder world composites, and development overlays do not belong.

An asset belongs in Toss Madness when it:

1. feels playful, tactile, and hand-crafted rather than clinical or generic;
2. uses the approved palette, warm outline language, and painted material treatment;
3. remains legible at gameplay size and never competes with the target, knife, or hazards;
4. communicates state with shape and value as well as color;
5. can be expressed through the shared UI/VFX tokens instead of a one-screen exception; and
6. has a recorded source and a usage right that can be proven before release.

This phase locks direction and production rules. It does not produce later-world art or
perform a risky bulk reorganization of imported libraries.

## Evidence Package

- [Current-state contact sheet](Assets/_Docs/Phase2VisualAudit/ContactSheet.png)
- [Per-asset disposition matrix](Assets/_Docs/Phase2VisualAudit/Data/DispositionByAsset.csv)
- [Texture import audit](Assets/_Docs/Phase2VisualAudit/Data/TextureImportAudit.csv)
- Referenced-asset binding inventories:
  [Loading](Assets/_Docs/Phase2VisualAudit/Data/LoadingReferencedAssets.csv),
  [Main Menu](Assets/_Docs/Phase2VisualAudit/Data/MainMenuReferencedAssets.csv),
  [Store](Assets/_Docs/Phase2VisualAudit/Data/StoreSceneReferencedAssets.csv),
  [Stages](Assets/_Docs/Phase2VisualAudit/Data/StagesMenuReferencedAssets.csv), and
  [Gameplay](Assets/_Docs/Phase2VisualAudit/Data/StagePlayReferencedAssets.csv)
- [Capture and data notes](Assets/_Docs/Phase2VisualAudit/README.md)

The five live scene scans contain 228 concrete component-to-asset bindings covering 75
unique referenced asset paths. Inactive popup states were included. Runtime-generated solid
colors and Unity UI elements with no source asset are covered by the screen-level matrix.

## Player-Facing State Audit

| State | Disposition | Production direction |
|---|---|---|
| Loading | Replace | Replace the blurred blue forest and plain text with an approved forest plate, logo treatment, and a small tokenized progress indicator. |
| Main menu | Refine | Keep the colorful forest, logo, selector, and tactile platform; normalize icons, button hierarchy, spacing, and release-safe debug state. |
| Daily missions | Refine | Keep the wood/parchment construction; enlarge progress and claim hierarchy and replace generic green claim buttons with shared states. |
| Store | Refine | Keep the wooden frames, knife presentation, and power-up imagery; resolve clipped horizontal discovery and standardize price/owned/selected states. |
| Stage selection | Refine | Keep the forest-framed structure; remove duplicate/old composites from shipping presentation and label or hide unfinished worlds. |
| Gameplay HUD | Keep / Refine | Keep the forest target and strong knife silhouette; simplify the top/bottom chrome and reserve the center interaction lane. |
| Settings | Replace | Rebuild the undersized controls as a reusable modal with large rows, clear state feedback, and safe-area spacing. |
| Privacy | Replace | Replace cramped placeholder copy and the plain green action with readable body typography, scroll affordance, and shared buttons. |
| Report error | Replace | Replace default white form controls with the approved modal, field, validation, success, and blocked-feedback tokens. |
| Add power-up | Replace | Rebuild the tiny bottom strip as a clear acquisition sheet with price, inventory, ad alternative, and close/back hierarchy. |
| Failure | Refine | Preserve the interrupted-game context; rebuild the mixed copy/actions so Retry is primary and Continue/Start Over are unmistakable. |
| Completion reward | Replace | Replace the flat purple panel and generic green button with a short, non-blocking gold-to-leaf reward beat. |
| Tutorial gesture | Refine | Replace the unexplained gesture/rocket-like symbol with an animated hand/trajectory cue plus concise truthful copy. |
| SRDebugger launcher | Remove | Development-only overlay; Phase 1 production validation must keep it out of release builds. |

`PVPPlay` and `PowerUpsMenu` are not in Android build settings and have no live player
navigation in the audited flow, so they are not part of the shipping-state contact sheet.
Mock ad/IAP presentation is development-only and governed by the Phase 1 production gate.

## Referenced Asset Disposition

The machine-readable matrix assigns every one of the 75 referenced asset paths exactly one
status and a reason:

| Disposition | Count | Meaning |
|---|---:|---|
| Keep | 16 | Already supports the target or is an approved engine/font/material dependency. |
| Refine | 44 | Useful foundation that needs palette, outline, material, hierarchy, timing, or provenance normalization. |
| Replace | 12 | Visibly conflicts with the target or relies on generic/default construction. |
| Remove | 3 | Old/duplicate configuration art that should not appear in shipping presentation. |

Referenced-source families are 36 Layer Lab UI paths, 20 legacy `Assets/Art` paths, seven
world/configuration paths, five CartoonVFX9X paths, three project materials, one TextMesh Pro
font, two Unity package defaults, and one Unity built-in asset.

The imported folders are much larger than the live dependency set: `Assets/Layer Lab` is
approximately 263 MB and `Assets/CartoonVFX9X` approximately 444 MB, while only 41 paths
across those two families appear in captured states. Folder presence is therefore an
unused-library issue, not proof of visible inconsistency. Keep unused pack content out of
production-facing palettes, `Resources`, and future atlases; do not delete or relocate it
until dependency and license review is complete.

## Provenance and Release Gate

| Family | Source record | License / proof state | Decision |
|---|---|---|---|
| GUI Pro - Casual Game | LAYERLAB; local `LayerLab_UserAssetGuide.txt` identifies the pack and its Lilita One dependency. | The repository does not contain the Asset Store receipt/entitlement record. Unity's standard Asset Store EULA is the default framework, but entitlement for this project must be attached. | Retain conditionally; replace if entitlement cannot be proven. |
| Lilita One | Google Fonts; copyright Juan Montoreano; local pack guide and upstream font repository agree. | SIL Open Font License 1.1 is identified upstream. | Keep for display/button typography and preserve the license notice. |
| Liberation Sans | TextMesh Pro essential resources. | Local `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. | Keep for body/legal copy and preserve the license notice. |
| CartoonVFX9X | Local readmes identify Le Tuan Anh/support contact. | No acquisition receipt or explicit license file was found in the repository. | Provenance hold; retain only after proof is attached, otherwise replace the five referenced effects. |
| Legacy `Assets/Art` and world/config sprites | Present in repository; original creator/source is not recorded. | No author declaration or usage-right record was found. | Owner must record authorship/rights or replace before release. |
| Project materials | Authored in this project and inherit referenced textures/shaders. | Dependency provenance governs the final status. | Keep the material setup; resolve dependent art. |
| Unity built-in / URP / TMP package defaults | Supplied with the Unity project and packages. | Governed by the applicable Unity/package terms. | Keep. |

Reference links: [Unity Asset Store Terms](https://unity.com/legal/as-terms) and
[Lilita One OFL record](https://github.com/google/fonts/blob/main/ofl/lilitaone/OFL.txt).

The audit is complete, but Phase 2 remains **In progress** until marketplace entitlement
receipts and an owner authorship/usage-right declaration are attached—or every affected
retained asset is moved to Replace. No new production work should deepen dependency on an
asset still under provenance hold.

## Lightweight Art Bible

### Tone and Composition

- Emotional target: joyful tension, quick mastery, playful danger, and satisfying craft.
- Reference world: a hand-built forest carnival/workshop, not a medieval realism scene and
  not a flat mobile utility interface.
- One focal hierarchy per state: target/knife during play, primary action in menus, earned
  reward during completion.
- Decorative detail stays outside the central interaction lane and must survive a blurred,
  grayscale, and small-screen check.

### Palette Tokens

| Token | Hex | Use |
|---|---|---|
| `Ink` | `#24160F` | Outlines, highest-contrast labels, silhouette separation. |
| `DeepCanopy` | `#123D2B` | Background depth and dark scrims. |
| `Leaf` | `#45C968` | Positive/success state; never the only success signal. |
| `Lagoon` | `#19BFC4` | Secondary action, time/slow effects, cool focus. |
| `SunGold` | `#FFC43D` | Rewards, primary focus, progress completion. |
| `Ember` | `#F26A2E` | Hazard, urgency, collision warmth. |
| `Failure` | `#E84B3C` | Failure/error paired with icon, motion, and copy. |
| `Parchment` | `#E8C79B` | Modal surfaces and readable warm fields. |
| `WoodMid` | `#8B4B24` | Primary structural surface. |
| `WoodDark` | `#4A2617` | Panel edges and depth. |
| `DimScrim` | `#07170FCC` | Modal focus without losing gameplay context. |
| `TextIvory` | `#FFF4DE` | Primary text on dark/wood surfaces. |

### Shape, Outline, and Materials

- Use rounded chunky silhouettes with slightly imperfect carved edges and clear massing.
- Primary silhouettes use a warm `Ink` outline approximately 2-4% of the element's shortest
  dimension; internal detail uses a lighter/thinner edge.
- Preferred materials are painted wood, matte stone, parchment, rope, leaves, and warm
  brushed metal. Avoid glassy sci-fi panels, untextured white fields, and isolated flat
  purple/green rectangles.
- Depth is communicated by a consistent lower-right shadow, small bevel/highlight, and
  occlusion—not by unrelated glow styles.

### Typography and Icons

- Lilita One is the display family for logo-adjacent headings, scores, and short button
  labels. Liberation Sans is the body/legal/accessibility family.
- Body text targets at least 4.5:1 contrast; large display text at least 3:1. Never place
  body/legal copy inside a low-contrast textured region without a quiet surface.
- Icons use a compact three-tone construction: `Ink` outline, semantic fill, and one warm
  highlight. They remain identifiable without color and at 64-96 dp.
- Currency, action, state, and navigation icons each use one canonical silhouette. Do not
  mix thin flat-vector symbols with painted/chunky icons.

### UI Tokens

| Token | Contract |
|---|---|
| `UI.PrimaryPanel` | Painted wood frame, parchment/quiet center, 16-24 dp inner padding. |
| `UI.Modal` | Primary panel + `DimScrim`; title, content, actions, and close affordance follow one grid. |
| `Button.Primary` | Sun-gold/parchment face, wood edge, Ink label; strongest scale and contrast. |
| `Button.Secondary` | Lagoon/wood treatment; visibly subordinate without appearing disabled. |
| `Button.Danger` | Ember/red plus warning silhouette and explicit verb. |
| `Button.Disabled` | Reduced saturation/value contrast, static depth, and locked/unavailable icon where useful. |
| `UI.ResourceChip` | Canonical currency icon, stable number width, gain/spend pulse hooks. |
| `UI.FormField` | Parchment inset, Ink text, clear focus/invalid/success states. |
| `UI.Scrim` | `DimScrim`; no interaction leaks outside the modal. |
| `UI.FocusGlow` | Short SunGold or Lagoon edge pulse, never a permanent bloom layer. |

Use an 8 dp layout grid, 16/24/32 dp spacing steps, a 48 dp minimum touch target, and
device safe-area insets. Controls need pressed, selected, disabled, blocked, loading,
success, and owned/unavailable states when semantically relevant.

### VFX Tokens

| Token | Shape / color | Timing and readability |
|---|---|---|
| `Hit.Target` | White/SunGold core with wood chips. | 100-180 ms impact; no more than 15% target occlusion. |
| `Hit.Hazard` | Ember/Failure sparks with a sharp opposing burst. | 250-400 ms; unmistakable from a valid hit without relying on red alone. |
| `Collect` | SunGold/Lagoon particles traveling toward the resource chip. | 300-550 ms; value applies independently of presentation completion. |
| `Complete` | SunGold impact resolving to Leaf progress/reward motion. | 600-1000 ms, non-modal, next target remains prompt. |
| `Shatter` | Angular wood/stone fragments inheriting target material. | Strong initial impulse, fast visual clearance from the throw lane. |
| `Power.SuperKnife` | Warm Ember/SunGold wedge and piercing trail. | Preserves knife silhouette and hazard readability. |
| `Power.Chrono` | Lagoon rings/ticks with restrained edge desaturation. | World speed change remains readable without full-screen blur. |

Every motion token needs a reduced-motion variant using shorter fades, scale/value changes,
and no meaning carried only by shake.

## Gameplay Readability Checklist

- [ ] Target diameter stays roughly 28-36% of portrait screen width in the core slice.
- [ ] Knife length stays roughly 12-17% of screen width and its blade/handle silhouette is
  recognizable against every approved background.
- [ ] Knife and stuck-hazard edges have at least 1.5 outline widths of separation at the
  intended collision-reading distance.
- [ ] The central throw lane is free from decorative UI and persistent VFX.
- [ ] Persistent HUD content stays primarily within the top and bottom 12% bands.
- [ ] Target impact VFX obscures no more than 15% of the target; a full-screen flash, if
  used, is no longer than 80 ms.
- [ ] Hazard, success, failure, and collectible states use shape/motion/copy as well as
  color, and pass a grayscale check.
- [ ] Body text meets 4.5:1 contrast and large text/UI boundaries meet 3:1 where applicable.
- [ ] Each state passes checks at native phone size, 50% screenshot scale, and over the
  brightest and darkest retained backgrounds.
- [ ] Tutorial callouts point to an available action and do not cover the target/knife
  collision read.

## Android Sprite and Memory Standards

### Reference and Import Rules

- Author against a 1080 x 1920 portrait reference, `Scale With Screen Size`, and 0.5
  width/height match; validate safe areas separately.
- UI and core gameplay sprites: sRGB on, mipmaps off, Read/Write off, Bilinear filtering.
  Pixel-authored exceptions must be documented and kept consistent as a family.
- Preferred Android compression: ASTC 4x4 for knife, target, logo, and critical text-bearing
  art; ASTC 6x6 for ordinary UI/world art. Produce a separate ETC2 compatibility variant if
  the supported-device matrix requires it; do not use Crunch as the ASTC strategy.
- Maximum imported size: 256 for small icons, 512 for controls/cards, 1024 for panels and
  ordinary atlases, and 2048 for full-screen backgrounds/large shared atlases. Source size
  alone does not justify a larger runtime texture.
- Keep font atlases at 1024 unless measured glyph coverage requires more; curate character
  sets per supported locale.

### Atlas and Memory Rules

- Create SpriteAtlas V2 groups for `CoreUI`, `GameplayCommon`, `World01`, and `SecondaryUI`.
  Use maximum 2048 atlases, 4-8 px padding, tight packing only where it does not create
  sampling artifacts, and 15% headroom for iteration.
- Never pack entire imported libraries, mutually exclusive worlds, or provenance-held art
  into a shared always-loaded atlas. Do not introduce obsolete packing tags.
- Authored peak targets for the core Android slice: active UI textures <=32 MB,
  gameplay/world textures <=48 MB, active VFX textures <=16 MB, and total authored texture
  residency <=96 MB on the mid-tier validation device. Measure in a player build; these are
  budgets, not current profiler results.
- World-specific content should be addressable/unloaded by world. Do not move broad packs
  into `Resources`.

The current audit found 58 referenced sprite textures: all use Bilinear filtering with
mipmaps off, but all also rely on generic 2048 maximum size/compression, have no Android
platform override, no packing tag, and no SpriteAtlas asset exists. Phase 7 implements and
profiles these standards; Phase 2 only locks them.

## Production Sequence

1. Phase 4 may build the shared UI states and feedback presenters from these tokens.
2. Phase 7 first replaces Loading, Settings/Privacy/Report, Failure/Completion, and the
   tutorial cue, then normalizes Main Menu/HUD/Store/Missions/Stages.
3. Provenance-held dependencies must be resolved before refined derivatives are approved.
4. Later-world naming, backgrounds, targets, and bulk props remain Phase 10 work.

## Exit Review

| Criterion | Result |
|---|---|
| Explain what belongs | Pass: Hand-Carved Forest Arcade belonging test and art bible are locked. |
| Every visible asset has a disposition | Pass: 75/75 referenced assets plus runtime/default screen elements are covered. |
| UI/VFX can proceed without per-screen invention | Pass: palette, typography, material, UI, VFX, layout, readability, and Android tokens are defined. |
| Later-world production remains out of scope | Pass: later worlds are deferred to Phase 10. |
| Retained assets have proven usage rights | Pending external evidence: marketplace receipts and legacy-owner declaration, or replacement. |
