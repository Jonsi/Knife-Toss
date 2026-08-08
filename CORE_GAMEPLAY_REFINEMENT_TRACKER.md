# Toss Madness Core Gameplay Refinement Tracker

Last updated: 2026-08-08

Companion roadmap: [Core Gameplay Refinement Phases](CORE_GAMEPLAY_REFINEMENT_PHASES.md)

Related design: [Game Design Document](GAME_DESIGN_DOCUMENT.md), [First 10 Minutes Design Pass](FIRST_10_MINUTES_DESIGN_PASS.md)

## Purpose

This is the execution tracker for taking the current game from a functional arcade loop to
a coherent, testable core vertical slice. It covers runtime reliability, onboarding,
power-ups, feedback, failure and completion flow, art direction, reusable core art,
secondary-screen UX, release safety, and final playtesting.

Bulk production of later worlds is intentionally deferred until the reusable core loop and
visual language pass their gates.

## Status Legend

| Status | Meaning |
|---|---|
| Done | Implemented and validated with recorded evidence. |
| In progress | Actively being implemented or validated. |
| Ready | Scoped and unblocked; safe to start. |
| Needs decision | Product or art direction must be settled before implementation. |
| Blocked | Cannot proceed because a named dependency is incomplete. |
| Deferred | Intentionally outside the current core-loop milestone. |

## Current Product Position

The core throw-and-avoid loop is viable. Real-input playtesting showed responsive throws,
clear stuck-knife hazards, effective target cracking, and a useful early difficulty ramp.
The largest gap is not the basic mechanic; it is consistency around the mechanic:

- tutorial prompts and inventory state do not always agree;
- feedback is inconsistent across gameplay, failure, rewards, and store actions;
- stage completion has intentionally uninterrupted pacing but lacks a completion beat;
- scene transitions briefly create duplicate systems;
- debug tooling is not sufficiently separated from production builds;
- the visual result mixes strong bespoke pieces, generic asset-pack elements, placeholders,
  and missing later-world content without a single curation standard.

## Approved Direction

- Automatic progression after stage completion is deliberate and should remain.
- Completion feedback should be short and non-blocking, led by VFX, sound, animation, and
  possibly a run reward accumulator.
- Cheats are valid for mechanic QA; the store and clean-save flow must still be tested as
  player-facing acquisition paths.
- Placeholder worlds and bulk later-world art are deferred until the core loops are stable.
- Debug UI and test-only systems must become explicit release-planning gates.
- Duplicate EventSystem and AudioListener transition warnings should be resolved.
- UX feedback is a game-wide system, not a collection of one-off button fixes.
- Core art refinement belongs in the current plan; bulk campaign art does not.
- The approved core visual target is **Hand-Carved Forest Arcade**: colorful forest-fantasy
  readability, warm ink outlines, painted wood/stone/metal, Lilita One display typography,
  Liberation Sans body typography, and one semantic UI/VFX palette.
- Final tutorial design and implementation are deferred to Phase 9, after mechanics,
  progression, economy, UI, feedback, and presentation are stable. Early work exposes
  truthful state contracts and generic guidance primitives only.

## Baseline Evidence

- [x] Main menu, daily missions, store, stage selection, gameplay, failure, restart,
  power-up buttons, all three Stage 1 levels, and the Stage 2 handoff were exercised.
- [x] A failure was caused with an actual timed collision and restarted through the real UI.
- [x] Stage 1 was completed with real Input System events rather than direct callbacks.
- [x] Playtest save mutations were restored after validation.
- [x] The Editor-only MCP harness compiles without errors.
- [ ] Audio quality, haptics, touch ergonomics, safe areas, and device performance have been
  evaluated on representative phones.

## Phase Board

