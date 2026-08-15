# Power-Up Button Art Replacement Plan

Date: 2026-08-08

Repository audit updated: 2026-08-15

Visual target: [Hand-Carved Forest Arcade](PHASE_2_VISUAL_DIRECTION_AND_ASSET_AUDIT.md)

## Implementation Result

Implemented as an art/layout-only replacement on 2026-08-08.

- `PowerUpButtonView` and `PowerUpHudController` retain their original repository logic.
- Shatter and Chrono configuration assets reference the new simplified icons.
- The production prefab is frameless, uses a 200x200 touch target, displays the icon at
  192x192, and places the count/acquisition badge outside the art at left-center.
- Existing activation, cancellation, throw, consumption, inventory, and Chrono cooldown
  behavior is unchanged.
- Project-owned art, source files, provenance, Android import overrides, and the currently
  empty sprite-atlas asset live under `Assets/Art/PowerUps`, `ArtSource/PowerUps`, and
  `Assets/_Docs/PowerUpButtonArt`.

Validation:

- `Game.PowerUps.Tests`: 7 passed, 0 failed, covering activation, cancellation, successful
  throw consumption, lifecycle clearing, unavailable inventory, and world effects.
- Unity compilation: 0 errors.
- Repository diff confirms both gameplay view/controller files match `HEAD` exactly.

The seven-test statement above records the 2026-08-08 art-delivery baseline. The current
suite contains 14 passing EditMode tests, but only three directly exercise button visuals;
see the audited coverage breakdown below.

## UX-408 Repository Audit (2026-08-15)

UX-408 is partially complete. The mechanic, art, and initial active-state work must be
preserved. The remaining ticket is a focused presentation-state closure, not a power-up
rewrite or a second art replacement.

Audit baseline: `main` at `be7c6618` (`feat: add typed feedback output hooks`), before this
documentation-only update. The audit used the serialized production prefab, current source,
test fixtures, repository history, and stored evidence. It did not claim new live runtime or
device validation.

### Implemented source of truth

| Responsibility | Current implementation | Status |
|---|---|---|
| Inventory, activation, cancellation, consumption, and blocking | `PowerUpService` | Complete; do not move into the view. |
| Timed-effect lifetime and expiry | `PowerUpEffectService` | Complete; do not reproduce with UI timers. |
| Stable lifecycle/result semantics | `PowerUpOutcomeEventChannel` with Activated, Consumed, Cancelled, Expired, Cleared, Unavailable, Blocked, InvalidConfiguration, Unsupported, and ExecutionFailed | Complete domain contract. |
| Shatter armed truth | `IPowerUpService.ActiveConfig`, reconciled after use and cancellation | Implemented. |
| Chrono active/progress truth | `IPowerUpService.TryGetWorldEffectProgress` | Implemented and polled by the HUD. |
| Normal count and zero-stock acquisition affordance | `PowerUpButtonView.SetCount` plus the prefab count/plus children | Implemented. |
| Active and unavailable visuals | Active material/pulse plus unavailable tint | Implemented prototype behavior. |
| Chrono radial progress | A filled `Image` driven by authoritative normalized progress | Functionally implemented, but the object is still created at runtime. |
| Authored Chrono ring art | `Assets/Art/PowerUps/PowerUpClockRing.png` | Asset exists, but has no runtime serialized reference. The current generated fill copies the power-up icon sprite instead. |
| Runtime-referenced visual family | Shatter/Chrono icons, active material, count/acquisition badge layout, individual import settings, and source/provenance record | Implemented; preserve for the handoff. |
| Handoff-only visual assets | `PowerUpSocket.png` and `PowerUpClockRing.png` | Present but unreferenced by runtime assets. Do not describe them as shipping integrations. |
| Sprite atlas | `PowerUpHUD.spriteatlasv2` | Asset and settings exist, but `packables` is empty and no sprites are packed. Packing/budget validation remains ART-708/P2-010 work. |
| Background and grayscale review | Four Stage 1 background composite, grayscale composite, and device-reference composites | Implemented as static evidence, not live Game View evidence. |

