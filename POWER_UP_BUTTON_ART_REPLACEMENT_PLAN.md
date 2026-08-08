# Power-Up Button Art Replacement Plan

Date: 2026-08-08

Visual target: [Hand-Carved Forest Arcade](PHASE_2_VISUAL_DIRECTION_AND_ASSET_AUDIT.md)

## Implementation Result

Implemented as an art/layout-only replacement on 2026-08-08.

- `PowerUpButtonView` and `PowerUpHudController` retain their original repository logic.
- Shatter and Chrono configuration assets reference the new simplified icons.
- The production prefab is frameless, uses a 200x200 touch target, displays the icon at
  192x192, and places the count/acquisition badge outside the art at left-center.
- Existing activation, cancellation, throw, consumption, inventory, and Chrono cooldown
  behavior is unchanged.
- Project-owned art, source files, provenance, Android import overrides, and the sprite atlas
  live under `Assets/Art/PowerUps`, `ArtSource/PowerUps`, and
  `Assets/_Docs/PowerUpButtonArt`.

Validation:

- `Game.PowerUps.Tests`: 7 passed, 0 failed, covering activation, cancellation, successful
  throw consumption, lifecycle clearing, unavailable inventory, and world effects.
- Unity compilation: 0 errors.
- Repository diff confirms both gameplay view/controller files match `HEAD` exactly.

## Purpose

Replace the current mixed power-up button presentation with one production-ready HUD family.
The Phase 3 radial Chrono sweep remains a functional timing prototype; it is not the final
art treatment.

This work is intentionally split between two phases:

- Phase 4 owns state semantics, reusable presentation behavior, accessibility, and the
  production handoff prototype.
- Phase 7 owns final frames, icons, overlays, badges, animation polish, atlasing, and
  gameplay-size art approval.

Tutorial gestures, teaching highlights, copy, and first-use sequencing remain Phase 9 work.

## Approved Visual Direction

- A tactile circular socket built from painted wood, warm metal, and a strong `Ink` edge.
- Compact three-tone icons: `Ink` outline, semantic fill, and one warm highlight.
- Shatter Strike uses the `Ember` and `SunGold` family.
- Chrono Shift uses the `Lagoon` family with readable clock ticks and restrained cool light.
- Count and acquisition badges use the shared HUD resource-chip construction.
- Meaning must remain readable at gameplay size and cannot rely only on hue, glow, sound,
  or motion.

## Required State Matrix

| State | Shared behavior | Shatter Strike | Chrono Shift |
|---|---|---|---|
| Normal | Raised tactile face, full icon, stable count badge. | Warm but inactive icon. | Cool but inactive icon. |
| Pressed | Short depth compression and scale response; input remains readable. | Brief warm edge response. | Brief cool edge response. |
| Active | Distinct silhouette/edge treatment; count remains readable. | Ember/SunGold loaded rim and restrained pulse while the special knife is armed. | Lagoon clock ring with clockwise remaining-time sweep while input is blocked. |
| Blocked | No inventory mutation; short non-color-only rejection plus shared reason callout. | Armed state remains authoritative. | Timer remains authoritative; repeated taps cannot restart or stack it. |
| Unavailable | Reduced saturation and depth plus a clear unavailable/acquisition symbol. | Zero inventory routes to acquisition. | Zero inventory routes to acquisition only after the timer has ended. |
| Gameplay disabled | Static locked treatment during fail/transition flow. | Cannot arm. | Cannot activate. |
| Consumed/expired | Brief completion response, then deterministic return to the correct state. | Return to normal after a successful special throw. | Ring completes/fades, then return to normal or acquisition state. |
| Reduced motion | Replace pulse/compression with value, outline, and icon changes. | Static warm loaded rim. | Static clock ring that still updates remaining time. |

Selected projectile state and active timed-effect state are different semantics and must not
share one ambiguous permanent highlight.

## Phase 4: State-System and Prototype Scope

1. Extend the reusable button-state presenter to cover normal, pressed, active, blocked,
   unavailable, gameplay-disabled, consumed, and expired states.
2. Drive states from `PowerUpOutcomeEventChannel` and authoritative service progress rather
   than inferred colors or local toggles.
3. Replace the runtime-created Chrono overlay with a named, reusable countdown presentation
   seam that can accept final Phase 7 art without mechanic changes.
4. Add Shatter's armed-state presentation without conflating it with Chrono's timed state.
5. Add preview controls to the feedback sandbox for every state and both power-ups.
6. Specify reduced-motion and mute fallbacks.
7. Capture a gameplay-size prototype over every retained Stage 1 background.

Phase 4 acceptance:

- every state is previewable without spending inventory;
- repeated Chrono input is visibly blocked and consumes nothing;
- the Chrono timer communicates direction and remaining duration without obscuring the icon
  or count;
- Shatter remains visibly armed until use, cancellation, or lifecycle cleanup;
- state meaning survives grayscale and reduced-motion review; and
- Phase 7 can replace sprites/materials without changing domain code.

## Phase 7: Final Art Production Scope

Produce and integrate:

- one shared power-up socket/frame family;
- normal, pressed, disabled, blocked, and active frame treatments;
- a final Shatter Strike icon and Ember/SunGold armed rim;
- a final Chrono Shift icon, Lagoon clock ring, ticks, and countdown mask;
- shared count, plus/acquisition, unavailable, and locked badges;
- reduced-motion variants;
- atlas/import settings matching the Phase 2 Android technical-art contract; and
- before/after captures at device resolution.

Final acceptance:

- both buttons clearly belong to the Hand-Carved Forest Arcade HUD;
- icons remain identifiable at 64-96 dp and without color;
- count and timer remain legible without entering the target/collision lane;
- normal, pressed, active, blocked, unavailable, and disabled states are distinguishable in
  a two-second glance test;
- no state depends on a permanent bloom layer;
- the retained backgrounds do not reduce state readability below the approved contrast
  budget; and
- Android texture residency and atlas rules remain within the Phase 2 budget.

## Replacement and Cleanup Rules

- Preserve the current mechanic and outcome contracts while replacing presentation assets.
- Do not delete source art until references, provenance, and licenses are verified.
- Remove current production-facing button/icon assets only after the new prefab passes
  gameplay-size regression and no shipping references remain.
- Record source, author, license, generation method, and modification history for every new
  art asset.

## Explicit Non-Goals

- Tutorial hand gestures or copy.
- Choosing which power-up is taught first.
- Changing inventory, pricing, duration, consumption, or acquisition rules.
- Adding full-screen Chrono blur or effects that obscure the target lane.