| Phase | Name | Status | Primary exit signal |
|---|---|---|---|
| 0 | Repeatable Playtest Foundation | Done | Real-input success, failure, restart, telemetry, and save restoration are repeatable. |
| 1 | Runtime Transition and Release Hygiene | In progress | Runtime and Android artifact pass; iOS artifact and release-identity decisions remain. |
| 2 | Visual Direction Lock and Asset Audit | In progress | Direction and inventory are complete; entitlement and legacy-art usage-right evidence remain. |
| 3 | Power-Up Mechanic Truth | Done | Both power-ups, acquisition/persistence, cleanup, and typed outcome contracts passed real-input and automated QA. |
| 4 | Shared UX Feedback Foundation | In progress | UX-401 now defines the shared typed event and arbitration contract; presenter work remains. |
| 5 | Failure, Retry, and Continue Polish | Blocked | Depends on the shared feedback foundation. |
| 6 | Continuous Completion and Reward Accumulation | Needs decision | Completion sequence and reward accumulation semantics are approved and implemented. |
| 7 | Core Vertical-Slice Art, VFX, and Audio Refinement | Blocked | Depends on stable loops and an approved visual target. |
| 8 | Secondary Screens and Meta UX Consistency | Blocked | Depends on the shared feedback foundation and core UI kit. |
| 9 | Tutorial Integration, Full Journey, Device, and Release Validation | Blocked | Final tutorial is integrated against stable systems; fresh-save journey and production build pass. |
| 10 | World Identity and Content Expansion | Deferred | Begins only after the core vertical slice passes Phase 9 validation. |

## Work Item Tracker

### Phase 0 - Repeatable Playtest Foundation

| ID | Work item | Priority | Status | Evidence / notes |
|---|---|---:|---|---|
| QA-000 | Install and connect Unity MCP for this project | P0 | Done | Package is present in the project manifest and lock file. |
| QA-001 | Add Editor-only real-input playtest harness | P0 | Done | `GameplayPlaytestHarness` queues Input System mouse state and never calls gameplay callbacks. |
| QA-002 | Capture target, knife, button, timing, and pending-input telemetry | P0 | Done | `SnapshotJson()` and console traces are available. |
| QA-003 | Add calculated safe-hit and intentional-collision actions | P0 | Done | Both paths succeeded in live Play Mode. |
| QA-004 | Document foreground/framerate timing requirement | P1 | Done | Included in the harness README. |
| QA-005 | Add repeatable clean-save setup/restore workflow | P1 | Ready | Current restoration was manual; make it a guarded Editor action before broader regression use. |

### Phase 1 - Runtime Transition and Release Hygiene

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| TECH-101 | Map ownership and lifetime of EventSystems across persistent and content scenes | P0 | Done | Ownership contract and handoff sequence documented in `RuntimeSceneAuthority.md`. |
| TECH-102 | Enforce exactly one active EventSystem during every transition | P0 | Done | Preload, reveal, Store x10, Stages x10, and failure-return soak produced no duplicate warnings. |
| TECH-103 | Enforce exactly one active AudioListener during every transition | P0 | Done | Ownership snapshots show only the intended scene listener enabled. |
| TECH-104 | Verify hidden/preloaded StagePlay cannot consume input before reveal | P0 | Done | Hidden StagePlay EventSystem, input module, canvases, and raycasters are disabled until reveal. |
| BUILD-101 | Define development, QA, and production build profiles | P0 | Done | Six Android/iOS profiles define explicit flavor symbols and development settings. |
| BUILD-102 | Remove `TEST_ENABLED`, SRDebugger, cheats, mock ads, and mock IAP from production | P0 | In progress | Android Production APK scan is clean across metadata, native symbols, and serialized data; iOS archive scan remains. |
| BUILD-103 | Add a build-time failure for forbidden production symbols/components | P0 | Done | Policy rejects unsafe symbols and enabled debug content; profile-aware staging removes test sources/Resources before production collection. |
| BUILD-104 | Confirm Android application ID and product label | P0 | Needs decision | Android identity is restored to the historical Firebase-registered `com.CuzGames.TossMadness`; the `Toss Mdaness` product label and store continuity still require release-owner confirmation. |
| BUILD-105 | Triage Android production build warnings | P1 | Ready | Build succeeded with 0 errors and 992 warnings, primarily Sentis shader variants plus a compressed icon warning. |
| TECH-105 | Run menu/game/store/stages transition soak | P1 | Done | Real-input Store and Stages round trips x10 plus gameplay failure return; zero warnings/errors. |