### Audited state matrix

| Required state | Current behavior | UX-408 closure requirement |
|---|---|---|
| Normal | Count, icon, and normal material are present. | Preserve. |
| Pressed | The prefab `Button` transition is `None`; no presenter-owned pressed response exists. | Add a deterministic pressed treatment without changing activation rules. |
| Shatter armed | Selection is derived from the service and uses the active material/pulse. | Preserve authoritative selection; expose it through the final state model. |
| Chrono active | Service progress drives an active material/pulse and radial fill. | Preserve service authority; move the fill to a named prefab-owned countdown seam. |
| Blocked | The service can emit `Blocked`, but an active Chrono button becomes non-interactable and its handler also returns early, so the HUD does not present repeated-tap rejection. | Route a valid repeated attempt to typed blocked presentation without consuming or restarting the effect. |
| Unavailable | Zero stock shows the unavailable tint and acquisition affordance. | Preserve; include it in deterministic state tests. |
| Gameplay disabled | Fail flow sets a private Boolean only. It does not update visuals or `Button.interactable`; zero-stock acquisition is checked before the gameplay-disabled guard. | Apply a visible, non-interactable disabled state and prevent acquisition UI during fail/transition flow. |
| Consumed/expired/cleared | Domain outcomes exist, but the HUD does not subscribe to the unified outcome channel and has no short end-state presentation. | Add deterministic transient/end-state handling, then reconcile to authoritative count/selection/progress. |
| Reduced motion | The power-up view always pulses while active. The `ActionButtonPresenter` Editor preview is unrelated and is not a shipping preference. | Do not invent product policy in UX-408. Provide only the presentation seam required for UX-406's later decision. |

### Current wiring limitation

`PowerUpHudController` subscribes to the legacy activated, used, and cancelled channels plus
level start. It does not subscribe to `PowerUpOutcomeEventChannel`, so unavailable, blocked,
expired, cleared, invalid, unsupported, and execution-failed outcomes cannot currently drive
button presentation. Chrono progress is authoritative despite this limitation because it is
queried directly from `IPowerUpService` every frame.

This audit does not require a particular class name or duplicate domain state. UX-408 needs
one testable presentation-state owner/reducer between the existing authoritative inputs and
the existing reusable button prefab. It may be a dedicated presenter or an equivalently
separated component, provided state precedence and teardown are explicit.

### Evidence index

| Evidence | Repository path |
|---|---|
| Current button view and runtime-created cooldown fill | `Assets/Scripts/Game/PowerUps/Views/PowerUpButtonView.cs` |
| HUD state/channel wiring and authoritative progress polling | `Assets/Scripts/Game/PowerUps/Views/PowerUpHudController.cs` |
| Authoritative mechanic and outcome emission | `Assets/Scripts/Game/PowerUps/Services/PowerUpService.cs` |
| Timed-effect lifetime and expiry | `Assets/Scripts/Game/PowerUps/Services/PowerUpEffectService.cs` |
| Unified outcome schema and channel | `Assets/Scripts/Game/PowerUps/Data/PowerUpOutcome.cs`, `PowerUpOutcomeEventData.cs`, and `Assets/Scripts/Game/PowerUps/Events/PowerUpOutcomeEventChannel.cs` |
| Reusable production button source | `Assets/Prefabs/UI/PowerUp/PowerUpButton.prefab` |
| Integrated art, handoff-only assets, and empty atlas asset | `Assets/Art/PowerUps` and `ArtSource/PowerUps` |
| Automated fixtures | `Assets/Tests/Game.PowerUps.Tests` |
| Static review evidence and provenance record | `Assets/_Docs/PowerUpButtonArt` |
| Ticket status and dependency boundary | `CORE_GAMEPLAY_REFINEMENT_TRACKER.md` and `GAME_WIDE_UX_UI_PRODUCTION_BACKLOG.md` |

### Current automated coverage