### Phase 2 - Visual Direction Lock and Asset Audit

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| ART-201 | Capture every player-facing screen and core gameplay state | P0 | Done | 13-state contact sheet covers menu, gameplay, fail, store, missions, stages, rewards, settings, and popups. |
| ART-202 | Inventory assets actually referenced by those screens | P0 | Done | Five live scene scans record 228 bindings to 75 unique paths, separately from unused packs. |
| ART-203 | Mark each visible asset Keep, Refine, Replace, or Remove | P0 | Done | The 75-row disposition CSV records 16 Keep, 44 Refine, 12 Replace, and three Remove decisions. |
| ART-204 | Approve the core visual target | P0 | Done | Hand-Carved Forest Arcade is approved in `PHASE_2_VISUAL_DIRECTION_AND_ASSET_AUDIT.md`. |
| ART-205 | Create a lightweight art bible and UI/VFX tokens | P0 | Done | The approved document defines palette, type, shape, material, layout, UI, and VFX tokens. |
| ART-206 | Define readability budgets for target, knife, hazards, collectibles, HUD, and VFX | P0 | Done | Checklist sets size, occlusion, interaction-lane, contrast, and reduced-motion limits. |
| ART-207 | Audit source/license/provenance for retained third-party and generated art | P1 | In progress | Font records are verified; marketplace receipts and legacy-art ownership/usage evidence must be attached or affected assets replaced. |
| ART-208 | Quarantine unrelated or unused asset-pack content from production-facing folders | P2 | Done | Production policy excludes broad packs from Resources/atlases and forbids destructive relocation until dependency/license review. |

Current inventory signal: `_StageAssets` contains usable image content for the basic set,
Elder Root, and Ignis Pit, while the named folders for Moss Sanctum through Void Gate are
currently empty. The broader art tree also includes a large generic flat-icon library and
multiple unrelated VFX/audio packs. This is an asset-curation problem as well as a missing-
art problem.

### Phase 3 - Power-Up Mechanic Truth

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| PWR-302 | Document and validate the cheat workflow for granting each power-up | P0 | Done | Android Development `TEST_ENABLED` profile and `Tools > Cheats` workflow documented; no save-file surgery. |
| PWR-303 | Test Super Knife from grant through throw, collision, consumption, and feedback | P0 | Done | Real HUD activation and gameplay tap consumed one unit, destroyed the target in one throw, and advanced cleanly. |
| PWR-304 | Test Slow Time duration, speed change, consumption, and recovery | P0 | Done | Real input measured 0.30 for five seconds, restored 1.00, and emitted Activated/Consumed/Expired. |
| PWR-301 | Fix stale Slow Time selected state after immediate consumption | P0 | Done | Selection now derives from `ActiveConfig`; final selection is false during and after the immediate world effect. |
| PWR-305 | Validate clean-save acquisition and persistence through the store | P0 | Done | Zero-count popup purchase spent 250, persisted six units, survived Play Mode stop, and rejected insufficient funds safely. |
| PWR-306 | Define stable power-up outcome events for shared feedback | P0 | Done | Unified channel covers activation, consumption, cancellation, expiry, cleanup, availability/block/failure reasons. |
| PWR-307 | Keep cheat-assisted mechanic QA separate from clean-save economy QA | P1 | Done | Evidence labels controlled cheat setup separately from real UI activation, purchase, rejection, and persistence paths. |

### Phase 4 - Shared UX Feedback Foundation

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| UX-401 | Define feedback events and priority rules | P0 | Done | `FeedbackEventChannel` and `FeedbackEventSystem.md` cover all nine semantics with explicit priority, FIFO queueing, interruption, cooldown, duplicate, overflow, and teardown rules; 19/19 focused EditMode cases pass. |
| UX-402 | Build reusable button pressed/disabled/loading/blocked presentation | P0 | Ready | Phase 2 now supplies the approved UI tokens and state language. |
| UX-403 | Build reusable toast/callout presenter with queue and deduplication | P0 | Ready | UX-401 supplies the tested event and arbitration contract; no presenter was started as part of UX-401. |
| UX-404 | Add consistent currency gain/spend animation and sound hooks | P0 | Ready | UX-401 now supplies the presentation-neutral shared event contract. |
| UX-405 | Add typed VFX/audio/haptic hooks without coupling domain services to views | P0 | Ready | Each feedback event can fan out to approved presenters. |
| UX-406 | Define reduced-motion, mute, and haptic-disable behavior | P1 | Needs decision | Accessibility variants have an explicit product policy. |
| UX-407 | Build a feedback sandbox/debug panel | P1 | Ready | Every feedback state can be previewed without changing economy data. |
| UX-408 | Prototype authoritative power-up button states and prepare the final-art handoff | P0 | In progress | The art/layout prototype is integrated and 14/14 current power-up EditMode tests pass, but the separate authoritative-state presenter and recorded runtime motion evidence are still outstanding; original gameplay logic remains unchanged. |