| Test fixture | Cases | What it proves |
|---|---:|---|
| `PowerUpServiceTests` | 7 | Consumption timing, authoritative active state, blocked reactivation without spend, unavailable outcome, cancellation, lifecycle clearing, and expiry. |
| `PowerUpButtonActiveVisualTests` | 3 | Active material restoration, zero-stock timed-effect visibility, and centered pulse scaling. |
| `KnifePowerUpVfxLifecycleTests` | 3 | Knife VFX teardown/reset/camera lifecycle; not button-state coverage. |
| `DeferredGameSceneVisibilityTests` | 1 | Hidden preload rendering; not button-state coverage. |
| **Current total** | **14** | All pass at the recorded baseline, but only three directly validate the button presentation. |

Missing focused coverage includes pressed-state teardown, blocked attempts, gameplay-disabled
precedence, zero-stock behavior while disabled, all unified outcome transitions, expiry/end
reconciliation, countdown component lifecycle, enable/disable subscription safety, and
reduced-motion seam behavior once its policy is approved.

### Evidence status

- Existing `Assets/_Docs/PowerUpButtonArt/Captures` images are deterministic static sprite
  composites. They prove art size/background/grayscale readability, not runtime binding,
  interaction, pulse motion, countdown motion, or transition cleanup.
- No UX-408 runtime video is currently stored in the repository.
- Because pulse, pressed response, countdown, and end states span multiple frames, UX-408
  cannot be marked Done until a short live Game View recording is captured and inspected in
  accordance with the repository motion-validation rule.

### Minimal closure checklist

- [ ] Introduce one presentation-state owner/reducer fed by the existing authoritative
  service progress and unified outcome channel.
- [ ] Preserve separate Shatter-armed and Chrono-active semantics.
- [ ] Add pressed, blocked, gameplay-disabled, and consumed/expired/cleared presentation with
  deterministic precedence and teardown.
- [ ] Replace runtime `new GameObject("CooldownFill")` construction with a serialized,
  reusable child component in `PowerUpButton.prefab`; retain prefab links for both instances.
- [ ] Add focused EditMode tests for the missing state matrix and lifecycle boundaries.
- [ ] Exercise Shatter and Chrono through real HUD input in Play Mode, record a short video,
  inspect it, and verify the Console and hierarchy remain clean.
- [ ] Update this audit and the tracker with exact test totals and evidence paths before
  marking UX-408 Done.

### Explicit ownership boundaries

- UX-408 owns only the state-presentation prototype and final-art integration seam.
- UX-406 owns the product decision and shipping behavior for reduced motion, mute, and
  haptic disable. UX-408 must not silently choose those preferences.
- UX-407 owns the feedback sandbox/debug panel. A production sandbox is not required to
  close UX-408.
- ART-708 owns final frame/icon/badge/timer-state art polish and final device-size approval.
- Power-up inventory, prices, duration, acquisition, consumption, and gameplay effects remain
  outside UX-408.

## Purpose

Replace the current mixed power-up button presentation with one production-ready HUD family.
The Phase 3 radial Chrono sweep remains a functional timing prototype; it is not the final
art treatment.

This work is intentionally split between two phases:

- Phase 4 owns state semantics, reusable presentation behavior, accessibility-ready seams,
  and the production handoff prototype. UX-406 separately owns shipping accessibility policy.
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
5. Expose deterministic state inputs that UX-407 can later preview without spending inventory;
   building the sandbox itself remains UX-407.
6. Expose a reduced-motion presentation seam; the shipping reduced-motion, mute, and haptic
   policy remains UX-406.
7. Capture a gameplay-size prototype over every retained Stage 1 background.

Phase 4 acceptance:

- every state can be driven deterministically by focused tests and is ready for UX-407 to
  expose without spending inventory;
- repeated Chrono input is visibly blocked and consumes nothing;
- the Chrono timer communicates direction and remaining duration without obscuring the icon
  or count;
- Shatter remains visibly armed until use, cancellation, or lifecycle cleanup;
- state meaning survives grayscale review, and no state architecture prevents the later
  UX-406 reduced-motion policy; and
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