### Phase 5 - Failure, Retry, and Continue Polish

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| FAIL-501 | Make knife-on-knife failure cause unmistakable | P0 | Blocked | Depends on UX-405 and the Phase 7 production cue pass. |
| FAIL-502 | Make Retry the primary action and Start Over a clear secondary button | P0 | Ready | First-time users identify both actions without explanation. |
| FAIL-503 | Replace awkward zero-cost presentation | P1 | Ready | Free continue is labeled meaningfully instead of `0` currency. |
| FAIL-504 | Define when Continue is free, ad-based, currency-based, or absent | P0 | Needs decision | Rules cover first session and repeat attempts. |
| FAIL-505 | Verify failure-to-input-ready pacing | P1 | Ready | Dramatic beat is present without feeling sluggish. |
| FAIL-506 | Validate retry, restart, continue, ad failure, and insufficient-funds paths | P0 | Blocked | Depends on FAIL-504 and Phase 4. |

### Phase 6 - Continuous Completion and Reward Accumulation

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| COMP-601 | Approve a non-blocking final-hit completion timeline | P0 | Needs decision | Timing specifies hit-stop, shatter, progress pulse, reward travel, and next target arrival. |
| COMP-602 | Decide reward accumulator semantics | P0 | Needs decision | Defines what accumulates, when it is saved, and where it is summarized. |
| COMP-603 | Implement completion cue orchestration without delaying automatic progression excessively | P0 | Blocked | Depends on COMP-601 and Phase 4. |
| COMP-604 | Add visible run/stage reward accumulation | P0 | Blocked | Depends on COMP-602. |
| COMP-605 | Make progress-bar completion and next-stage handoff readable | P1 | Ready | Player understands completion and the next goal during continuous play. |
| COMP-606 | Verify rewards cannot be duplicated or lost during interruption | P0 | Blocked | Depends on COMP-602 and implementation. |

Recommended starting hypothesis: a 0.6-1.0 second non-modal completion beat, with a short
impact pause, stronger shatter, progress pulse, earned currency moving into a persistent
run counter, and the next target entering immediately afterward.

### Phase 7 - Core Vertical-Slice Art, VFX, and Audio Refinement

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| ART-701 | Refine the main menu, gameplay, failure, and completion as one visual family | P0 | Blocked | Depends on ART-205 and stable Phase 5/6 layouts. |
| ART-702 | Establish one reusable target/knife rendering standard | P0 | Blocked | Materials, outline, lighting, hit state, and silhouettes are coherent. |
| ART-703 | Replace visibly unrelated core UI icons, panels, and fonts | P0 | Ready | Phase 2 identifies the replacements and supplies the approved UI kit. |
| ART-704 | Standardize target crack, hit, shatter, collision, collectible, and power-up VFX | P0 | Blocked | Effects share palette, timing, scale, and readability limits. |
| AUDIO-701 | Standardize throw, stick, collision, crack, shatter, reward, and UI sound families | P0 | Ready | Cues feel related and have a documented mix hierarchy. |
| ART-705 | Add missing state art: disabled, pressed, selected, owned, unavailable, success, and failure | P0 | Blocked | Depends on Phase 4 state definitions. |
| ART-706 | Validate all core art at gameplay size and over each retained background | P0 | Blocked | Knives and collision lanes remain readable on device. |
| ART-707 | Set sprite/import/compression/atlas standards for mobile | P1 | Done | Phase 2 locks Android compression, atlas, filtering, size, and memory rules after auditing 58 referenced textures. |
| ART-708 | Replace power-up button frames, icons, badges, and active/timer art | P0 | Blocked | Depends on UX-408; final Shatter and Chrono controls pass the replacement plan's gameplay-size, accessibility, provenance, and Android budget gates. |

### Phase 8 - Secondary Screens and Meta UX Consistency

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| STORE-801 | Explain unaffordable purchases through shared feedback | P0 | Blocked | Depends on Phase 4. |
| STORE-802 | Add clear purchase success, ownership, and equip transitions | P0 | Blocked | Depends on Phase 4 and core UI kit. |
| STORE-803 | Clarify horizontal scrolling and clipped additional cards | P1 | Ready | Players discover all offers without instruction. |
| MISSION-801 | Improve mission progress, claimability, and claim feedback | P1 | Blocked | Depends on Phase 4. |
| STAGE-801 | Clearly label placeholder worlds as development content | P1 | Ready | Placeholder content cannot be mistaken for final. |
| STAGE-802 | Fix locked/reward/button visual states in stage selection | P1 | Blocked | Depends on core UI states. |
| POPUP-801 | Replace development-looking mock-flow presentation in player-facing QA captures | P2 | Ready | QA/debug presentation cannot be confused with production UI. |
| UX-801 | Audit copy, spacing, hierarchy, safe areas, and back navigation on every secondary screen | P1 | Blocked | Depends on Phase 7 core visual language. |

### Phase 9 - Tutorial Integration, Full Journey, Device, and Release Validation

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| TUT-901 | Map the final minimum teaching sequence against the completed core UX | P0 | Blocked | Begins only after Phases 1-8 stabilize mechanics, progression, economy, feedback, and presentation. |
| TUT-902 | Decide first taught power-up and empty-inventory behavior | P0 | Blocked | Choose using final acquisition cost/path, pacing, and mechanic comprehension evidence. |
| TUT-903 | Implement truthful state-driven prompts and success-based completion flags | P0 | Blocked | Prompts query live state and complete only after the demonstrated action succeeds. |
| TUT-904 | Add tutorial skip, dismissal, interruption recovery, and replay/debug coverage | P1 | Blocked | Tutorial state cannot trap, silently skip, or corrupt a player journey. |
| TUT-905 | Validate final copy, gestures, focus, safe areas, and accessibility on device | P0 | Blocked | Uses final UI/art and representative device classes. |
| QA-901 | Run fresh-save first-ten-minutes playtest with real input | P0 | Blocked | Phases 1-8 complete. |
| QA-902 | Run progressed-save regression through store, missions, stages, and power-ups | P0 | Blocked | Phases 1-8 complete. |
| QA-903 | Test representative low/mid/high Android devices and target iPhone | P0 | Blocked | Device matrix approved. |
| QA-904 | Evaluate audio mix, haptics, touch latency, safe areas, thermals, and frame pacing | P0 | Blocked | Must be assessed on hardware. |
| QA-905 | Run production-build forbidden-content scan | P0 | Blocked | BUILD-103 implemented. |
| QA-906 | Run save interruption, offline, ad failure, and app-resume tests | P0 | Blocked | Relevant flows stable. |
| QA-907 | Record final screenshots, video, Console logs, telemetry, and known-issues list | P0 | Blocked | Evidence attached before approval. |

### Phase 10 - World Identity and Content Expansion

| ID | Work item | Priority | Status | Evidence / acceptance |
|---|---|---:|---|---|
| WORLD-1001 | Finalize world naming and campaign spine | P1 | Deferred | Starts after core vertical-slice approval. |
| WORLD-1002 | Define a mechanical and emotional identity for each world | P1 | Deferred | Every world changes more than its background. |
| WORLD-1003 | Produce missing Moss Sanctum through Void Gate art | P2 | Deferred | Uses the approved production kit and budgets. |
| WORLD-1004 | Replace duplicate W1/W2 composite entries with approved content | P1 | Deferred | No placeholder duplication in the release catalog. |
| WORLD-1005 | Validate every background/target against gameplay readability budgets | P0 | Deferred | Content cannot ship on theme alone. |

## Decisions Still Needed

| Decision | Needed by | Current recommendation |
|---|---|---|
| First power-up teaching path | Phase 9 | Decide against the finished mechanics and acquisition flow; do not lock it early. |
| Empty-inventory hint behavior | Phase 9 | Never point at an unusable action without an acquisition explanation. |
| Continue rules | Phase 5 | Keep the first failure low-pressure; delay aggressive ad/currency decisions. |
| Completion duration | Phase 6 | Start with 0.6-1.0 seconds and tune through real play. |
| Reward accumulator cashout | Phase 6 | Save rewards immediately; presentation may accumulate without risking loss. |
| Reduced motion and haptics | Phase 4 | Provide independent settings and never rely on shake alone for meaning. |

## Validation Standard

A task is not Done merely because code or art exists. Completion requires the applicable
evidence:

- real-input Unity playthrough;
- clean Console and compilation result;
- before/after screenshot or video for presentation work;
- telemetry/log evidence for timing, inventory, rewards, and state transitions;
- fresh-save and progressed-save coverage where behavior differs;
- representative-device validation for touch, audio, haptics, safe areas, and performance;
- restored test save or isolated test profile after validation.

## Update Protocol

When work is completed:

1. Update the work item status and evidence in this tracker.
2. Record any changed product decision under Approved Direction or Decisions Still Needed.
3. Add discovered work as a new ID instead of silently expanding an existing item.
4. Do not advance a phase until its exit gate in the companion roadmap is satisfied.
5. Keep later-world content Deferred until Phase 9 explicitly opens Phase 10.
